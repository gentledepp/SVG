using SkiaSharp;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System;
using System.Linq;
using Svg.Interfaces;

namespace Svg.DeepZoom
{
    public class TileRenderer : ITileRenderer
    {
        private readonly IFileSystem _fileSystem;

        private readonly ITileCache _cache;

        private readonly TileCacheOptions _options;
        public int Width { get; private set; }
        public int Height { get; private set; }
        private int MinTileSize { get; set; } = TileConstants.TileSize / 2 ;

        public void SetDimensions(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public TileRenderer(int width, int height) : this(width, height, null)
        {
        }

        public TileRenderer(int width, int height, TileCacheOptions options)
        {
            Width = width;
            Height = height;
            _options = options;
            if(options != null)
            {
                _cache = new TileCache(options);
            }
            _fileSystem = SvgEngine.Resolve<IFileSystem>();
        }

        public TileRenderer(int width, int height, int minTileSize, TileCacheOptions options)
        {
            Width = width;
            Height = height;
            _options = options;
            if (options != null)
            {
                _cache = new TileCache(options);
            }
            _fileSystem = SvgEngine.Resolve<IFileSystem>();
            MinTileSize = minTileSize;
        }


        public void RenderBitmap(string tileFolderPath, string outputPath, float x, float y, float zoomFactor = 1)
        {
            var tileProvider = new Func<string, string, Stream>((folder, fileName) =>
                LoadTileStream(_fileSystem.PathCombine(tileFolderPath, folder), fileName));

            using var tileBitmap = RenderBitmap(tileProvider, x, y, zoomFactor);

            // Save the tile as a PNG file
            using var image = SKImage.FromBitmap(tileBitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            using var output = _fileSystem.OpenWrite(outputPath);
            data.SaveTo(output);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tileFolderPath"></param>
        /// <param name="offsetX">Horizontal offset in original size pixels</param>
        /// <param name="offsetY">Vertical offset in original size pixels</param>
        /// <param name="zoomFactor"> Example zoom factor: 1.0 means fully zoomed in at highest detail (z0), 0.5 means one level zoomed out (z1), etc.</param>
        /// <returns></returns>
        public SKBitmap RenderBitmap(Func<string, string, Stream> tileProvider, float offsetX, float offsetY,
            float zoomFactor = 1)
        {
            var tileSize = TileConstants.TileSize;
            SKBitmap bitmap = new SKBitmap(Width, Height);
            using var canvas = new SKCanvas(bitmap);

            // Calculate the appropriate zoom level from the zoom factor
            // Higher zoomFactor corresponds to lower zoom level folder (z0, z1, etc.)
            int zoomLevel =
                Math.Max((int)Math.Floor(Math.Log(1f / zoomFactor, 2)), 0); // Correct zoom level calculation

            // Calculate size of tiles
            var tileSizeAtZoom = tileSize * zoomFactor * (int)Math.Pow(2, zoomLevel);

            //if tile is too small we increase zoomLevel to render less
            if (tileSizeAtZoom < MinTileSize)
            {
                zoomLevel += 1;
                tileSizeAtZoom = tileSize * zoomFactor * (int)Math.Pow(2, zoomLevel);
            }

            var widthAtZoom = canvas.LocalClipBounds.Width / zoomFactor;
            var heightAtZoom = canvas.LocalClipBounds.Height / zoomFactor;

            int startTileX = (int)Math.Floor(-offsetX / tileSizeAtZoom);
            int endTileX = (int)Math.Ceiling((-offsetX + widthAtZoom) / tileSizeAtZoom);
            int startTileY = (int)Math.Floor(-offsetY / tileSizeAtZoom);
            int endTileY = (int)Math.Ceiling((-offsetY + heightAtZoom) / tileSizeAtZoom);


            int count = 0;
            // Loop through the visible range of tiles and draw them
            for (int tileX = startTileX; tileX <= endTileX; tileX++)
            {
                for (int tileY = startTileY; tileY <= endTileY; tileY++)
                {
                    // Load the tile bitmap, not disposing bitmap because its cached
                    var tileBitmap = LoadTile($"z{zoomLevel}", $"y{tileY}_x{tileX}.png", tileProvider);

                    if (tileBitmap != null)
                    {
                        // Calculate the position to draw the tile on the canvas
                        float drawX = offsetX + tileX * tileSizeAtZoom;
                        float drawY = offsetY + tileY * tileSizeAtZoom;

                        var area = new SKRect(drawX, drawY, drawX + tileSizeAtZoom, drawY + tileSizeAtZoom);

                        bool isVisible = canvas.LocalClipBounds.IntersectsWith(area);
                        if (isVisible)
                        {
                            try
                            {
                                // Draw the tile on the canvas
                                canvas.DrawBitmap(tileBitmap, area);

                                count++;

#if DEBUG
                                var paint = new SKPaint();
                                paint.Color = SKColors.Red;
                                paint.StrokeWidth = 1;
                                paint.IsStroke = true;
                                canvas.DrawRect(area.Left, area.Top, area.Width, area.Height, paint);
#endif
                            }
                            finally
                            {
                                if (_cache is null)
                                    tileBitmap.Dispose();
                            }
                        }
                    }
                }
            }

            SvgEngine.Logger.Warn("tiles rendered:" + count);
            SvgEngine.Logger.Warn("zoom:" + zoomFactor);
            return bitmap;
        }

        public async Task RenderBitmapAsync(string tileFolderPath, string outputPath, float x, float y,
            float zoomFactor = 1)
        {

            var tileProvider = new Func<string, string, Task<Stream>>((folder, fileName) =>
                LoadTileStreamAsync(_fileSystem.PathCombine(tileFolderPath, folder), fileName));

            using var tileBitmap = await RenderBitmapAsync(tileProvider, x, y, zoomFactor);


            // Save the tile as a PNG file
            using var image = SKImage.FromBitmap(tileBitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            using var output = _fileSystem.OpenWrite(outputPath);
            data.SaveTo(output);
        }

        public async Task<SKBitmap> RenderBitmapAsync(Func<string, string, Task<Stream>> tileProvider, float offsetX,
            float offsetY, float zoomFactor = 1)
        {
            var tileSize = TileConstants.TileSize;
            SKBitmap bitmap = new SKBitmap(Width, Height);
            using var canvas = new SKCanvas(bitmap);

            // Calculate the appropriate zoom level from the zoom factor
            // Higher zoomFactor corresponds to lower zoom level folder (z0, z1, etc.)
            int zoomLevel = Math.Max((int)Math.Floor(Math.Log(1f / zoomFactor, 2)), 0); // Correct zoom level calculation

            // Calculate size of tiles
            var tileSizeAtZoom = tileSize * zoomFactor * (int)Math.Pow(2, zoomLevel);

            //if tile is too small we increase zoomLevel to render less
            if (tileSizeAtZoom < MinTileSize)
            {
                zoomLevel += 1;
                tileSizeAtZoom = tileSize * zoomFactor * (int)Math.Pow(2, zoomLevel);
            }

            var widthAtZoom = canvas.LocalClipBounds.Width / zoomFactor;
            var heightAtZoom = canvas.LocalClipBounds.Height / zoomFactor;

            int startTileX = (int)Math.Floor(-offsetX / tileSizeAtZoom);
            int endTileX = (int)Math.Ceiling((-offsetX + widthAtZoom) / tileSizeAtZoom);
            int startTileY = (int)Math.Floor(-offsetY / tileSizeAtZoom);
            int endTileY = (int)Math.Ceiling((-offsetY + heightAtZoom) / tileSizeAtZoom);

            // List to hold all tasks for rendering tiles
            var tileLoadTasks = new List<Task<(float x, float y, SKBitmap tileBitmap)>>();

            // Loop through the visible range of tiles and render them asynchronously
            for (int tileX = startTileX; tileX <= endTileX; tileX++)
            {
                for (int tileY = startTileY; tileY <= endTileY; tileY++)
                {
                    // Local copies of tile indices for use inside the Task
                    int localTileX = tileX;
                    int localTileY = tileY;

                    // Start a new task to load and render each tile
                    tileLoadTasks.Add(Task.Run(async () =>
                    {
                        var bmp = await LoadTileAsync($"z{zoomLevel}", $"y{localTileY}_x{localTileX}.png",
                            tileProvider);
                        // Calculate the position to draw the tile on the canvas
                        float drawX = offsetX + localTileX * tileSizeAtZoom;
                        float drawY = offsetY + localTileY * tileSizeAtZoom;

                        return (x: drawX, y: drawY, tileBitmap: bmp);
                    }));
                }
            }

            while (tileLoadTasks.Any())
            {
                var tileLoadTask = await Task.WhenAny(tileLoadTasks);
                tileLoadTasks.Remove(tileLoadTask);

                var (x, y, tileBitmap) = await tileLoadTask;

                if (tileBitmap != null)
                {
                    try
                    {
                        // Ensure the draw area is within the visible portion of the canvas
                        var area = new SKRect(x, y, x + tileSizeAtZoom, y + tileSizeAtZoom);
                        
                        bool isVisible = canvas.LocalClipBounds.IntersectsWith(area);
                        if (isVisible)
                        {
                            // Draw the tile on the canvas
                            canvas.DrawBitmap(tileBitmap, area);

                        }
                    }
                    finally
                    {
                        if (_cache is null)
                            tileBitmap.Dispose();
                    }
                }
            }

            return bitmap;
        }

        private async Task<Stream> LoadTileStreamAsync(string zoomFolderName, string tileFileName)
        {
            var tilePath = _fileSystem.PathCombine(zoomFolderName, tileFileName);

            if (!_fileSystem.FileExists(tilePath))
            {
                return null;
            }

            using FileStream fs = new FileStream(tilePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096,
                useAsync: true);
            var memoryStream = new MemoryStream();

            // Asynchronously copy the file content to the memory stream
            await fs.CopyToAsync(memoryStream);

            // Decode the bitmap from the memory stream
            memoryStream.Position = 0;
            return memoryStream;
        }

        private Stream LoadTileStream(string zoomFolderName, string tileFileName)
        {
            var tilePath = _fileSystem.PathCombine(zoomFolderName, tileFileName);

            if (!_fileSystem.FileExists(tilePath))
            {
                return null;
            }

            using FileStream fs = new FileStream(tilePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096,
                useAsync: true);
            var memoryStream = new MemoryStream();

            fs.CopyTo(memoryStream);

            // Decode the bitmap from the memory stream
            memoryStream.Position = 0;
            return memoryStream;
        }

        // Asynchronous method to load a tile from disk
        private async Task<SKBitmap> LoadTileAsync(string zoomFolderName, string tileFileName,
            Func<string, string, Task<Stream>> tileProvider)
        {
            if (_cache is { } cache)
            {
                var item = await cache.GetOrCreateAsync(Path.Combine(zoomFolderName, tileFileName),
                    async () =>
                    {
                        using var stream = await tileProvider.Invoke(zoomFolderName, tileFileName);
                        if (stream is null)
                            return null;
                        return SKBitmap.Decode(stream);

                    });

                return item.Tile;
            }

            using var stream = await tileProvider.Invoke(zoomFolderName, tileFileName);
            if (stream is null)
                return null;
            return SKBitmap.Decode(stream);
        }

        private SKBitmap LoadTile(string zoomFolderName, string tileFileName,
            Func<string, string, Stream> tileProvider)
        {
            var tilePath = Path.Combine(zoomFolderName, tileFileName);

            if (_cache is { } cache)
            {
                var item = cache.GetOrCreate(tilePath,
                    () =>
                    {
                        using var stream = tileProvider.Invoke(zoomFolderName, tileFileName);
                        if (stream is null)
                            return null;
                        return SKBitmap.Decode(stream);

                    });

                return item.Tile;
            }
            var stream = tileProvider.Invoke(zoomFolderName, tileFileName);
            if (stream is null)
                return null;
            return SKBitmap.Decode(stream);
        }

        public void Dispose()
        {
            _cache?.Dispose();
        }
    }
}