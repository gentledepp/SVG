using NUnit.Framework;
using SkiaSharp;
using Svg.DeepZoom;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.IO.Compression;

namespace Svg.Tests.Win
{
    public class TileGeneratorTests
    {
        const string SmallFileName = "Assets\\mountain_4000x1800";
        private const string LargeFileName = "Assets\\landscape_12000x6000";
        private ITileGenerator _tileService;

        [SetUp]
        public void SetUp()
        {
            SvgPlatform.Init();

            _tileService = new TileGenerator();
        }

        private static Func<string, string, Task<Stream>> CreateStreamProvider(string tileDir)
        {
            return (folderName, fileName) =>
            {
                var dir = Path.Combine(tileDir, folderName);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                var filePath = Path.Combine(dir, fileName);
                return Task.FromResult<Stream>(File.Create(filePath));
            };
        }

        [Test]
        public async Task L_CanCreateTiles()
        {
            var file = Path.Combine(TestContext.CurrentContext.WorkDirectory, $"{SmallFileName}.jpg");
            var tileDir = Path.Combine(TestContext.CurrentContext.WorkDirectory, $"tiles_{SmallFileName}");
            var progressValue = 0;
            var progress = new Progress<int>();
            progress.ProgressChanged += (sender, i) =>
            {
                progressValue = i;
            };

            var td = new DirectoryInfo(tileDir);
            if (td.Exists)
                td.Delete(true);

            await _tileService.GenerateTilesAsync(file, CreateStreamProvider(tileDir), progress);

            var tiles = Directory.EnumerateFiles(tileDir, "*.*", SearchOption.AllDirectories);
            Assert.True(tiles.Any());
        }

        [Test]
        public async Task XL_CanCreateTilesAsyncInParallel()
        {
            var file = Path.Combine(TestContext.CurrentContext.WorkDirectory, $"{LargeFileName}.jpg");
            var tileDir = Path.Combine(TestContext.CurrentContext.WorkDirectory, $"tiles_{LargeFileName}");
            var td = new DirectoryInfo(tileDir);
            if (td.Exists)
                td.Delete(true);
            var progressValue = 0;
            var progress = new Progress<int>();
            progress.ProgressChanged += (sender, i) =>
            {
                progressValue = i;
            };
            var gen = new TileGenerator();

            using var fStream = File.OpenRead(file);

            await gen.GenerateTilesAsync(fStream, CreateStreamProvider(tileDir), progress: progress, backgroundColor: "#ffffff", maxParallelTasks: -1);

            var tiles = Directory.EnumerateFiles(tileDir, "*.*", SearchOption.AllDirectories);
            Assert.True(tiles.Any());
        }

        [Test]
        public async Task XL_CanCreateTilesAsyncInParallel_LimitingParallelizationToOne()
        {
            var file = Path.Combine(TestContext.CurrentContext.WorkDirectory, $"{LargeFileName}.jpg");
            var tileDir = Path.Combine(TestContext.CurrentContext.WorkDirectory, $"tiles_{LargeFileName}");
            var td = new DirectoryInfo(tileDir);
            if (td.Exists)
                td.Delete(true);
            var progressValue = 0;
            var progress = new Progress<int>();
            progress.ProgressChanged += (sender, i) =>
            {
                progressValue = i;
            };
            var gen = new TileGenerator();

            using var fStream = File.OpenRead(file);

            await gen.GenerateTilesAsync(fStream, CreateStreamProvider(tileDir), progress: progress, backgroundColor: "#ffffff", maxParallelTasks: 1);

            var tiles = Directory.EnumerateFiles(tileDir, "*.*", SearchOption.AllDirectories);
            Assert.True(tiles.Any());
        }


        [Test]
        public async Task CreateTile_DownsampledLevels_UseSmoothFiltering()
        {
            // A 1024x1024 1px black/white checkerboard. At z2 the whole image downsamples 4x into a
            // single 256x256 tile. Nearest-neighbor sampling of a period-2 pattern with stride 4 yields
            // a uniform colour (all 255 or all 0); any bilerp/mipmap filter averages to mid-gray (~127).
            const int size = 1024;
            using var source = new SKBitmap(size, size, SKColorType.Bgra8888, SKAlphaType.Premul);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    source.SetPixel(x, y, ((x + y) & 1) == 0 ? SKColors.White : SKColors.Black);
                }
            }

            using var srcStream = new MemoryStream();
            using (var w = new SKManagedWStream(srcStream))
                source.Encode(w, SKEncodedImageFormat.Png, 100);
            srcStream.Position = 0;

            var tiles = new ConcurrentDictionary<string, MemoryStream>();
            Func<string, string, Task<Stream>> provider = (folder, file) =>
            {
                var ms = new MemoryStream();
                tiles[$"{folder}/{file}"] = ms;
                return Task.FromResult<Stream>(ms);
            };

            await new TileGenerator().GenerateTilesAsync(srcStream, provider);

