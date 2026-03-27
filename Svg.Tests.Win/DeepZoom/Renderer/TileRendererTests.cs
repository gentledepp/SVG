using NUnit.Framework;
using Shouldly;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System;
using SkiaSharp;
using Svg.DeepZoom;

namespace Svg.Tests.Win.Renderer;

public class TileRendererTests
{
    const string SmallFileName = "Assets\\mountain_4000x1800";
    private const string LargeFileName = "Assets\\landscape_12000x6000";

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
    [TestCase(2.0f)]
    [TestCase(1.5f)]
    [TestCase(1.0f)]
    [TestCase(0.5f)]
    [TestCase(0.25f)]
    [TestCase(0.125f)]
    public async Task CanRenderAtOriginWithDifferentZoomFactors(float zoomFactor)
    {
        var fileName = SmallFileName;
        var tileDir = await ArrangeTiles(fileName);

        var rndr = new TileRenderer(800, 600, new TileCacheOptions(TimeSpan.FromHours(1)));
        var fn = Path.Combine(TestContext.CurrentContext.WorkDirectory, "Assets", "tiles_Assets","mountain_4000x1800", $"x0_y0-z{(Math.Max((int)Math.Round(4/2*zoomFactor), 0)).ToString(CultureInfo.InvariantCulture)} zoo{zoomFactor} -rnder{rndr.Width}x{rndr.Height}.png");


        rndr.RenderBitmap(tileDir, fn, 0, 0, zoomFactor);

        AssertRenderedFile(fn, rndr.Width, rndr.Height);
    }

    [Test]
    [TestCase(2.0f)]
    [TestCase(1.5f)]
    [TestCase(1.0f)]
    [TestCase(0.5f)]
    [TestCase(0.25f)]
    public async Task CanRenderAtImageCenterWithDifferentZoomFactors(float zoomFactor)
    {
        var fileName = SmallFileName;
        var tileDir = await ArrangeTiles(fileName);
        var rndr = new TileRenderer(800, 600, new TileCacheOptions(TimeSpan.FromHours(1)));
        var fn = Path.Combine(TestContext.CurrentContext.WorkDirectory, "Assets", $"x1700_y900z-{(1f / zoomFactor).ToString(CultureInfo.InvariantCulture)}-rnder{rndr.Width}x{rndr.Height}.jpeg");

        // offset is a canvas translation in screen pixels: drawX = offsetX + tileX * tileSizeAtZoom.
        // To center the 4000x1800 image inside the viewport, translate by (viewport - image*zoom)/2.
        var x = (rndr.Width - 4000 * zoomFactor) / 2f;
        var y = (rndr.Height - 1800 * zoomFactor) / 2f;

        rndr.RenderBitmap(tileDir, fn, x, y, zoomFactor);

        AssertRenderedFile(fn, rndr.Width, rndr.Height);
    }


    [Test]
    [TestCase(2.0f)]
    [TestCase(1.5f)]
    [TestCase(1.0f)]
    [TestCase(0.5f)]
    [TestCase(0.25f)]
    [TestCase(0.125f)]
    [TestCase(0.0625f)]
    [TestCase(0.03125f)]
    public async Task CanRenderHugeImageAtOriginWithDifferentZoomFactors(float zoomFactor)
    {
        // "Huge" test must use the large asset: the tile pyramid's max zoom level is
        // ceil(log2(max(w,h)/256)) — 4 for 4000x1800, 6 for 12000x6000 — so the deepest
        // zoomFactor cases (0.03125 needs z5) only have tiles on the large image.
        var fileName = LargeFileName;
        var tileDir = await ArrangeTiles(fileName);
        var rndr = new TileRenderer(1131, 703, new TileCacheOptions(TimeSpan.FromHours(1)));
        var fn = Path.Combine(TestContext.CurrentContext.WorkDirectory, $"{fileName}x0_y0-z{(1f / zoomFactor).ToString(CultureInfo.InvariantCulture)}-rnder{rndr.Width}x{rndr.Height}.jpeg");


        await rndr.RenderBitmapAsync(tileDir, fn, 0, 0, zoomFactor);

        AssertRenderedFile(fn, rndr.Width, rndr.Height);
    }


    [Test]
    [TestCase(2.0f)]
    [TestCase(1.5f)]
    [TestCase(1.0f)]
    [TestCase(0.5f)]
    [TestCase(0.25f)]
    [TestCase(0.125f)]
    public async Task CanRenderAsyncAtOriginWithDifferentZoomFactors(float zoomFactor)
    {
        var fileName = SmallFileName;
        var tileDir = await ArrangeTiles(fileName);
        var rndr = new TileRenderer(800, 600, new TileCacheOptions(TimeSpan.FromHours(1)));
        var fn = Path.Combine(TestContext.CurrentContext.WorkDirectory, "Assets", $"x0_y0-z{(1f / zoomFactor).ToString(CultureInfo.InvariantCulture)}-rnder{rndr.Width}x{rndr.Height}.jpeg");


        await rndr.RenderBitmapAsync(tileDir, fn, 0, 0, zoomFactor);

        AssertRenderedFile(fn, rndr.Width, rndr.Height);
    }

