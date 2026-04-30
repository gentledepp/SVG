using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;
using Svg.DeepZoom;
using Svg.Editor.Extensions;
using Svg.Editor.Interfaces;
using Svg.Interfaces;
using Svg.Transforms;

namespace Svg.Editor.Tools
{
    public class PlaceAsBackgroundTool : UndoableToolBase
    {
        public PlaceAsBackgroundTool(IDictionary<string, object> properties, IUndoRedoService undoRedoService) : base("Background Image", properties, undoRedoService)
        {
            IconName = "ic_insert_photo.svg";
        }
	    public override bool CanSerialize => false;

	    public override async Task Initialize(ISvgDrawingCanvas ws)
        {
            await base.Initialize(ws);

            Commands = new List<IToolCommand>
            {
                new ToolCommand(this, "Choose background image", async o =>
                {
                    var imgs = SvgEngine.TryResolve<IPickImageService>();
                    if (imgs == null) return;

                    ImagePath = await imgs.PickImagePathAsync(Canvas.ScreenWidth);
                    if (ImagePath == null) return;
                    PlaceImage(ImagePath);
                }, o => ChooseBackgroundEnabled, iconName: "ic_insert_photo.svg", description: LocalizationService.GetString("Svg.Editor.BackgroundTool.ChooseBackgroundImage.Description")),
                new ToolCommand(this, "Choose image / svg / zip to tile render", async ob =>
                {
                    var imgs = SvgEngine.TryResolve<IPickImageService>();
                    var fileSystem = SvgEngine.TryResolve<IFileSystem>();
                    if (imgs == null || fileSystem == null) return;

                    var pickedPath = await imgs.PickImageOrZipPathAsync(Canvas.ScreenWidth);
                    if (pickedPath == null) return;

                    SvgImage svgImage;

                    if (pickedPath.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                    {
                        int estimatedWidth = 0, estimatedHeight = 0;
                        using (var zipStream = File.OpenRead(pickedPath))
                        using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Read))
                        {
                            // Prefer exact dimensions written by TileGenerator.
                            var dimEntry = archive.GetEntry("dimensions.json");
                            if (dimEntry != null)
                            {
                                using var dimStream = dimEntry.Open();
                                using var reader = new StreamReader(dimStream, Encoding.UTF8);
                                var json = reader.ReadToEnd();
                                var wm = System.Text.RegularExpressions.Regex.Match(json, @"""width""\s*:\s*(\d+)");
                                var hm = System.Text.RegularExpressions.Regex.Match(json, @"""height""\s*:\s*(\d+)");
                                if (wm.Success && hm.Success)
                                {
                                    estimatedWidth  = int.Parse(wm.Groups[1].Value);
                                    estimatedHeight = int.Parse(hm.Groups[1].Value);
                                }
                            }

                            // Fallback: scan z0 tiles (handles zips without metadata).
                            // Normalise path separators — Windows ZipArchive stores entries with backslash.
                            if (estimatedWidth == 0 || estimatedHeight == 0)
                            {
                                int maxTileX = -1, maxTileY = -1;
                                foreach (var entry in archive.Entries)
                                {
                                    if (!entry.FullName.Replace('\\', '/').StartsWith("z0/", StringComparison.OrdinalIgnoreCase))
                                        continue;
                                    var name = Path.GetFileNameWithoutExtension(entry.Name);
                                    var parts = name.TrimStart('y').Split(new[] { "_x" }, StringSplitOptions.None);
                                    if (parts.Length == 2 &&
                                        int.TryParse(parts[0], out var ty) &&
                                        int.TryParse(parts[1], out var tx))
                                    {
                                        if (tx > maxTileX) maxTileX = tx;
                                        if (ty > maxTileY) maxTileY = ty;
                                    }
                                }
                                if (maxTileX >= 0 && maxTileY >= 0)
                                {
                                    estimatedWidth  = (maxTileX + 1) * 256;
                                    estimatedHeight = (maxTileY + 1) * 256;
                                }
                            }
                        }
                        svgImage = new SvgImage { Width = estimatedWidth, Height = estimatedHeight, Href = pickedPath };
                    }
                    else
                    {
                        var fullSourcePath = Path.IsPathRooted(pickedPath)
                            ? pickedPath
                            : Path.Combine(fileSystem.GetDefaultStoragePath(), pickedPath);

                        var outputZipFile = Path.Combine(fileSystem.GetDefaultStoragePath(), "TilesBackground.zip");
                        if (File.Exists(outputZipFile))
                            File.Delete(outputZipFile);

                        var gen = new TileGenerator();
                        int imageWidth, imageHeight;

                        using (var zipFileStream = fileSystem.OpenWrite(outputZipFile))
                        using (var archive = new ZipArchive(zipFileStream, ZipArchiveMode.Create))
                        {
                            var streamProvider = (string folderName, string fileName) =>
                            {
                                var entry = archive.CreateEntry(Path.Combine(folderName, fileName));
                                return Task.FromResult<Stream>(entry.Open());
                            };

                            if (pickedPath.EndsWith(".svg", StringComparison.OrdinalIgnoreCase))
                            {
                                var doc = SvgDocument.Open(fullSourcePath);
                                const int a1Width = 7016;   // A1 at 300 DPI
                                const int a1Height = 9933;
                                var dims = doc.GetDimensions();
                                var targetW = dims.Width;
                                var targetH = dims.Height;
                                if (targetW < a1Width && targetH < a1Height)
                                {
                                    // Scale up to fit A1, preserving aspect ratio.
                                    var scale = Math.Min(a1Width / targetW, a1Height / targetH);
                                    targetW *= scale;
                                    targetH *= scale;
                                }
                                imageWidth = (int)targetW;
                                imageHeight = (int)targetH;
                                await Task.Run(()=> gen.GenerateTilesAsync(doc, imageWidth, streamProvider));
                            }
                            else
                            {
                                using var sourceStream = File.OpenRead(fullSourcePath);
                                using var codec = SKCodec.Create(sourceStream);
                                imageWidth = codec.Info.Width;
                                imageHeight = codec.Info.Height;
                                sourceStream.Position = 0;
                                await Task.Run(() => gen.GenerateTilesAsync(sourceStream, streamProvider));
                            }
                        }

                        svgImage = new SvgImage { Width = imageWidth, Height = imageHeight, Href = outputZipFile };
                    }

                    // Remove any previously placed tiled image (mirrors PlaceImage cleanup pattern).
                    var children = Canvas.Document.Children;
                    var formerTileRender = children.FirstOrDefault(x => x.CustomAttributes.ContainsKey(TileRenderAttributeKey));
                    if (formerTileRender != null)
                    {
                        children.Remove(formerTileRender);
                        formerTileRender.Dispose();
                    }

                    svgImage.CustomAttributes.Add(TileRenderAttributeKey, "");
                    svgImage.CustomAttributes.Add(BackgroundCustomAttributeKey, "");
                    children.Add(svgImage);
                    Canvas.FireInvalidateCanvas();
                    Canvas.FireToolCommandsChanged();

                }, o => ChooseBackgroundEnabled, iconName: "ic_insert_photo.svg", description: LocalizationService.GetString("Svg.Editor.BackgroundTool.ChooseTileRender.Description")),
                new ToolCommand(this, "Remove background image", o =>
                {
                    var children = Canvas.Document.Children;
                    var background = children.FirstOrDefault(x => x.CustomAttributes.ContainsKey(BackgroundCustomAttributeKey));
                    if (background != null)
                    {
                        children.Remove(background);
                        background.Dispose();

                        Canvas.Constraints = null;

                        ImagePath = null;

                        Canvas.FireInvalidateCanvas();
                        Canvas.FireToolCommandsChanged();
                    }
                }, o => ChooseBackgroundEnabled && Canvas.Document.Children.Any(x => x.CustomAttributes.ContainsKey(BackgroundCustomAttributeKey)), iconName: "ic_delete.svg", description: LocalizationService.GetString("Svg.Editor.BackgroundTool.Remove.Description"))
            };

            if (ImagePath != null)
            {
                PlaceImage(ImagePath);
            }
        }

