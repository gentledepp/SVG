using Avalonia.Controls;
using SkiaSharp;
using Svg.Interfaces;
using Svg.Platform;
using Svg.Transforms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Svg.DeepZoom
{

    /// <summary>
    /// code reference from project: https://github.com/gentledepp/POC_TilingSample
    /// </summary>
    public class TileGenerator : ITileGenerator
    {

        private readonly IFileSystem _fileSystem;

        public TileGenerator()
        {
            _fileSystem = SvgEngine.Resolve<IFileSystem>();
        }

        public Task GenerateTilesAsync(string sourceImagePath,
            Func<string, string, Task<Stream>> tileOutputStreamProvider, IProgress<int> progress = null,
            string backgroundColor = "#ffffff")
        {
            var sourceImageStream = _fileSystem.OpenRead(sourceImagePath);
            return GenerateTilesAsync(sourceImageStream, tileOutputStreamProvider, progress,
                backgroundColor, 1);
        }

        public async Task GenerateTilesAsync(Stream sourceImageStream,
            Func<string, string, Task<Stream>> tileOutputStreamProvider, IProgress<int> progress = null,
            string backgroundColor = "#ffffff",
            int maxParallelTasks = -1)
        {
            progress ??= new Progress<int>();
            progress.Report(0);
            const int tileSize = TileConstants.TileSize;
            using SKBitmap originalBitmap = SKBitmap.Decode(sourceImageStream);
            int originalWidth = originalBitmap.Width;
            int originalHeight = originalBitmap.Height;

            // Determine the maximum zoom level needed
            int maxZoomLevel =
                (int)Math.Ceiling(Math.Log(Math.Max(originalWidth, originalHeight) / (double)tileSize, 2));

            // List to keep track of all tasks for generating tiles
            var tileCreationTasks = new List<Task>();

            var @lock = new SemaphoreSlim(maxParallelTasks <= 0 ? int.MaxValue : maxParallelTasks);

            var progressInterval = 100 / maxZoomLevel;

            // Loop over each zoom level
            for (int zoom = 0; zoom <= maxZoomLevel; zoom++)
            {
                progress.Report(progressInterval * zoom);

                double zoomLevelFactor = Math.Pow(2, zoom);
                double zoomWidth = Math.Ceiling(originalWidth / zoomLevelFactor);
                double zoomHeight = Math.Ceiling(originalHeight / zoomLevelFactor);

                // Ensure the output directory for the current zoom level
                var zoomFolderName = $"z{zoom}";

                // Create all tiles in parallel
                for (int x = 0; x < (int)Math.Ceiling(zoomWidth / tileSize); x++)
                {
                    for (int y = 0; y < (int)Math.Ceiling(zoomHeight / tileSize); y++)
                    {
                        // Create a local copy of the variables to avoid closure issues
                        int localX = x;
                        int localY = y;

                        if (maxParallelTasks == 1)
                        {
                            double progressIncrements = (zoom + 1) / maxZoomLevel * 100.0;
                            progress.Report((int)progressIncrements);

                            await CreateTileAsync(originalBitmap, zoomLevelFactor, tileSize, localX, localY,
                                zoomFolderName, @lock, progress, tileOutputStreamProvider, backgroundColor);
                        }
                        else
                        {
                            // Add a task to create each tile
                            tileCreationTasks.Add(Task.Run(async () =>
                            {
                                await CreateTileAsync(originalBitmap, zoomLevelFactor, tileSize, localX, localY,
                                    zoomFolderName, @lock, progress, tileOutputStreamProvider, backgroundColor);
                            }));
                        }
                    }
                }
            }

            // Wait for all tasks to complete
            await Task.WhenAll(tileCreationTasks);
        }

        private async Task CreateTileAsync(SKBitmap originalBitmap, double zoomLevelFactor, int tileSize, int x, int y,
            string zoomDir, SemaphoreSlim @lock, IProgress<int> progress = null,
            Func<string, string, Task<Stream>> streamProvider = null, string backgroundColor = "#ffffff")
        {
            await @lock.WaitAsync();

            var p = streamProvider ?? DefaultStreamProvider;

            try
            {
                int originalWidth = originalBitmap.Width;
                int originalHeight = originalBitmap.Height;

                // Calculate the source rectangle in the original image at this zoom level
                double srcX = x * tileSize * zoomLevelFactor;
                double srcY = y * tileSize * zoomLevelFactor;
                double srcWidth = Math.Min(tileSize * zoomLevelFactor, originalWidth - srcX);
                double srcHeight = Math.Min(tileSize * zoomLevelFactor, originalHeight - srcY);

                // Create the tile
                using (SKBitmap tileBitmap = new SKBitmap(tileSize, tileSize, SKColorType.Bgra8888, SKAlphaType.Premul))
                using (SKCanvas tileCanvas = new SKCanvas(tileBitmap))
                {
                    // Clear the tile to fully transparent
                    tileCanvas.Clear(SKColor.Parse(backgroundColor));

                    // Draw the scaled portion of the original image onto the tile
                    SKRect destRect = new SKRect(0, 0, (float)(srcWidth / zoomLevelFactor),
                        (float)(srcHeight / zoomLevelFactor));
                    SKRect srcRect = new SKRect((float)srcX, (float)srcY, (float)(srcX + srcWidth),
                        (float)(srcY + srcHeight));
                    tileCanvas.DrawBitmap(originalBitmap, srcRect, destRect);


                    using (var memoryStream = new MemoryStream())
                    using (var skStream = new SKManagedWStream(memoryStream))
                    {
                        // Encode the bitmap to the memory stream
                        tileBitmap.Encode(skStream, SKEncodedImageFormat.Png, 100);

                        // Reset the memory stream position to the beginning
                        memoryStream.Position = 0;

                        // Write the memory stream to the file asynchronously
                        using (var fs = await p(zoomDir, $"y{y}_x{x}.png"))
                        {
                            await memoryStream.CopyToAsync(fs);
                        }
                    }
                }
            }
            finally
            {
                @lock.Release(1);
            }
        }

        public Task<Stream> DefaultStreamProvider(string zoomDir, string tileFileName)
        {
            // Save the tile to disk
            var di = new DirectoryInfo(zoomDir);
            if (!di.Exists) di.Create();


            string tilePath = _fileSystem.PathCombine(zoomDir, tileFileName);
            var fs = _fileSystem.OpenWrite(tilePath);
            return Task.FromResult<Stream>(fs);
        }

        public async Task<SvgDocument> GenerateSvgDocumentWithTilesAsync(
            Bitmap svgBitmap,
            IProgress<int> progress = null,
            string backgroundColor = "#ffffff",
            int maxParallelTasks = -1)
        {
            progress ??= new Progress<int>();
            progress.Report(0);
            const int tileSize = TileConstants.TileSize;

            using SKBitmap originalBitmap = ((SkiaBitmap)svgBitmap).Image.Copy(SKColorType.Bgra8888);

            int originalWidth = originalBitmap.Width;
            int originalHeight = originalBitmap.Height;
            // Before any tile creation, right after skBitmap.Image:
            Debug.WriteLine($"ByteCount={originalBitmap.ByteCount}, RowBytes={originalBitmap.RowBytes}");
            Debug.WriteLine($"GetPixels ptr is zero: {originalBitmap.GetPixels() == IntPtr.Zero}");
            var p = originalBitmap.GetPixel(originalWidth / 2, originalHeight / 2);
            Debug.WriteLine($"Source center pixel: R={p.Red} G={p.Green} B={p.Blue} A={p.Alpha}");
           
            var newDoc = new SvgDocument();
            newDoc.Width = new SvgUnit(SvgUnitType.Pixel, originalWidth);
            newDoc.Height = new SvgUnit(SvgUnitType.Pixel, originalHeight);
            newDoc.ViewBox = null;

            // Determine the maximum zoom level needed
            int maxZoomLevel =
                (int)Math.Ceiling(Math.Log(Math.Max(originalWidth, originalHeight) / (double)tileSize, 2));

            // List to keep track of all tasks for generating tiles
            var tileCreationTasks = new List<Task>();

            var @lock = new SemaphoreSlim(maxParallelTasks <= 0 ? int.MaxValue : maxParallelTasks);

            var progressInterval = 100 / maxZoomLevel;

            // Loop over each zoom level
            for (int zoom = 0; zoom <= maxZoomLevel; zoom++)
            {
                progress.Report(progressInterval * zoom);

                double zoomLevelFactor = Math.Pow(2, zoom);
                double zoomWidth = Math.Ceiling(originalWidth / zoomLevelFactor);
                double zoomHeight = Math.Ceiling(originalHeight / zoomLevelFactor);

                // Create all tiles in parallel
                for (int x = 0; x < (int)Math.Ceiling(zoomWidth / tileSize); x++)
                {
                    for (int y = 0; y < (int)Math.Ceiling(zoomHeight / tileSize); y++)
                    {
                        // Create a local copy of the variables to avoid closure issues
                        int localX = x;
                        int localY = y;

                        if (maxParallelTasks == 1)
                        {
                            double progressIncrements = (zoom + 1) / maxZoomLevel * 100.0;
                            progress.Report((int)progressIncrements);

                            await CreateTileAsync(originalBitmap, newDoc, zoomLevelFactor, tileSize, localX, localY,
                                "" + zoom,
                                @lock, progress, backgroundColor);
                        }
                        else
                        {
                            // Add a task to create each tile
                            tileCreationTasks.Add(CreateTileAsync(originalBitmap, newDoc, zoomLevelFactor, tileSize, localX, localY,
                                    "" + zoom, @lock, progress, backgroundColor));
                        }
                    }
                }
            }

            // Wait for all tasks to complete
            await Task.WhenAll(tileCreationTasks);

            return newDoc;
        }

        private async Task CreateTileAsync(SKBitmap originalBitmap, SvgDocument documentOutput, double zoomLevelFactor,
            int tileSize, int x, int y,
            string zoomLevel, SemaphoreSlim @lock, IProgress<int> progress = null, string backgroundColor = "#ffffff")
        {
            await @lock.WaitAsync();

            try
            {
                int originalWidth = originalBitmap.Width;
                int originalHeight = originalBitmap.Height;

                Debug.Assert(originalBitmap.Width > 0 && originalBitmap.Height > 0, "Source bitmap is empty!");
                Debug.Assert(!originalBitmap.GetPixel(originalBitmap.Width / 2, originalBitmap.Height / 2).Equals(SKColors.White),
                    "Source bitmap appears blank!");

                // Calculate the source rectangle in the original image at this zoom level
                double srcX = x * tileSize * zoomLevelFactor;
                double srcY = y * tileSize * zoomLevelFactor;
                double srcWidth = Math.Min(tileSize * zoomLevelFactor, originalWidth - srcX);
                double srcHeight = Math.Min(tileSize * zoomLevelFactor, originalHeight - srcY);

                // Create the tile
                using (SKBitmap tileBitmap = new SKBitmap(tileSize, tileSize, SKColorType.Bgra8888, SKAlphaType.Premul))
                using (SKCanvas tileCanvas = new SKCanvas(tileBitmap))
                {
                    // Clear the tile to fully transparent
                    tileCanvas.Clear(SKColor.Parse(backgroundColor));

                    // Draw the scaled portion of the original image onto the tile
                    SKRect destRect = new SKRect(0, 0, (float)(srcWidth / zoomLevelFactor),
                        (float)(srcHeight / zoomLevelFactor));
                    SKRect srcRect = new SKRect((float)srcX, (float)srcY, (float)(srcX + srcWidth),
                        (float)(srcY + srcHeight));
                    tileCanvas.DrawBitmap(originalBitmap, srcRect, destRect);
#if DEBUG
                    using var paint = new SKPaint();
                    paint.StrokeWidth = 1;
                    paint.Color = SKColors.Red;
                    paint.IsStroke = true;
                    tileCanvas.DrawRect(destRect.Left, destRect.Top, destRect.Width, destRect.Height, paint);
#endif
                    // Add this debug check:
                    var testPixel = tileBitmap.GetPixel(tileSize / 2, tileSize / 2);
                    Debug.WriteLine($"[Tile {x},{y} z{zoomLevel}] pixel after draw: R={testPixel.Red} G={testPixel.Green} B={testPixel.Blue}");
                    Debug.WriteLine($"[Source bitmap] ColorType={originalBitmap.ColorType}, AlphaType={originalBitmap.AlphaType}");


                    // Encode the bitmap to the memory stream
                    using var skData = tileBitmap.Encode(SKEncodedImageFormat.Jpeg, 100);
                    var base64 = Convert.ToBase64String(skData.ToArray());
                    var dataUri = $"data:image/jpeg;base64,{base64}";
                    var imgElement = new SvgImage
                    {
                        X = new SvgUnit(SvgUnitType.Pixel, (float)srcX),
                        Y = new SvgUnit(SvgUnitType.Pixel, (float)srcY),
                        Width = new SvgUnit(SvgUnitType.Pixel, (float)srcWidth),
                        Height = new SvgUnit(SvgUnitType.Pixel, (float)srcHeight),
                        Href = dataUri,
                        ID = $"zoom{zoomLevel}_y{y}_x{x}",
                    };

                    lock (documentOutput)
                    {
                        documentOutput.Children.Add(imgElement);
                    }
                }
            }
            finally
            {
                @lock.Release(1);
            }
        }
    }
}