    [Test]
    [TestCase(2.0f)]
    [TestCase(1.5f)]
    [TestCase(1.0f)]
    [TestCase(0.5f)]
    [TestCase(0.25f)]
    public async Task CanRenderAsyncAtImageCenterWithDifferentZoomFactors(float zoomFactor)
    {
        var fileName = SmallFileName;
        var tileDir = await ArrangeTiles(fileName);

        var rndr = new TileRenderer(800, 600, new TileCacheOptions(TimeSpan.FromHours(1)));
        var fn = Path.Combine(TestContext.CurrentContext.WorkDirectory, "Assets", $"x1700_y900z-{(1f / zoomFactor).ToString(CultureInfo.InvariantCulture)}-rnder{rndr.Width}x{rndr.Height}.jpeg");

        // offset is a canvas translation in screen pixels: drawX = offsetX + tileX * tileSizeAtZoom.
        // To center the 4000x1800 image inside the viewport, translate by (viewport - image*zoom)/2.
        var x = (rndr.Width  - 4000 * zoomFactor) / 2f;
        var y = (rndr.Height - 1800 * zoomFactor) / 2f;

        await rndr.RenderBitmapAsync(tileDir, fn, x, y, zoomFactor);

        AssertRenderedFile(fn, rndr.Width, rndr.Height);
    }


    [Test]
    [TestCase(4.0f)]
    [TestCase(2.0f)]
    [TestCase(1.5f)]
    [TestCase(1.0f)]
    [TestCase(0.5f)]
    [TestCase(0.25f)]
    [TestCase(0.125f)]
    [TestCase(0.0625f)]
    [TestCase(0.03125f)]
    public async Task CanRenderAsyncHugeImageAtOriginWithDifferentZoomFactors(float zoomFactor)
    {
        var fileName = LargeFileName;
        var tileDir = await ArrangeTiles(fileName);

        var rndr = new TileRenderer(800, 600, new TileCacheOptions(TimeSpan.FromHours(1)));
        var fn = Path.Combine(TestContext.CurrentContext.WorkDirectory, $"{fileName}x0_y0-z{(zoomFactor).ToString(CultureInfo.InvariantCulture)}-rnder{rndr.Width}x{rndr.Height}.jpeg");

        await rndr.RenderBitmapAsync(tileDir, fn, 0, 0, zoomFactor);
    }


    [Test]
    [TestCase(2.0f)]
    [TestCase(1.5f)]
    [TestCase(1.0f)]
    [TestCase(0.5f)]
    [TestCase(0.25f)]
    [TestCase(0.125f)]
    public async Task CanRenderAsyncAtOriginWithDifferentZoomFactors_UsingCache(float zoomFactor)
    {
        var fileName = SmallFileName;
        var tileDir = await ArrangeTiles(fileName);

        var trackingCache = new TrackingTileCache(new TileCacheOptions(TimeSpan.FromHours(1)));
        using var rndr = new TileRenderer(800, 600, trackingCache);
        var fn = Path.Combine(TestContext.CurrentContext.WorkDirectory, "Assets", $"x0_y0-z{(1f / zoomFactor).ToString(CultureInfo.InvariantCulture)}-rnder{rndr.Width}x{rndr.Height}.jpeg");

        // first render: tiles are not yet cached, all loads go to disk
        await rndr.RenderBitmapAsync(tileDir, fn, 0, 0, zoomFactor);

        TestContext.WriteLine($"First render  — hits: {trackingCache.HitCount}, misses: {trackingCache.MissCount}");
        trackingCache.MissCount.ShouldBeGreaterThan(0, "first render should load tiles from disk");

        trackingCache.Reset();

        // second render: all tiles are in cache, no disk I/O
        await rndr.RenderBitmapAsync(tileDir, fn, 0, 0, zoomFactor);

        TestContext.WriteLine($"Second render — hits: {trackingCache.HitCount}, misses: {trackingCache.MissCount}");
        trackingCache.HitCount.ShouldBeGreaterThan(0, "second render should use cached tiles");
        trackingCache.MissCount.ShouldBe(0, "second render should not load any tiles from disk");
    }

    [Test]
    public async Task LoadTileStream_ReturnsFileStreamDirectly_WithoutMemoryCopy()
    {
        // After the fix LoadTileStream returns the FileStream (or a wrapper) directly rather than
        // copying all bytes into a MemoryStream — the key observable is that the returned stream
        // is NOT a MemoryStream.
        var tileDir = await ArrangeTiles(SmallFileName);
        SvgPlatform.Init();

        var rndr = new TileRenderer(1, 1);
        var zoomFolder = Path.Combine(tileDir, "z0");
        var firstTile = Directory.EnumerateFiles(zoomFolder, "*.png").First();
        var tileFile = Path.GetFileName(firstTile);

        using var stream = rndr.LoadTileStream(zoomFolder, tileFile);

        Assert.IsNotNull(stream);
        Assert.IsNotInstanceOf<MemoryStream>(stream,
            "LoadTileStream should return the FileStream directly, not copy into a MemoryStream.");
        Assert.IsTrue(stream.CanRead);
        using var bmp = SkiaSharp.SKBitmap.Decode(stream);
        Assert.IsNotNull(bmp, "Stream must be decodable by SKBitmap.Decode.");
    }

