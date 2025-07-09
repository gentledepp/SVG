using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Svg.Interfaces;

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

        public Task GenerateTilesAsync(string sourceImagePath, Func<string, string, Task<Stream>> tileOutputStreamProvider, IProgress<int> progress = null, string backgroundColor = "#ffffff") {
            using var sourceImageStream = _fileSystem.OpenRead(sourceImagePath);
            return GenerateTilesAsync(sourceImageStream, tileOutputStreamProvider, progress,
                backgroundColor, 1);
        }

        public async Task GenerateTilesAsync(Stream sourceImageStream,
            Func<string, string, Task<Stream>> tileOutputStreamProvider, IProgress<int> progress = null, string backgroundColor = "#ffffff",
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
            for (int zoom = -1; zoom <= maxZoomLevel; zoom++)
            {
                progress.Report(progressInterval * zoom);

                int zoomLevelFactor = (int)Math.Pow(2, zoom);
                int zoomWidth = (int)Math.Ceiling(originalWidth / (double)zoomLevelFactor);
                int zoomHeight = (int)Math.Ceiling(originalHeight / (double)zoomLevelFactor);

                // Ensure the output directory for the current zoom level
                var zoomFolderName = $"z{zoom}";

                // Create all tiles in parallel
                for (int x = 0; x < (int)Math.Ceiling(zoomWidth / (double)tileSize); x++)
                {
                    for (int y = 0; y < (int)Math.Ceiling(zoomHeight / (double)tileSize); y++)
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

        private async Task CreateTileAsync(SKBitmap originalBitmap, int zoomLevelFactor, int tileSize, int x, int y,
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
                int srcX = x * tileSize * zoomLevelFactor;
                int srcY = y * tileSize * zoomLevelFactor;
                int srcWidth = Math.Min(tileSize * zoomLevelFactor, originalWidth - srcX);
                int srcHeight = Math.Min(tileSize * zoomLevelFactor, originalHeight - srcY);

                // Create the tile
                using (SKBitmap tileBitmap = new SKBitmap(tileSize, tileSize, SKColorType.Bgra8888, SKAlphaType.Premul))
                using (SKCanvas tileCanvas = new SKCanvas(tileBitmap))
                {
                    // Clear the tile to fully transparent
                    tileCanvas.Clear(SKColor.Parse(backgroundColor));

                    // Draw the scaled portion of the original image onto the tile
                    SKRect destRect = new SKRect(0, 0, srcWidth / (float)zoomLevelFactor,
                        srcHeight / (float)zoomLevelFactor);
                    SKRect srcRect = new SKRect(srcX, srcY, srcX + srcWidth, srcY + srcHeight);
                    tileCanvas.DrawBitmap(originalBitmap, srcRect, destRect);


                    using (var memoryStream = new MemoryStream())
                    using (var skStream = new SKManagedWStream(memoryStream))
                    {
                        // Encode the bitmap to the memory stream
                        tileBitmap.Encode(skStream, SKEncodedImageFormat.Jpeg, 100); // Save with 90% quality

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
    }
}