using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileSystemHelper.Platforms;
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
                }, o => ChooseBackgroundEnabled, iconName: "ic_insert_photo.svg"),
                new ToolCommand(this, "Choose svg/image to tile render", async ob =>
                {
                    var xa = Canvas.ScreenWidth;
                    var y = Canvas.ScreenHeight;
                    var imgs = SvgEngine.TryResolve<IPickImageService>();
                    var fileSystem = SvgEngine.TryResolve<IFileSystem>();
                    

                    ImagePath = await imgs.PickImagePathAsync(Canvas.ScreenWidth);
                    if (ImagePath == null) return;

                    using var newBmp = ScaleAndPlaceBackground(ImagePath, 3508, 2480); // A3 example
                    
                    if(File.Exists(Path.Combine(fileSystem.GetDefaultStoragePath(),ImagePath)))
                        File.Delete(Path.Combine(fileSystem.GetDefaultStoragePath(),ImagePath));

                    using (var file = fileSystem.OpenWrite(Path.Combine(fileSystem.GetDefaultStoragePath(),ImagePath)))
                    {
                        newBmp.SavePng(file);
                    }


                    var gen = new TileGenerator();
                    var outPutZiFile = Path.Combine(fileSystem.GetDefaultStoragePath(), "TilesStream.zip");
                   if(File.Exists(outPutZiFile))
                       File.Delete(outPutZiFile);
                   using (var zipFileStream = fileSystem.OpenWrite(outPutZiFile))
                   {
                       using var archive = new ZipArchive(zipFileStream, ZipArchiveMode.Create);

                       var streamProvider = (string folderName, string fileName) =>
                       {
                           var entry = archive.CreateEntry(Path.Combine(folderName, fileName));
                           return Task.FromResult<Stream>(entry.Open());
                       };

                       await gen.GenerateTilesAsync(Path.Combine(fileSystem.GetDefaultStoragePath(), ImagePath),
                           streamProvider);
                   }

                   if (ImagePath == null) return;
                    //Canvas.Constraints = RectangleF.Create(0, 0, size.Width, size.Height);
                    var image = Canvas.Document.AddImageInBackground(ImagePath);
                    image.Href = outPutZiFile;
                    Canvas.FireInvalidateCanvas();
                    Canvas.FireToolCommandsChanged();

                }, o => ChooseBackgroundEnabled, iconName: "ic_insert_photo.svg"),
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
                }, o => ChooseBackgroundEnabled && Canvas.Document.Children.Any(x => x.CustomAttributes.ContainsKey(BackgroundCustomAttributeKey)), iconName: "ic_delete.svg")
            };

            if (ImagePath != null)
            {
                PlaceImage(ImagePath);
            }
        }

        private Bitmap ScaleAndPlaceBackground(string path, int newWidth, int newHeight)
        {
            var doc = SvgDocument
                .Open(
                    "C:\\Users\\zepr2\\AppData\\Local\\Packages\\b59e58e0-ad0a-44f6-8ad5-b3e2cc51b8b8_n40svhjkyhv9a\\LocalState\\864a895d-ba0d-4cbf-973a-4cea2598289a.svg");

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

                //Canvas.Constraints = RectangleF.Create(0, 0, size.Width, size.Height);
                Canvas.FireInvalidateCanvas();
                Canvas.FireToolCommandsChanged();
            }
            catch (IOException)
            {
            }
        }

        public static readonly string ChooseBackgroundEnabledKey = @"choosebackgroundenabled";
        public static readonly string ImagePathKey = @"imagepath";

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