    [Test]
    public async Task LoadTileStreamAsync_ReturnsFileStreamDirectly_WithoutMemoryCopy()
    {
        var tileDir = await ArrangeTiles(SmallFileName);
        SvgPlatform.Init();

        var rndr = new TileRenderer(1, 1);
        var zoomFolder = Path.Combine(tileDir, "z0");
        var firstTile = Directory.EnumerateFiles(zoomFolder, "*.png").First();
        var tileFile = Path.GetFileName(firstTile);

        using var stream = await rndr.LoadTileStreamAsync(zoomFolder, tileFile);

        Assert.IsNotNull(stream);
        Assert.IsNotInstanceOf<MemoryStream>(stream,
            "LoadTileStreamAsync should return the FileStream directly, not copy into a MemoryStream.");
    }

    [Test]
    public async Task RenderBitmap_OffscreenTilesAreNotLoaded()
    {
        // Arrange tiles for the small mountain test asset.
        var tileDir = await ArrangeTiles(SmallFileName);

        // The render loop iterates over a rectangular tile range that exceeds the visible clip rect at its
        // right and bottom edges. With the fix, only visible tiles call the provider.
        //
        // Viewport 800x600, offset 0, zoomFactor 1, tileSizeAtZoom 256:
        //   startTileX = 0, endTileX = ceil(800/256) = 4  → 5 iterations
        //   startTileY = 0, endTileY = ceil(600/256) = 3  → 4 iterations
        //   Full grid: 20 iterations.
        //   Visible tiles (intersecting clip 0..800 x 0..600 at 256 stride): tileX=0..3 (4) * tileY=0..2 (3) = 12.
        int loadCalls = 0;
        Stream CountingProvider(string folder, string file)
        {
            Interlocked.Increment(ref loadCalls);
            var path = Path.Combine(tileDir, folder, file);
            return File.Exists(path) ? File.OpenRead(path) : null;
        }

        var rndr = new TileRenderer(800, 600);
        using var _ = rndr.RenderBitmap(CountingProvider, 0, 0, 1f);

        TestContext.WriteLine($"Provider calls: {loadCalls}");
        Assert.AreEqual(12, loadCalls,
            $"Expected only visible tiles to be loaded (12); got {loadCalls} provider calls.");
    }

    private static void AssertRenderedFile(string path, int expectedWidth, int expectedHeight)
    {
        File.Exists(path).ShouldBeTrue($"renderer should have written output file: {path}");
        new FileInfo(path).Length.ShouldBeGreaterThan(0, "output file should not be empty");

        using var decoded = SKBitmap.Decode(path);
        decoded.ShouldNotBeNull("output file should be decodable as an image");
        decoded.Width.ShouldBe(expectedWidth);
        decoded.Height.ShouldBe(expectedHeight);

        // Ensure at least some tiles were drawn — a fully transparent/black bitmap means nothing rendered.
        var pixels = decoded.Pixels;
        pixels.Any(p => p.Alpha != 0 && (p.Red != 0 || p.Green != 0 || p.Blue != 0))
            .ShouldBeTrue("rendered bitmap should contain at least one non-empty pixel");
    }

    private async Task<string> ArrangeTiles(string fileName)
    {
        SvgPlatform.Init();

        var file = Path.Combine(TestContext.CurrentContext.WorkDirectory, "Assets", $"{fileName}.jpg");
        var tileDir = Path.Combine(TestContext.CurrentContext.WorkDirectory, "Assets", $"tiles_{fileName}");
        var td = new DirectoryInfo(tileDir);
        if (!td.Exists)
        {
            await new TileGenerator().GenerateTilesAsync(file, CreateStreamProvider(tileDir));
        }
        return tileDir;
    }

    private sealed class TrackingTileCache : ITileCache
    {
        private readonly ITileCache _inner;
        public int HitCount { get; private set; }
        public int MissCount { get; private set; }

        public TrackingTileCache(TileCacheOptions options) => _inner = new TileCache(options);

        public void Reset() { HitCount = 0; MissCount = 0; }

        public TileCacheItem GetOrCreate(string key, Func<SKBitmap> itemProvider)
        {
            bool miss = false;
            var result = _inner.GetOrCreate(key, () => { miss = true; return itemProvider(); });
            if (miss) MissCount++; else HitCount++;
            return result;
        }

        public async Task<TileCacheItem> GetOrCreateAsync(string key, Func<Task<SKBitmap>> itemProvider)
        {
            bool miss = false;
            var result = await _inner.GetOrCreateAsync(key, async () => { miss = true; return await itemProvider(); });
            if (miss) MissCount++; else HitCount++;
            return result;
        }

        public bool TryGetValue(string key, out TileCacheItem item) => _inner.TryGetValue(key, out item);
        public void Remove(string key) => _inner.Remove(key);
        public void Dispose() => _inner.Dispose();
    }
}
