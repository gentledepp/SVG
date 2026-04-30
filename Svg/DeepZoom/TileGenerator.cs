using SkiaSharp;
using Svg.Interfaces;
using Svg.Platform;
using System;
using System.Collections.Generic;
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
                backgroundColor, -1);
        }

        public async Task GenerateTilesAsync(Stream sourceImageStream,
            Func<string, string, Task<Stream>> tileOutputStreamProvider, IProgress<int> progress = null,
            string backgroundColor = "#ffffff",
            int maxParallelTasks = -1,  // -1 → Environment.ProcessorCount
            SKEncodedImageFormat? imageFormat = null,  // null → auto-detect from source
            int quality = -1)
        {
            progress ??= new Progress<int>();
            progress.Report(0);
            const int tileSize = TileConstants.TileSize;

            // Buffer source bytes so multiple SKCodec instances can be created for parallel tile decoding.
            byte[] sourceBytes;
            using (var buf = new MemoryStream())
            {
                await sourceImageStream.CopyToAsync(buf);
                sourceBytes = buf.ToArray();
            }
            Func<SKCodec> codecFactory = () => SKCodec.Create(new MemoryStream(sourceBytes));

            using var codec = codecFactory();
            if (codec == null)
                throw new InvalidOperationException("Cannot create codec from stream; format may be unsupported.");

            // null → auto-detect format from source; explicit value → use as-is.
            SKEncodedImageFormat effectiveFormat = imageFormat
                ?? (codec.EncodedFormat == SKEncodedImageFormat.Jpeg
                    ? SKEncodedImageFormat.Jpeg
                    : SKEncodedImageFormat.Png);

            // Auto-select quality: JPEG 85 (high quality, reasonable size); PNG 100 (lossless regardless).
            if (quality < 0)
                quality = effectiveFormat == SKEncodedImageFormat.Jpeg ? 85 : 100;

            int originalWidth = codec.Info.Width;
            int originalHeight = codec.Info.Height;

            int maxZoomLevel = (int)Math.Ceiling(Math.Log(Math.Max(originalWidth, originalHeight) / (double)tileSize, 2));

            var levelTilesX = new int[maxZoomLevel + 1];
            var levelTilesY = new int[maxZoomLevel + 1];
            levelTilesX[0] = (int)Math.Ceiling((double)originalWidth / tileSize);
            levelTilesY[0] = (int)Math.Ceiling((double)originalHeight / tileSize);
            for (int z = 1; z <= maxZoomLevel; z++)
            {
                levelTilesX[z] = (int)Math.Ceiling(levelTilesX[z - 1] / 2.0);
                levelTilesY[z] = (int)Math.Ceiling(levelTilesY[z - 1] / 2.0);
            }

            var pending = new Dictionary<(int z, int x, int y), SKImage>();
            var pendingLock = new object();
            int totalZ0Tiles = levelTilesX[0] * levelTilesY[0];

            if (SupportsSubset(codec, originalWidth, originalHeight))
                await GenerateZ0TilesSubsetAsync(codecFactory, originalWidth, originalHeight,
                    levelTilesX[0], levelTilesY[0], tileSize, backgroundColor, effectiveFormat, quality,
                    pending, pendingLock, levelTilesX, levelTilesY, maxZoomLevel,
                    tileOutputStreamProvider, progress, totalZ0Tiles, maxParallelTasks);
            else
                await GenerateZ0TilesScanlineAsync(codec, originalWidth, originalHeight,
                    levelTilesX[0], levelTilesY[0], tileSize, backgroundColor, effectiveFormat, quality,
                    pending, pendingLock, levelTilesX, levelTilesY, maxZoomLevel,
                    tileOutputStreamProvider, progress, totalZ0Tiles);

            foreach (var kvp in pending)
            {
                OnTileRemovedFromPending(kvp.Key.z, kvp.Key.x, kvp.Key.y);
                kvp.Value.Dispose();
            }
            pending.Clear();
            await WriteMetadataAsync(originalWidth, originalHeight, tileOutputStreamProvider);
        }

        private async Task GenerateZ0TilesSubsetAsync(
            Func<SKCodec> codecFactory, int originalWidth, int originalHeight,
            int tilesX, int tilesY, int tileSize, string backgroundColor,
            SKEncodedImageFormat imageFormat, int quality,
            Dictionary<(int z, int x, int y), SKImage> pending, object pendingLock,
            int[] levelTilesX, int[] levelTilesY, int maxZoomLevel,
            Func<string, string, Task<Stream>> streamProvider,
            IProgress<int> progress, int totalZ0Tiles, int maxParallelTasks)
        {
            int parallelism = maxParallelTasks > 0 ? maxParallelTasks : Environment.ProcessorCount;
            // cpuSemaphore gates concurrent decode + encode (CPU-bound work).
            // ioSemaphore gates concurrent file writes (keeps provider call concurrency ≤ parallelism).
            using var cpuSemaphore = new SemaphoreSlim(parallelism, parallelism);
            using var ioSemaphore = new SemaphoreSlim(parallelism, parallelism);
            int tilesCompleted = 0;

            int numBlocksX = (int)Math.Ceiling(tilesX / 2.0);
            int numBlocksY = (int)Math.Ceiling(tilesY / 2.0);

            var tasks = new List<Task>();

            for (int by = 0; by < numBlocksY; by++)
            {
                for (int bx = 0; bx < numBlocksX; bx++)
                {
                    for (int dy = 0; dy < 2; dy++)
                    {
                        for (int dx = 0; dx < 2; dx++)
                        {
                            int tx = bx * 2 + dx;
                            int ty = by * 2 + dy;
                            if (tx >= tilesX || ty >= tilesY) continue;

                            int lTx = tx, lTy = ty;
                            tasks.Add(Task.Run(async () =>
                            {
                                // Decode (CPU-limited)
                                await cpuSemaphore.WaitAsync();
                                SKBitmap? tileBitmap = null;
                                try
                                {
                                    using var localCodec = codecFactory();
                                    tileBitmap = DecodeSubsetTile(localCodec, lTx, lTy,
                                        originalWidth, originalHeight, tileSize, backgroundColor);
                                }
                                finally { cpuSemaphore.Release(); }

                                if (tileBitmap == null) return;
                                try
                                {
                                    // Encode (CPU-limited); runs in parallel with other tiles' I/O.
                                    byte[] encodedBytes;
                                    await cpuSemaphore.WaitAsync();
                                    try { encodedBytes = EncodeTile(tileBitmap, imageFormat, quality); }
                                    finally { cpuSemaphore.Release(); }

                                    // Write to file (I/O, limited to ≤ parallelism concurrent writes).
                                    await ioSemaphore.WaitAsync();
                                    try { await WriteEncodedTileAsync(encodedBytes, 0, lTx, lTy, streamProvider); }
                                    finally { ioSemaphore.Release(); }

                                    var img = SKImage.FromBitmap(tileBitmap);
                                    lock (pendingLock)
                                    {
                                        pending[(0, lTx, lTy)] = img;
                                        OnTileAddedToPending(0, lTx, lTy);
                                    }
                                    progress.Report(Interlocked.Increment(ref tilesCompleted) * 100 / Math.Max(1, totalZ0Tiles));

                                    await TryCascadeAsync(0, lTx, lTy, pending, pendingLock,
                                        levelTilesX, levelTilesY, maxZoomLevel, tileSize, backgroundColor,
                                        streamProvider, imageFormat, quality, cpuSemaphore, ioSemaphore);
                                }
                                finally { tileBitmap.Dispose(); }
                            }));
                        }
                    }
                }
            }

            await Task.WhenAll(tasks);
        }

        private async Task GenerateZ0TilesScanlineAsync(
            SKCodec codec, int originalWidth, int originalHeight,
            int tilesX, int tilesY, int tileSize, string backgroundColor,
            SKEncodedImageFormat imageFormat, int quality,
            Dictionary<(int z, int x, int y), SKImage> pending, object pendingLock,
            int[] levelTilesX, int[] levelTilesY, int maxZoomLevel,
            Func<string, string, Task<Stream>> streamProvider,
            IProgress<int> progress, int totalZ0Tiles)
        {
            // Use Rgba8888+Unpremul for scanline decode.  If the codec doesn't support per-scanline
            // decode (PNG in SkiaSharp 3.x returns a failure), fall back to a single GetPixels full
            // decode.  DrawImage handles RGBA→BGRA conversion when compositing onto the Bgra8888 surface.
            var decodeInfo = new SKImageInfo(originalWidth, originalHeight, SKColorType.Rgba8888, SKAlphaType.Unpremul);
            bool useScanline = codec.StartScanlineDecode(decodeInfo) == SKCodecResult.Success;

            // Full-decode fallback (PNG): decode entire image once, extract tiles via DrawImage.
            SKBitmap? fullBmp = null;
            SKImage? fullImg = null;
            if (!useScanline)
            {
                fullBmp = new SKBitmap(originalWidth, originalHeight, SKColorType.Rgba8888, SKAlphaType.Unpremul);
                codec.GetPixels(decodeInfo, fullBmp.GetPixels());
                fullImg = SKImage.FromBitmap(fullBmp);
            }

            int tilesCompleted = 0;

            for (int ty = 0; ty < tilesY; ty++)
            {
                int bandHeight = Math.Min(tileSize, originalHeight - ty * tileSize);
                int bandStartY = ty * tileSize;

                SKBitmap? bandBmp = null;
                SKImage? bandImg = null;
                if (useScanline)
                {
                    bandBmp = new SKBitmap(originalWidth, bandHeight, SKColorType.Rgba8888, SKAlphaType.Unpremul);
                    codec.GetScanlines(bandBmp.GetPixels(), bandHeight, bandBmp.RowBytes);
                    bandImg = SKImage.FromBitmap(bandBmp);
                }

                SKImage srcImg = bandImg ?? fullImg!;
                float srcY = useScanline ? 0 : bandStartY;

                for (int tx = 0; tx < tilesX; tx++)
                {
                    int tileStartX = tx * tileSize;
                    int tileW = Math.Min(tileSize, originalWidth - tileStartX);

                    using var tileBitmap = CreateBitmap(tileSize, tileSize);
                    using var canvas = new SKCanvas(tileBitmap);
                    canvas.Clear(SKColor.Parse(backgroundColor));
                    canvas.DrawImage(srcImg,
                        new SKRect(tileStartX, srcY, tileStartX + tileW, srcY + bandHeight),
                        new SKRect(0, 0, tileW, bandHeight));

                    await WriteTileAndStorePendingAsync(tileBitmap, 0, tx, ty, pending, pendingLock,
                        streamProvider, imageFormat, quality);

                    progress.Report(++tilesCompleted * 100 / Math.Max(1, totalZ0Tiles));

                    await TryCascadeAsync(0, tx, ty, pending, pendingLock, levelTilesX, levelTilesY,
                        maxZoomLevel, tileSize, backgroundColor, streamProvider, imageFormat, quality,
                        null, null);
                }

                bandImg?.Dispose();
                bandBmp?.Dispose();
            }

            fullImg?.Dispose();
            fullBmp?.Dispose();
        }

        protected virtual bool SupportsSubset(SKCodec codec, int originalWidth, int originalHeight)
        {
            var probe = new SKRectI(0, 0,
                Math.Min(TileConstants.TileSize, originalWidth),
                Math.Min(TileConstants.TileSize, originalHeight));
            return codec.GetValidSubset(ref probe);
        }

        protected virtual SKBitmap DecodeSubsetTile(SKCodec codec, int tileX, int tileY,
            int originalWidth, int originalHeight, int tileSize, string backgroundColor)
        {
            int srcX = tileX * tileSize;
            int srcY = tileY * tileSize;
            int srcW = Math.Min(tileSize, originalWidth - srcX);
            int srcH = Math.Min(tileSize, originalHeight - srcY);

            var desiredRect = new SKRectI(srcX, srcY, srcX + srcW, srcY + srcH);
            var subsetRect = desiredRect;
            codec.GetValidSubset(ref subsetRect); // snap to MCU boundary

            var decodeInfo = new SKImageInfo(subsetRect.Width, subsetRect.Height, SKColorType.Bgra8888, SKAlphaType.Premul);
            using var decodedBmp = CreateDecodeBuffer(subsetRect.Width, subsetRect.Height);
            var options = new SKCodecOptions(SKZeroInitialized.No, subsetRect);
            codec.GetPixels(decodeInfo, decodedBmp.GetPixels(), decodedBmp.RowBytes, options);

            // Offset within decoded region where the desired tile content starts (MCU snap overshoot).
            int cropX = desiredRect.Left - subsetRect.Left;
            int cropY = desiredRect.Top - subsetRect.Top;

            var tileBitmap = CreateBitmap(tileSize, tileSize);
            using var tileCanvas = new SKCanvas(tileBitmap);
            tileCanvas.Clear(SKColor.Parse(backgroundColor));
            tileCanvas.DrawBitmap(decodedBmp,
                new SKRect(cropX, cropY, cropX + srcW, cropY + srcH),
                new SKRect(0, 0, srcW, srcH));

            return tileBitmap;
        }

        protected virtual SKBitmap CreateDecodeBuffer(int width, int height)
            => new SKBitmap(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);

        // ---- SVG-aware pipeline ----

        public Task GenerateTilesAsync(
            SvgDocument document,
            Func<string, string, Task<Stream>> tileOutputStreamProvider,
            IProgress<int> progress = null,
            string backgroundColor = "#ffffff",
            int maxParallelTasks = -1,
            SKEncodedImageFormat? imageFormat = null,
            int quality = -1,
            int overdrawMargin = 0)
        {
            var docSize = document.GetDimensions();
            int targetWidth = (int)Math.Ceiling(docSize.Width);
            return GenerateTilesAsync(document, targetWidth, tileOutputStreamProvider, progress,
                backgroundColor, maxParallelTasks, imageFormat, quality, overdrawMargin);
        }

        public async Task GenerateTilesAsync(
            SvgDocument document,
            int targetWidth,
            Func<string, string, Task<Stream>> tileOutputStreamProvider,
            IProgress<int> progress = null,
            string backgroundColor = "#ffffff",
            int maxParallelTasks = -1,
            SKEncodedImageFormat? imageFormat = null,
            int quality = -1,
            int overdrawMargin = 0)
        {
            progress ??= new Progress<int>();
            progress.Report(0);
            const int tileSize = TileConstants.TileSize;

            // SVG always produces raster tiles; default to PNG. Auto-select quality.
            SKEncodedImageFormat effectiveFormat = imageFormat ?? SKEncodedImageFormat.Png;
            if (quality < 0)
                quality = effectiveFormat == SKEncodedImageFormat.Jpeg ? 85 : 100;

            var docSize = document.GetDimensions();
            int pyramidW = targetWidth;
            int pyramidH = (int)Math.Ceiling(targetWidth * docSize.Height / docSize.Width);
            float scaleX = pyramidW / docSize.Width;
            float scaleY = pyramidH / docSize.Height;

            int maxZoomLevel = (int)Math.Ceiling(Math.Log(Math.Max(pyramidW, pyramidH) / (double)tileSize, 2));

            // Precompute tile counts at each zoom level; z=0 is finest (full resolution).
            var levelTilesX = new int[maxZoomLevel + 1];
            var levelTilesY = new int[maxZoomLevel + 1];
            levelTilesX[0] = (int)Math.Ceiling((double)pyramidW / tileSize);
            levelTilesY[0] = (int)Math.Ceiling((double)pyramidH / tileSize);
            for (int z = 1; z <= maxZoomLevel; z++)
            {
                levelTilesX[z] = (int)Math.Ceiling(levelTilesX[z - 1] / 2.0);
                levelTilesY[z] = (int)Math.Ceiling(levelTilesY[z - 1] / 2.0);
            }

            // Holds tiles awaiting sibling completion for cascade downsampling.
            var pending = new Dictionary<(int z, int x, int y), SKImage>();
            var pendingLock = new object();

            int totalZ0Tiles = levelTilesX[0] * levelTilesY[0];
            int tilesCompleted = 0;

            // Render z=0 tiles in super-tile blocks (4×4 tiles = 1024×1024 per SVG render pass).
            // Within each block, tiles are extracted in 2×2-sub-block-major order so each 2×2
            // group can cascade to its parent immediately.
            //
            // One shared surface + renderer are created here and kept alive for ALL blocks so that
            // the renderer's DrawingCache (brushes, pens, font data) is populated on the first block
            // and reused for every subsequent block rather than being rebuilt from scratch each time.
            const int tilesPerBlock = 4;
            int surfaceSize = tilesPerBlock * tileSize;
            // SkiaGraphics takes ownership of the surface and disposes it with the renderer.
            var sharedSurface = CreateSurface(
                new SKImageInfo(surfaceSize, surfaceSize, SKColorType.Bgra8888, SKAlphaType.Premul));
            document.Overflow = SvgOverflow.Auto;
            using var sharedRendererHandle = document.CreateRendererFromGraphics(
                new SkiaGraphics(sharedSurface), pyramidW, pyramidH);
            var sharedRenderer = sharedRendererHandle.Renderer;
            sharedRenderer.SetBoundable(new GenericBoundable(0, 0, pyramidW, pyramidH));

            int numSuperX = (int)Math.Ceiling(levelTilesX[0] / (double)tilesPerBlock);
            int numSuperY = (int)Math.Ceiling(levelTilesY[0] / (double)tilesPerBlock);

            for (int sby = 0; sby < numSuperY; sby++)
            {
                for (int sbx = 0; sbx < numSuperX; sbx++)
                {
                    using var blockBitmap = RenderSvgToBlock(
                        document, sbx, sby, tilesPerBlock,
                        levelTilesX[0], levelTilesY[0],
                        pyramidW, pyramidH, scaleX, scaleY, tileSize, backgroundColor,
                        sharedSurface, sharedRenderer);

                    for (int by = 0; by < (int)Math.Ceiling(tilesPerBlock / 2.0); by++)
                    for (int bx = 0; bx < (int)Math.Ceiling(tilesPerBlock / 2.0); bx++)
                    for (int dy = 0; dy < 2; dy++)
                    for (int dx = 0; dx < 2; dx++)
                    {
                        int localX = bx * 2 + dx;
                        int localY = by * 2 + dy;
                        int tx = sbx * tilesPerBlock + localX;
                        int ty = sby * tilesPerBlock + localY;
                        if (tx >= levelTilesX[0] || ty >= levelTilesY[0]) continue;

                        using var tileBitmap = ExtractTileFromBlock(blockBitmap, localX, localY, tileSize);

                        await WriteTileAndStorePendingAsync(tileBitmap, 0, tx, ty, pending, pendingLock,
                            tileOutputStreamProvider, effectiveFormat, quality);

                        progress.Report(++tilesCompleted * 100 / Math.Max(1, totalZ0Tiles));

                        await TryCascadeAsync(0, tx, ty, pending, pendingLock, levelTilesX, levelTilesY,
                            maxZoomLevel, tileSize, backgroundColor, tileOutputStreamProvider,
                            effectiveFormat, quality, null, null);
                    }
                }
            }

            // Dispose any remaining pending images (z=maxZoomLevel tile has no parent).
            foreach (var kvp in pending)
            {
                OnTileRemovedFromPending(kvp.Key.z, kvp.Key.x, kvp.Key.y);
                kvp.Value.Dispose();
            }
            pending.Clear();
            await WriteMetadataAsync(pyramidW, pyramidH, tileOutputStreamProvider);
        }

        private async Task WriteTileAndStorePendingAsync(
            SKBitmap bitmap, int z, int x, int y,
            Dictionary<(int z, int x, int y), SKImage> pending, object pendingLock,
            Func<string, string, Task<Stream>> streamProvider,
            SKEncodedImageFormat imageFormat, int quality)
        {
            var encodedBytes = EncodeTile(bitmap, imageFormat, quality);
            await WriteEncodedTileAsync(encodedBytes, z, x, y, streamProvider);

            var image = SKImage.FromBitmap(bitmap);
            lock (pendingLock)
            {
                pending[(z, x, y)] = image;
                OnTileAddedToPending(z, x, y);
            }
        }

        private async Task TryCascadeAsync(
            int z, int x, int y,
            Dictionary<(int z, int x, int y), SKImage> pending, object pendingLock,
            int[] levelTilesX, int[] levelTilesY, int maxZoomLevel, int tileSize,
            string backgroundColor, Func<string, string, Task<Stream>> streamProvider,
            SKEncodedImageFormat imageFormat, int quality,
            SemaphoreSlim? cpuSemaphore, SemaphoreSlim? ioSemaphore)
        {
            int parentZ = z + 1;
            if (parentZ > maxZoomLevel) return;

            int parentX = x / 2;
            int parentY = y / 2;

            // Determine which children of this parent are within bounds.
            var childCoords = new List<(int cx, int cy)>(4);
            for (int dy = 0; dy < 2; dy++)
            for (int dx = 0; dx < 2; dx++)
            {
                int cx = parentX * 2 + dx;
                int cy = parentY * 2 + dy;
                if (cx < levelTilesX[z] && cy < levelTilesY[z])
                    childCoords.Add((cx, cy));
            }

            // Atomically claim all children from pending; bail out if any child is still missing.
            // No await inside the lock — only synchronous dictionary operations.
            List<(int cx, int cy, SKImage img)>? claimed = null;
            lock (pendingLock)
            {
                var imgs = new List<(int, int, SKImage)>(childCoords.Count);
                foreach (var (cx, cy) in childCoords)
                {
                    if (!pending.TryGetValue((z, cx, cy), out var img)) { imgs = null!; break; }
                    imgs.Add((cx, cy, img));
                }
                if (imgs != null)
                {
                    foreach (var (cx, cy, _) in imgs)
                    {
                        pending.Remove((z, cx, cy));
                        OnTileRemovedFromPending(z, cx, cy);
                    }
                    claimed = imgs;
                }
            }

            if (claimed == null) return;

            // Outside the lock: draw parent bitmap from children, encode, write.
            // Children are "owned" by this call and disposed in the finally block.
            var parentBitmap = CreateBitmap(tileSize, tileSize);
            byte[] encodedBytes;
            SKImage? parentImage = null;
            try
            {
                if (cpuSemaphore != null) await cpuSemaphore.WaitAsync();
                try
                {
                    using var parentCanvas = new SKCanvas(parentBitmap);
                    parentCanvas.Clear(SKColor.Parse(backgroundColor));
                    using var paint = new SKPaint { FilterQuality = SKFilterQuality.Medium };
                    foreach (var (cx, cy, img) in claimed)
                    {
                        int dx = cx - parentX * 2;
                        int dy = cy - parentY * 2;
                        parentCanvas.DrawImage(img,
                            new SKRect(dx * 128, dy * 128, (dx + 1) * 128, (dy + 1) * 128),
                            paint);
                    }
                    encodedBytes = EncodeTile(parentBitmap, imageFormat, quality);
                }
                finally { cpuSemaphore?.Release(); }

                if (ioSemaphore != null) await ioSemaphore.WaitAsync();
                try { await WriteEncodedTileAsync(encodedBytes, parentZ, parentX, parentY, streamProvider); }
                finally { ioSemaphore?.Release(); }

                parentImage = SKImage.FromBitmap(parentBitmap);
            }
            finally
            {
                parentBitmap.Dispose();
                foreach (var (_, _, img) in claimed) img.Dispose();
            }

            lock (pendingLock)
            {
                pending[(parentZ, parentX, parentY)] = parentImage!;
                OnTileAddedToPending(parentZ, parentX, parentY);
            }

            // Recurse: check whether this parent completes its own grandparent block.
            await TryCascadeAsync(parentZ, parentX, parentY, pending, pendingLock,
                levelTilesX, levelTilesY, maxZoomLevel, tileSize, backgroundColor,
                streamProvider, imageFormat, quality, cpuSemaphore, ioSemaphore);
        }

        private static byte[] EncodeTile(SKBitmap bitmap, SKEncodedImageFormat imageFormat, int quality)
        {
            using var ms = new MemoryStream();
            using (var skStream = new SKManagedWStream(ms))
                bitmap.Encode(skStream, imageFormat, quality);
            return ms.ToArray();
        }

        private static async Task WriteEncodedTileAsync(
            byte[] bytes, int z, int x, int y,
            Func<string, string, Task<Stream>> streamProvider)
        {
            using var outStream = await streamProvider($"z{z}", $"y{y}_x{x}.png");
            await outStream.WriteAsync(bytes, 0, bytes.Length);
        }

        private static async Task WriteMetadataAsync(
            int width, int height,
            Func<string, string, Task<Stream>> streamProvider)
        {
            var json = System.Text.Encoding.UTF8.GetBytes($"{{\"width\":{width},\"height\":{height}}}");
            using var outStream = await streamProvider("", "dimensions.json");
            await outStream.WriteAsync(json, 0, json.Length);
        }

        protected virtual SKBitmap RenderSvgToTile(
            SvgDocument document, int tileX, int tileY,
            int pyramidW, int pyramidH, float scaleX, float scaleY,
            int tileSize, string backgroundColor, int overdrawMargin)
        {
            int renderSize = tileSize + 2 * overdrawMargin;
            var info = new SKImageInfo(renderSize, renderSize, SKColorType.Bgra8888, SKAlphaType.Premul);

            // Surface is created without `using` — SkiaGraphics takes ownership and disposes it.
            var surface = CreateSurface(info);
            var canvas = surface.Canvas;
            canvas.Clear(SKColor.Parse(backgroundColor));

            // Post-multiply order: Translate first then Scale so Scale is applied first to SVG coords.
            canvas.Translate(-(tileX * tileSize - overdrawMargin), -(tileY * tileSize - overdrawMargin));
            canvas.Scale(scaleX, scaleY);

            using var rendererHandle = document.CreateRendererFromGraphics(
                new SkiaGraphics(surface), pyramidW, pyramidH);
            var renderer = rendererHandle.Renderer;
            renderer.SetBoundable(new GenericBoundable(0, 0, pyramidW, pyramidH));
            document.Overflow = SvgOverflow.Auto;
            document.RenderToRenderer(renderer);

            // Snapshot before renderer (and thus SkiaGraphics → surface) is disposed.
            using var snapshot = surface.Snapshot();

            var result = CreateBitmap(tileSize, tileSize);
            using var dstCanvas = new SKCanvas(result);

            if (overdrawMargin > 0)
            {
                dstCanvas.DrawImage(snapshot,
                    new SKRect(overdrawMargin, overdrawMargin, overdrawMargin + tileSize, overdrawMargin + tileSize),
                    new SKRect(0, 0, tileSize, tileSize));
            }
            else
            {
                dstCanvas.DrawImage(snapshot, new SKRect(0, 0, tileSize, tileSize));
            }

            return result;
        }

        /// <summary>
        /// Renders one super-tile block onto <paramref name="sharedSurface"/> using
        /// <paramref name="renderer"/>, whose DrawingCache is kept alive across calls by the
        /// caller. Returns a bitmap cropped to the actual tile content area of the block.
        /// </summary>
        protected virtual SKBitmap RenderSvgToBlock(
            SvgDocument document,
            int blockX, int blockY,
            int tilesPerBlock,
            int tilesX, int tilesY,
            int pyramidW, int pyramidH,
            float scaleX, float scaleY,
            int tileSize,
            string backgroundColor,
            SKSurface sharedSurface,
            ISvgRenderer renderer)
        {
            int blockTilesW = Math.Min(tilesPerBlock, tilesX - blockX * tilesPerBlock);
            int blockTilesH = Math.Min(tilesPerBlock, tilesY - blockY * tilesPerBlock);
            int blockPixW   = blockTilesW * tileSize;
            int blockPixH   = blockTilesH * tileSize;
            int blockPixX   = blockX * tilesPerBlock * tileSize;
            int blockPixY   = blockY * tilesPerBlock * tileSize;

            var canvas = sharedSurface.Canvas;
            canvas.Clear(SKColor.Parse(backgroundColor));
            // Save/RestoreToCount keeps transform changes scoped to this block so consecutive
            // block renders start from a clean identity state.
            int saveCount = canvas.Save();
            canvas.Translate(-blockPixX, -blockPixY);
            canvas.Scale(scaleX, scaleY);
            document.RenderToRenderer(renderer);
            canvas.RestoreToCount(saveCount);

            using var snapshot = sharedSurface.Snapshot();
            var result = CreateBitmap(blockPixW, blockPixH);
            using var dstCanvas = new SKCanvas(result);
            dstCanvas.DrawImage(snapshot, new SKRect(0, 0, blockPixW, blockPixH), new SKRect(0, 0, blockPixW, blockPixH));
            return result;
        }

        private SKBitmap ExtractTileFromBlock(SKBitmap block, int localX, int localY, int tileSize)
        {
            var tile = CreateBitmap(tileSize, tileSize);
            using var c = new SKCanvas(tile);
            c.DrawBitmap(block,
                new SKRect(localX * tileSize, localY * tileSize,
                           (localX + 1) * tileSize, (localY + 1) * tileSize),
                new SKRect(0, 0, tileSize, tileSize));
            return tile;
        }

        protected virtual SKBitmap CreateBitmap(int width, int height)
            => new SKBitmap(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);

        protected virtual SKSurface CreateSurface(SKImageInfo info)
            => SKSurface.Create(info);

        protected virtual void OnTileAddedToPending(int z, int x, int y) { }
        protected virtual void OnTileRemovedFromPending(int z, int x, int y) { }

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