            // maxZoomLevel = ceil(log2(1024/256)) = 2. At z2 the whole image fits in one 256x256 tile (4x downsample).
            Assert.IsTrue(tiles.TryGetValue("z2/y0_x0.png", out var tileMs), "Expected z2 tile not generated");

            using var tileBitmap = SKBitmap.Decode(tileMs.ToArray());

            int midTone = 0;
            int total = tileBitmap.Width * tileBitmap.Height;
            for (int y = 0; y < tileBitmap.Height; y++)
            {
                for (int x = 0; x < tileBitmap.Width; x++)
                {
                    var px = tileBitmap.GetPixel(x, y);
                    int gray = (px.Red + px.Green + px.Blue) / 3;
                    if (gray >= 64 && gray <= 192) midTone++;
                }
            }

            TestContext.WriteLine($"Mid-tone pixels in z2 tile: {midTone} / {total}");
            Assert.Greater(midTone, total / 2,
                $"Downsampled tile has {midTone} mid-tone pixels out of {total}; expected >50% — filter quality is not being applied.");
        }

        [Test]
        public async Task GenerateTilesAsync_WithJpegEncoding_ProducesSmallerOutput()
        {
            // Photographic source — JPEG at 85 should be noticeably smaller than PNG 100.
            var file = Path.Combine(TestContext.CurrentContext.WorkDirectory, $"{SmallFileName}.jpg");

            long pngBytes = 0;
            long jpegBytes = 0;

            using (var fStream = File.OpenRead(file))
            {
                Func<string, string, Task<Stream>> pngProvider = (folder, fname) =>
                {
                    var ms = new MemoryStream();
                    return Task.FromResult<Stream>(new CountingStream(ms, n => Interlocked.Add(ref pngBytes, n)));
                };
                // Explicitly force PNG so auto-detection of the JPEG source doesn't apply.
                await new TileGenerator().GenerateTilesAsync(fStream, pngProvider,
                    imageFormat: SKEncodedImageFormat.Png);
            }

            using (var fStream = File.OpenRead(file))
            {
                Func<string, string, Task<Stream>> jpegProvider = (folder, fname) =>
                {
                    var ms = new MemoryStream();
                    return Task.FromResult<Stream>(new CountingStream(ms, n => Interlocked.Add(ref jpegBytes, n)));
                };
                await new TileGenerator().GenerateTilesAsync(
                    fStream, jpegProvider,
                    imageFormat: SKEncodedImageFormat.Jpeg, quality: 85);
            }

            TestContext.WriteLine($"PNG total bytes:  {pngBytes:N0}");
            TestContext.WriteLine($"JPEG total bytes: {jpegBytes:N0}");

            Assert.Greater(pngBytes, 0, "PNG output must be non-empty.");
            Assert.Greater(jpegBytes, 0, "JPEG output must be non-empty.");
            Assert.Less((double)jpegBytes / pngBytes, 0.70,
                $"JPEG should be <70% of PNG; got ratio {(double)jpegBytes / pngBytes:P0}.");
        }

        private sealed class CountingStream : Stream
        {
            private readonly Stream _inner;
            private readonly Action<long> _onDispose;
            public CountingStream(Stream inner, Action<long> onDispose) { _inner = inner; _onDispose = onDispose; }
            public override bool CanRead => false;
            public override bool CanSeek => false;
            public override bool CanWrite => true;
            public override long Length => _inner.Length;
            public override long Position { get => _inner.Position; set => _inner.Position = value; }
            public override void Flush() => _inner.Flush();
            public override int Read(byte[] b, int o, int c) => throw new NotSupportedException();
            public override long Seek(long o, SeekOrigin s) => throw new NotSupportedException();
            public override void SetLength(long v) => _inner.SetLength(v);
            public override void Write(byte[] b, int o, int c) => _inner.Write(b, o, c);
            public override Task WriteAsync(byte[] b, int o, int c, CancellationToken t) => _inner.WriteAsync(b, o, c, t);
            protected override void Dispose(bool d) { if (d) _onDispose(_inner.Length); base.Dispose(d); }
        }

        // ---- SVG pipeline tests ----

        private sealed class RecordingTileGenerator : TileGenerator
        {
            private int _maxDim;
            private int _decodeSubsetTileCallCount;
            public int MaxDim => _maxDim;
            public int RenderSvgToTileCallCount { get; private set; }  // SVG path is sequential
            public int DecodeSubsetTileCallCount => _decodeSubsetTileCallCount;
            public bool? LastSubsetSupported { get; private set; }

            private int _liveCount;
            private int _peakLiveCount;
            public int PeakLiveCount => _peakLiveCount;

            private void UpdateMaxDim(int dim)
            {
                int v;
                do { v = _maxDim; } while (dim > v && Interlocked.CompareExchange(ref _maxDim, dim, v) != v);
            }

            protected override SKBitmap CreateBitmap(int width, int height)
            {
                UpdateMaxDim(Math.Max(width, height));
                return base.CreateBitmap(width, height);
            }

            protected override SKSurface CreateSurface(SKImageInfo info)
            {
                UpdateMaxDim(Math.Max(info.Width, info.Height));
                return base.CreateSurface(info);
            }

            protected override SKBitmap CreateDecodeBuffer(int width, int height)
            {
                UpdateMaxDim(Math.Max(width, height));
                return base.CreateDecodeBuffer(width, height);
            }

            protected override SKBitmap RenderSvgToTile(SvgDocument document, int tileX, int tileY,
                int pyramidW, int pyramidH, float scaleX, float scaleY,
                int tileSize, string backgroundColor, int overdrawMargin)
            {
                RenderSvgToTileCallCount++;
                return base.RenderSvgToTile(document, tileX, tileY, pyramidW, pyramidH,
                    scaleX, scaleY, tileSize, backgroundColor, overdrawMargin);
            }

            public int RenderSvgToBlockCallCount { get; private set; }

            protected override SKBitmap RenderSvgToBlock(
                SvgDocument document, int blockX, int blockY, int tilesPerBlock,
                int tilesX, int tilesY,
                int pyramidW, int pyramidH, float scaleX, float scaleY,
                int tileSize, string backgroundColor,
                SKSurface sharedSurface, ISvgRenderer renderer)
            {
                RenderSvgToBlockCallCount++;
                return base.RenderSvgToBlock(document, blockX, blockY, tilesPerBlock,
                    tilesX, tilesY, pyramidW, pyramidH, scaleX, scaleY, tileSize, backgroundColor,
                    sharedSurface, renderer);
            }

            protected override SKBitmap DecodeSubsetTile(SKCodec codec, int tileX, int tileY,
                int originalWidth, int originalHeight, int tileSize, string backgroundColor)
            {
                Interlocked.Increment(ref _decodeSubsetTileCallCount);
                return base.DecodeSubsetTile(codec, tileX, tileY, originalWidth, originalHeight,
                    tileSize, backgroundColor);
            }

            protected override bool SupportsSubset(SKCodec codec, int originalWidth, int originalHeight)
            {
                LastSubsetSupported = base.SupportsSubset(codec, originalWidth, originalHeight);
                return LastSubsetSupported.Value;
            }

            protected override void OnTileAddedToPending(int z, int x, int y)
            {
                int now = Interlocked.Increment(ref _liveCount);
                int old;
                do { old = _peakLiveCount; }
                while (now > old && Interlocked.CompareExchange(ref _peakLiveCount, now, old) != old);
            }

            protected override void OnTileRemovedFromPending(int z, int x, int y)
            {
                Interlocked.Decrement(ref _liveCount);
            }
        }

        private static Func<string, string, Task<Stream>> CreateMemoryStreamProvider(
            ConcurrentDictionary<string, MemoryStream> tiles)
        {
            return (folder, fname) =>
            {
                var ms = new MemoryStream();
                tiles[$"{folder}/{fname}"] = ms;
                return Task.FromResult<Stream>(ms);
            };
        }

        [Test]
        public async Task GenerateTilesFromSvgAsync_DoesNotAllocateFullSizeBitmap()
        {
            var file = Path.Combine(TestContext.CurrentContext.WorkDirectory, "Assets\\plan_iss.svg");
            var doc = SvgDocument.Open(file);
            var gen = new RecordingTileGenerator();
            var tiles = new ConcurrentDictionary<string, MemoryStream>();

            await gen.GenerateTilesAsync(doc, CreateMemoryStreamProvider(tiles));

            Assert.IsTrue(tiles.Any(), "No tiles were generated.");
            // Super-tile rendering renders 4×4 tiles (1024px) at most — still far below the full pyramid width.
            Assert.LessOrEqual(gen.MaxDim, 4 * 256,
                $"SVG tile pipeline allocated a surface/bitmap of {gen.MaxDim}px; expected ≤{4 * 256} (no full-pyramid decode).");
        }

        [Test]
        public async Task GenerateTilesFromSvgAsync_WithTargetWidth_UsesAspectPreservingHeight()
        {
            const int targetWidth = 4000;
            const int tileSize = 256;

            var file = Path.Combine(TestContext.CurrentContext.WorkDirectory, "Assets\\plan_iss.svg");
            var doc = SvgDocument.Open(file);
            var docSize = doc.GetDimensions();

            int expectedTilesX = (int)Math.Ceiling((double)targetWidth / tileSize);
            int pyramidH = (int)Math.Ceiling(targetWidth * docSize.Height / docSize.Width);
            int expectedTilesY = (int)Math.Ceiling((double)pyramidH / tileSize);

            var tiles = new ConcurrentDictionary<string, MemoryStream>();
            await new TileGenerator().GenerateTilesAsync(doc, targetWidth, CreateMemoryStreamProvider(tiles));

            int z0Count = tiles.Keys.Count(k => k.StartsWith("z0/"));
            Assert.AreEqual(expectedTilesX * expectedTilesY, z0Count,
                $"Expected {expectedTilesX}×{expectedTilesY}={expectedTilesX * expectedTilesY} z=0 tiles; got {z0Count}.");

            for (int ty = 0; ty < expectedTilesY; ty++)
                for (int tx = 0; tx < expectedTilesX; tx++)
                    Assert.IsTrue(tiles.ContainsKey($"z0/y{ty}_x{tx}.png"),
                        $"Missing z=0 tile y={ty}, x={tx}.");
        }

        [Test]
        public async Task GenerateTilesFromSvgAsync_ProducesExpectedZoomLevels()
        {
            const int tileSize = 256;

            var file = Path.Combine(TestContext.CurrentContext.WorkDirectory, "Assets\\plan_iss.svg");
            var doc = SvgDocument.Open(file);
            var docSize = doc.GetDimensions();
            int pyramidW = (int)Math.Ceiling(docSize.Width);
            int pyramidH = (int)Math.Ceiling(docSize.Height);

            int maxZoomLevel = (int)Math.Ceiling(Math.Log(Math.Max(pyramidW, pyramidH) / (double)tileSize, 2));
            var levelTilesX = new int[maxZoomLevel + 1];
            var levelTilesY = new int[maxZoomLevel + 1];
            levelTilesX[0] = (int)Math.Ceiling((double)pyramidW / tileSize);
            levelTilesY[0] = (int)Math.Ceiling((double)pyramidH / tileSize);
            for (int z = 1; z <= maxZoomLevel; z++)
            {
                levelTilesX[z] = (int)Math.Ceiling(levelTilesX[z - 1] / 2.0);
                levelTilesY[z] = (int)Math.Ceiling(levelTilesY[z - 1] / 2.0);
            }

            var tiles = new ConcurrentDictionary<string, MemoryStream>();
            await new TileGenerator().GenerateTilesAsync(doc, CreateMemoryStreamProvider(tiles));

            int totalExpected = 0;
            for (int z = 0; z <= maxZoomLevel; z++)
            {
                int expected = levelTilesX[z] * levelTilesY[z];
                totalExpected += expected;
                int actual = tiles.Keys.Count(k => k.StartsWith($"z{z}/"));
                Assert.AreEqual(expected, actual,
                    $"Level z={z}: expected {expected} tiles ({levelTilesX[z]}×{levelTilesY[z]}), got {actual}.");
            }

            int actualTileCount = tiles.Keys.Count(k => k.StartsWith("z"));
            Assert.AreEqual(totalExpected, actualTileCount, "Total tile count mismatch.");

            Assert.IsTrue(tiles.ContainsKey("/dimensions.json"), "dimensions.json missing.");

#if !NETFRAMEWORK
            var dimsJson = System.Text.Encoding.UTF8.GetString(tiles["/dimensions.json"].ToArray());
            var dims = System.Text.Json.JsonDocument.Parse(dimsJson).RootElement;
            Assert.AreEqual(pyramidW, dims.GetProperty("width").GetInt32(), "dimensions.json width mismatch.");
            Assert.AreEqual(pyramidH, dims.GetProperty("height").GetInt32(), "dimensions.json height mismatch.");
#endif
        }

        [Test]
        public async Task GenerateTilesFromSvgAsync_TilesMatchCurrentPipeline_WithinTolerance()
        {
            const float maxMeanL1Delta = 4f / 255f;
            const int tileSize = 256;

            var file = Path.Combine(TestContext.CurrentContext.WorkDirectory, "Assets\\testBosPlan.svg");

            // Old raster pipeline: SVG → full bitmap → PNG stream → GenerateTilesAsync(Stream)
            var rasterTiles = new ConcurrentDictionary<string, MemoryStream>();
            {
                var doc = SvgDocument.Open(file);
                using var bitmap = doc.Draw();
                using var pngStream = new MemoryStream();
                bitmap.SavePng(pngStream, 100);
                pngStream.Position = 0;
                await new TileGenerator().GenerateTilesAsync(pngStream,
                    CreateMemoryStreamProvider(rasterTiles));
            }

            // New SVG pipeline
            var svgTiles = new ConcurrentDictionary<string, MemoryStream>();
            {
                var doc = SvgDocument.Open(file);
                await new TileGenerator().GenerateTilesAsync(doc, CreateMemoryStreamProvider(svgTiles));
            }

            var docSize = SvgDocument.Open(file).GetDimensions();
            int pyramidW = (int)Math.Ceiling(docSize.Width);
            int pyramidH = (int)Math.Ceiling(docSize.Height);
            int tilesX = (int)Math.Ceiling((double)pyramidW / tileSize);
            int tilesY = (int)Math.Ceiling((double)pyramidH / tileSize);

            int compared = 0;
            double totalMeanL1 = 0;

            // Interior tiles only (exclude last column and last row to avoid edge artefacts).
            for (int ty = 0; ty < tilesY - 1; ty++)
            {
                for (int tx = 0; tx < tilesX - 1; tx++)
                {
                    var key = $"z0/y{ty}_x{tx}.png";
                    if (!rasterTiles.TryGetValue(key, out var rasterMs) ||
                        !svgTiles.TryGetValue(key, out var svgMs))
                        continue;

                    using var rBmp = SKBitmap.Decode(rasterMs.ToArray());
                    using var sBmp = SKBitmap.Decode(svgMs.ToArray());
                    if (rBmp == null || sBmp == null) continue;

                    long l1 = 0;
                    int pixels = rBmp.Width * rBmp.Height;
                    for (int py = 0; py < rBmp.Height; py++)
                    {
                        for (int px = 0; px < rBmp.Width; px++)
                        {
                            var rp = rBmp.GetPixel(px, py);
                            var sp = sBmp.GetPixel(px, py);
                            l1 += Math.Abs(rp.Red - sp.Red)
                                + Math.Abs(rp.Green - sp.Green)
                                + Math.Abs(rp.Blue - sp.Blue);
                        }
                    }

                    totalMeanL1 += (double)l1 / (pixels * 3 * 255.0);
                    compared++;
                }
            }

            Assert.Greater(compared, 0, "No interior z=0 tiles were compared.");
            double meanL1 = totalMeanL1 / compared;
            TestContext.WriteLine($"Compared {compared} interior z=0 tiles. Mean L1 delta: {meanL1:F6}");
            Assert.Less(meanL1, maxMeanL1Delta,
                $"Mean L1 delta {meanL1:F6} exceeds tolerance {maxMeanL1Delta:F6}.");
        }

        [Test]
        public async Task GenerateTilesFromSvgAsync_LowerLevelsBuiltFromUpperTiles()
        {
            const int tileSize = 256;
            const int tilesPerBlock = 4;

            var file = Path.Combine(TestContext.CurrentContext.WorkDirectory, "Assets\\plan_iss.svg");
            var doc = SvgDocument.Open(file);
            var docSize = doc.GetDimensions();
            int pyramidW = (int)Math.Ceiling(docSize.Width);
            int pyramidH = (int)Math.Ceiling(docSize.Height);
            int tilesX = (int)Math.Ceiling((double)pyramidW / tileSize);
            int tilesY = (int)Math.Ceiling((double)pyramidH / tileSize);
            int expectedBlocks = (int)Math.Ceiling(tilesX / (double)tilesPerBlock)
                               * (int)Math.Ceiling(tilesY / (double)tilesPerBlock);

            var gen = new RecordingTileGenerator();
            var tiles = new ConcurrentDictionary<string, MemoryStream>();
            await gen.GenerateTilesAsync(doc, CreateMemoryStreamProvider(tiles));

            // Super-tile pipeline: one block render per 4×4 group; no individual tile renders.
            Assert.AreEqual(expectedBlocks, gen.RenderSvgToBlockCallCount,
                $"Expected {expectedBlocks} block renders (one per {tilesPerBlock}×{tilesPerBlock} super-tile); got {gen.RenderSvgToBlockCallCount}.");
            Assert.AreEqual(0, gen.RenderSvgToTileCallCount,
                "RenderSvgToTile should not be called when the super-tile pipeline is active.");
        }

        [Test]
        public async Task GenerateTilesFromSvgAsync_DisposesChildTilesEagerly()
        {
            const int tileSize = 256;

            var file = Path.Combine(TestContext.CurrentContext.WorkDirectory, "Assets\\plan_iss.svg");
            var doc = SvgDocument.Open(file);
            var docSize = doc.GetDimensions();
            int pyramidW = (int)Math.Ceiling(docSize.Width);
            int pyramidH = (int)Math.Ceiling(docSize.Height);
            int maxZoomLevel = (int)Math.Ceiling(Math.Log(Math.Max(pyramidW, pyramidH) / (double)tileSize, 2));

            var gen = new RecordingTileGenerator();
            var tiles = new ConcurrentDictionary<string, MemoryStream>();
            await gen.GenerateTilesAsync(doc, CreateMemoryStreamProvider(tiles));

            int allowedPeak = 4 * (maxZoomLevel + 1);
            TestContext.WriteLine($"Peak live tiles: {gen.PeakLiveCount}, maxZoomLevel: {maxZoomLevel}, bound: {allowedPeak}");
            Assert.LessOrEqual(gen.PeakLiveCount, allowedPeak,
                $"Peak live pending-tile count {gen.PeakLiveCount} exceeds O(levels) bound {allowedPeak}; cascade is not disposing children eagerly.");
        }

        [Test]
        public async Task GenerateTilesFromSvgAsync_BosPlan_ProducesCorrectTileStructure()
        {
            const int tileSize = 256;
            const int tilesPerBlock = 4;

            var file = Path.Combine(TestContext.CurrentContext.WorkDirectory, "Assets\\testBosPlan.svg");
            var doc = SvgDocument.Open(file);
            var docSize = doc.GetDimensions();
            int pyramidW = (int)Math.Ceiling(docSize.Width);
            int pyramidH = (int)Math.Ceiling(docSize.Height);
            int tilesX = (int)Math.Ceiling((double)pyramidW / tileSize);
            int tilesY = (int)Math.Ceiling((double)pyramidH / tileSize);
            int expectedZ0 = tilesX * tilesY;
            int expectedBlocks = (int)Math.Ceiling(tilesX / (double)tilesPerBlock)
                               * (int)Math.Ceiling(tilesY / (double)tilesPerBlock);

            var gen = new RecordingTileGenerator();
            var tiles = new ConcurrentDictionary<string, MemoryStream>();
            await gen.GenerateTilesAsync(doc, CreateMemoryStreamProvider(tiles));

            // Correct number of z=0 tiles.
            int z0Count = tiles.Keys.Count(k => k.StartsWith("z0/"));
            Assert.AreEqual(expectedZ0, z0Count,
                $"Expected {tilesX}×{tilesY}={expectedZ0} z=0 tiles; got {z0Count}.");

            // Super-tile batch count: one render per 4×4 block, no per-tile renders.
            Assert.AreEqual(expectedBlocks, gen.RenderSvgToBlockCallCount,
                $"Expected {expectedBlocks} block renders; got {gen.RenderSvgToBlockCallCount}.");
            Assert.AreEqual(0, gen.RenderSvgToTileCallCount,
                "RenderSvgToTile should not be called by the default SVG pipeline.");

            // All z=0 tiles decode successfully and contain non-background content.
            int emptyTiles = 0;
            foreach (var key in tiles.Keys.Where(k => k.StartsWith("z0/")))
            {
                using var bmp = SKBitmap.Decode(tiles[key].ToArray());
                Assert.IsNotNull(bmp, $"Tile {key} could not be decoded.");
                Assert.AreEqual(tileSize, bmp.Width,  $"Tile {key} has wrong width.");
                Assert.AreEqual(tileSize, bmp.Height, $"Tile {key} has wrong height.");

                bool hasContent = false;
                for (int py = 0; py < bmp.Height && !hasContent; py++)
                    for (int px = 0; px < bmp.Width && !hasContent; px++)
                    {
                        var c = bmp.GetPixel(px, py);
                        if (c.Alpha > 0 && !(c.Red == 255 && c.Green == 255 && c.Blue == 255))
                            hasContent = true;
                    }
                if (!hasContent) emptyTiles++;
            }

            TestContext.WriteLine($"z=0 tiles with non-background content: {z0Count - emptyTiles}/{z0Count}");
            Assert.Less(emptyTiles, z0Count,
                "All z=0 tiles are fully white/transparent; SVG content was not rendered.");

            Assert.IsTrue(tiles.ContainsKey("/dimensions.json"), "dimensions.json is missing.");
        }

        [Test]
        public async Task GenerateTilesFromSvgAsync_ViewportLargerThanViewBox_ContentFillsPyramid()
        {
            // Regression: width/height 10× the viewBox used to shrink content to 1/100 of
            // the pyramid (SvgViewBox.CalculateTransform returned min/max instead of viewport/viewBox).
            // Sizes are tile-aligned so the top-left z=0 tile is fully inside the pyramid.
            const string rawSvg = "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"2560\" height=\"1280\" " +
                                  "viewBox=\"0 0 256 128\">" +
                                  "<rect x=\"0\" y=\"0\" width=\"256\" height=\"128\" fill=\"red\"/></svg>";
            var doc = SvgDocument.FromSvg<SvgDocument>(rawSvg);

            var tiles = new ConcurrentDictionary<string, MemoryStream>();
            await new TileGenerator().GenerateTilesAsync(doc, CreateMemoryStreamProvider(tiles));

            Assert.IsTrue(tiles.ContainsKey("z0/y0_x0.png"), "Missing top-left z=0 tile.");
            using var bmp = SKBitmap.Decode(tiles["z0/y0_x0.png"].ToArray());
            Assert.IsNotNull(bmp);

            int redPixels = 0;
            int total = bmp.Width * bmp.Height;
            for (int py = 0; py < bmp.Height; py++)
                for (int px = 0; px < bmp.Width; px++)
                {
                    var c = bmp.GetPixel(px, py);
                    if (c.Red >= 200 && c.Green < 60 && c.Blue < 60) redPixels++;
                }

            TestContext.WriteLine($"Red pixels in top-left z=0 tile: {redPixels} / {total}");
            Assert.Greater(redPixels, total * 0.9,
                $"Expected >90% red pixels in the top-left z=0 tile (rect fills the pyramid); got {redPixels}/{total}. " +
                "The viewBox-to-viewport scale likely collapsed content when viewport > viewBox.");
        }

        [Test]
        public async Task GenerateTilesFromSvgAsync_ViewportSmallerThanViewBox_ContentStillRenders()
        {
            // Companion to the regression above — scale < 1 path, tile-aligned so the
            // top-left z=0 tile is fully inside the pyramid.
            const string rawSvg = "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"512\" height=\"256\" " +
                                  "viewBox=\"0 0 2560 1280\">" +
                                  "<rect x=\"0\" y=\"0\" width=\"2560\" height=\"1280\" fill=\"red\"/></svg>";
            var doc = SvgDocument.FromSvg<SvgDocument>(rawSvg);

            var tiles = new ConcurrentDictionary<string, MemoryStream>();
            await new TileGenerator().GenerateTilesAsync(doc, CreateMemoryStreamProvider(tiles));

            Assert.IsTrue(tiles.ContainsKey("z0/y0_x0.png"), "Missing top-left z=0 tile.");
            using var bmp = SKBitmap.Decode(tiles["z0/y0_x0.png"].ToArray());
            Assert.IsNotNull(bmp);

            int redPixels = 0;
            int total = bmp.Width * bmp.Height;
            for (int py = 0; py < bmp.Height; py++)
                for (int px = 0; px < bmp.Width; px++)
                {
                    var c = bmp.GetPixel(px, py);
                    if (c.Red >= 200 && c.Green < 60 && c.Blue < 60) redPixels++;
                }

            TestContext.WriteLine($"Red pixels in top-left z=0 tile: {redPixels} / {total}");
            Assert.Greater(redPixels, total * 0.9,
                $"Expected >90% red pixels in the top-left z=0 tile; got {redPixels}/{total}.");
        }

        // ---- Codec (raster) pipeline tests ----

        [Test]
        public async Task GenerateTilesFromCodecAsync_JPEG_DoesNotAllocateFullSizeBitmap()
        {
            var file = Path.Combine(TestContext.CurrentContext.WorkDirectory, $"{SmallFileName}.jpg");
            var gen = new RecordingTileGenerator();
            var tiles = new ConcurrentDictionary<string, MemoryStream>();

            using var stream = File.OpenRead(file);
            await gen.GenerateTilesAsync(stream, CreateMemoryStreamProvider(tiles));

            Assert.IsTrue(tiles.Any(), "No tiles were generated.");
            Assert.LessOrEqual(gen.MaxDim, 512,
                $"Codec pipeline allocated a buffer of {gen.MaxDim}px; expected ≤512 (no full-image decode). " +
                $"Source image is 4000×1800.");
        }

        [Test]
        public async Task GenerateTilesFromCodecAsync_JPEG_CodecPathIsConsistent()
        {
            // Verifies that whichever path SupportsSubset selects (subset or scanline),
            // DecodeSubsetTile call count matches: >0 iff subset was selected.
            var file = Path.Combine(TestContext.CurrentContext.WorkDirectory, $"{SmallFileName}.jpg");
            var gen = new RecordingTileGenerator();
            var tiles = new ConcurrentDictionary<string, MemoryStream>();

            using var stream = File.OpenRead(file);
            await gen.GenerateTilesAsync(stream, CreateMemoryStreamProvider(tiles));

            Assert.IsNotNull(gen.LastSubsetSupported, "SupportsSubset was never called.");
            Assert.IsTrue(tiles.Any(), "No tiles were generated.");

            if (gen.LastSubsetSupported.Value)
                Assert.Greater(gen.DecodeSubsetTileCallCount, 0,
                    "Subset path was selected but DecodeSubsetTile was never called.");
            else
                Assert.AreEqual(0, gen.DecodeSubsetTileCallCount,
                    "Scanline path was selected but DecodeSubsetTile was called anyway.");
        }

        [Test]
        public async Task GenerateTilesFromCodecAsync_PNG_UsesScanlinePath()
        {
            // Synthetic 512×256 PNG — PNG does not support SKCodec subset decode.
            using var srcBmp = new SKBitmap(512, 256, SKColorType.Bgra8888, SKAlphaType.Premul);
            for (int y = 0; y < srcBmp.Height; y++)
                for (int x = 0; x < srcBmp.Width; x++)
                    srcBmp.SetPixel(x, y, new SKColor((byte)(x % 256), (byte)(y % 256), 128));

            using var pngStream = new MemoryStream();
            using (var w = new SKManagedWStream(pngStream))
                srcBmp.Encode(w, SKEncodedImageFormat.Png, 100);
            pngStream.Position = 0;

            var gen = new RecordingTileGenerator();
            var tiles = new ConcurrentDictionary<string, MemoryStream>();
            await gen.GenerateTilesAsync(pngStream, CreateMemoryStreamProvider(tiles));

            Assert.AreEqual(false, gen.LastSubsetSupported,
                "PNG should not support subset decoding; expected scanline path.");
            Assert.AreEqual(0, gen.DecodeSubsetTileCallCount,
                "No subset tiles should be decoded for PNG input.");
            Assert.IsTrue(tiles.Any(), "No tiles were generated.");
            int z0Count = tiles.Keys.Count(k => k.StartsWith("z0/"));
            Assert.AreEqual(2, z0Count,
                $"Expected 2 z=0 tiles for 512×256 PNG (2×1 grid), got {z0Count}.");
        }

        [Test]
        public async Task GenerateTilesFromCodecAsync_JPEG_Z0TilesMatchDirectDecode()
        {
            var file = Path.Combine(TestContext.CurrentContext.WorkDirectory, $"{SmallFileName}.jpg");

            // Reference: full direct decode
            using var refBmp = SKBitmap.Decode(File.ReadAllBytes(file));

            // Force PNG output so the tile is losslessly encoded — this test verifies decode
            // correctness (subset decode == full decode), not the output format selection.
            var tiles = new ConcurrentDictionary<string, MemoryStream>();
            using (var stream = File.OpenRead(file))
                await new TileGenerator().GenerateTilesAsync(stream, CreateMemoryStreamProvider(tiles),
                    imageFormat: SKEncodedImageFormat.Png);

            const string key = "z0/y0_x0.png";
            Assert.IsTrue(tiles.ContainsKey(key), $"Tile {key} not found.");
            using var tileBmp = SKBitmap.Decode(tiles[key].ToArray());
            Assert.IsNotNull(tileBmp, "Could not decode tile bitmap.");

            int maxDiff = 0;
            for (int py = 0; py < tileBmp.Height; py++)
            {
                for (int px = 0; px < tileBmp.Width; px++)
                {
                    var rp = refBmp.GetPixel(px, py);
                    var tp = tileBmp.GetPixel(px, py);
                    maxDiff = Math.Max(maxDiff, Math.Abs(rp.Red - tp.Red));
                    maxDiff = Math.Max(maxDiff, Math.Abs(rp.Green - tp.Green));
                    maxDiff = Math.Max(maxDiff, Math.Abs(rp.Blue - tp.Blue));
                }
            }

            TestContext.WriteLine($"Max pixel diff for {key}: {maxDiff}");
            Assert.AreEqual(0, maxDiff,
                $"Codec z=0 tile differs from direct decode by up to {maxDiff} per channel.");
        }

        [Test]
        public async Task GenerateTilesFromCodecAsync_PNG_Z0TilesMatchDirectDecode()
        {
            const int width = 512;
            const int height = 256;
            const int tileSize = 256;

            // Synthetic gradient PNG
            using var srcBmp = new SKBitmap(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    srcBmp.SetPixel(x, y, new SKColor((byte)(x % 256), (byte)(y % 256), 128));

            using var pngData = new MemoryStream();
            using (var w = new SKManagedWStream(pngData))
                srcBmp.Encode(w, SKEncodedImageFormat.Png, 100);

            // Codec pipeline
            pngData.Position = 0;
            var tiles = new ConcurrentDictionary<string, MemoryStream>();
            await new TileGenerator().GenerateTilesAsync(pngData, CreateMemoryStreamProvider(tiles));

            const string key = "z0/y0_x0.png";
            Assert.IsTrue(tiles.ContainsKey(key), $"Tile {key} not found.");
            using var tileBmp = SKBitmap.Decode(tiles[key].ToArray());
            Assert.IsNotNull(tileBmp, "Could not decode tile bitmap.");

            // Compare first 256×256 of source to tile
            int maxDiff = 0;
            for (int py = 0; py < tileSize; py++)
            {
                for (int px = 0; px < tileSize; px++)
                {
                    var rp = srcBmp.GetPixel(px, py);
                    var tp = tileBmp.GetPixel(px, py);
                    maxDiff = Math.Max(maxDiff, Math.Abs(rp.Red - tp.Red));
                    maxDiff = Math.Max(maxDiff, Math.Abs(rp.Green - tp.Green));
                    maxDiff = Math.Max(maxDiff, Math.Abs(rp.Blue - tp.Blue));
                }
            }

            TestContext.WriteLine($"Max pixel diff for {key} (PNG scanline): {maxDiff}");
            Assert.AreEqual(0, maxDiff,
                $"PNG scanline z=0 tile differs from reference by up to {maxDiff} per channel.");
        }

        [Test]
        public async Task GenerateTilesAsync_DefaultMaxParallelism_DoesNotExceedProcessorCount()
        {
            var file = Path.Combine(TestContext.CurrentContext.WorkDirectory, $"{SmallFileName}.jpg");
            var tileDir = Path.Combine(TestContext.CurrentContext.WorkDirectory, "tiles_parallelism_default");
            var td = new DirectoryInfo(tileDir);
            if (td.Exists) td.Delete(true);

            int currentConcurrent = 0;
            int maxConcurrent = 0;

            Func<string, string, Task<Stream>> provider = async (folderName, fileName) =>
            {
                int now = Interlocked.Increment(ref currentConcurrent);
                int old;
                do { old = maxConcurrent; }
                while (now > old && Interlocked.CompareExchange(ref maxConcurrent, now, old) != old);

                // Hold the slot so overlapping calls are observable.
                await Task.Delay(20);

                var dir = Path.Combine(tileDir, folderName);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                var filePath = Path.Combine(dir, fileName);
                var fs = File.Create(filePath);

                Interlocked.Decrement(ref currentConcurrent);
                return (Stream)fs;
            };

            using var fStream = File.OpenRead(file);
            await new TileGenerator().GenerateTilesAsync(fStream, provider);

            TestContext.WriteLine($"Observed max concurrent provider calls: {maxConcurrent}, ProcessorCount: {Environment.ProcessorCount}");
            Assert.LessOrEqual(maxConcurrent, Environment.ProcessorCount,
                $"Default concurrency {maxConcurrent} exceeded Environment.ProcessorCount {Environment.ProcessorCount}; unlimited parallelism can trigger SkiaSharp SEHException.");
        }
    }
}