        private Bitmap ScaleAndPlaceBackground(string path, int newWidth, int newHeight)
        {
            var doc = SvgDocument
                .Open(ImagePath);

            //var bImage = doc.Children.OfType<SvgImage>().FirstOrDefault();

            //// readjust svg doc
            var docWidth = doc.Width;
            var docHeight = doc.Height;
            var scale = Math.Max(newHeight / docHeight, newWidth / docWidth);

            doc.Width = newWidth;
                doc.Height = newHeight;
                doc.ViewBox = null;
                doc.Transforms.Add(new SvgScale(scale));

                return doc.DrawDocument();
        }

        private void PlaceImage(string path)
        {
            try
            {
                var children = Canvas.Document.Children;

                // remove already placed background
                var formerBackground = children.FirstOrDefault(x => x.CustomAttributes.ContainsKey(BackgroundCustomAttributeKey));
                if (formerBackground != null)
                {
                    children.Remove(formerBackground);
                    formerBackground.Dispose();
                }

                var image = Canvas.Document.AddImageInBackground(path);
                // add custom icl attributes
                image.CustomAttributes.Add(BackgroundCustomAttributeKey, "");
                image.AddConstraints(NoSnappingConstraint, NoFillConstraint, NoStrokeConstraint);
                // add constraints to the canvas
                var size = image.GetImageSize();

                Canvas.Constraints = RectangleF.Create(0, 0, size.Width, size.Height);
                Canvas.FireInvalidateCanvas();
                Canvas.FireToolCommandsChanged();
            }
            catch (IOException)
            {
            }
        }

        public static readonly string ChooseBackgroundEnabledKey = @"choosebackgroundenabled";
        public static readonly string ImagePathKey = @"imagepath";
        private const string TileRenderAttributeKey = "tilerender";

        public bool ChooseBackgroundEnabled
        {
            get
            {
                if (SvgEngine.TryResolve<IPickImageService>() == null)
                    return false;

                object chooseBackgroundEnabled;
                return Properties.TryGetValue(ChooseBackgroundEnabledKey, out chooseBackgroundEnabled) && Convert.ToBoolean(chooseBackgroundEnabled);
            }
            set { Properties[ChooseBackgroundEnabledKey] = value; }
        }

        public string ImagePath
        {
            get
            {
                object imagePath;
                Properties.TryGetValue(ImagePathKey, out imagePath);
                return imagePath as string;
            }
            set { Properties[ImagePathKey] = value; }
        }
    }
}
