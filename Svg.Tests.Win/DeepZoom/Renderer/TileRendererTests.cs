using NUnit.Framework;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System;
using Svg.DeepZoom;

namespace Svg.Tests.Win.Renderer;

public class TileRendererTests
{
    //const string SmallFileName = "Assets\\mountain_4000x1800";
    //private const string LargeFileName = "Assets\\landscape_12000x6000";

    //[Test]
    //[TestCase(2.0f)]
    //[TestCase(1.5f)]
    //[TestCase(1.0f)]
    //[TestCase(0.5f)]
    //[TestCase(0.25f)]
    //[TestCase(0.125f)]
    //public async Task CanRenderAtOriginWithDifferentZoomFactors(float zoomFactor)
    //{
    //    var fileName = SmallFileName;
    //    var tileDir = await ArrangeTiles(fileName);

    //    var rndr = new TileRenderer(800, 600, new TileCacheOptions(TimeSpan.FromHours(1)));
    //    var fn = Path.Combine(Environment.CurrentDirectory, "tiles_Assets","mountain_4000x1800", $"x0_y0-z{(Math.Max((int)Math.Round(4/2*zoomFactor), 0)).ToString(CultureInfo.InvariantCulture)} zoo{zoomFactor} -rnder{rndr.Width}x{rndr.Height}.png");


    //    rndr.RenderBitmap(tileDir, fn, 0, 0, zoomFactor);
    //}

    //[Test]
    //[TestCase(2.0f)]
    //[TestCase(1.5f)]
    //[TestCase(1.0f)]
    //[TestCase(0.5f)]
    //[TestCase(0.25f)]
    //public async Task CanRenderAtImageCenterWithDifferentZoomFactors(float zoomFactor)
    //{
    //    var fileName = SmallFileName;
    //    var tileDir = await ArrangeTiles(fileName);
    //    var rndr = new TileRenderer(800, 600, new TileCacheOptions(TimeSpan.FromHours(1)));
    //    var fn = Path.Combine(Environment.CurrentDirectory, $"x1700_y900z-{(1f / zoomFactor).ToString(CultureInfo.InvariantCulture)}-rnder{rndr.Width}x{rndr.Height}.jpeg");

    //    var cx = (4000 / 2) - (rndr.Width/2);
    //    var cy = (1800 / 2) - (rndr.Height / 2);

    //    var x = cx;// * zoomFactor;
    //    var y = cy;// * zoomFactor;

    //    rndr.RenderBitmap(tileDir, fn, x, y, zoomFactor);
    //}


    //[Test]
    //[TestCase(2.0f)]
    //[TestCase(1.5f)]
    //[TestCase(1.0f)]
    //[TestCase(0.5f)]
    //[TestCase(0.25f)]
    //[TestCase(0.125f)]
    //[TestCase(0.0625f)]
    //[TestCase(0.03125f)]
    //public async Task CanRenderHugeImageAtOriginWithDifferentZoomFactors(float zoomFactor)
    //{
    //    var fileName = SmallFileName;
    //    var tileDir = await ArrangeTiles(fileName);
    //    var rndr = new TileRenderer(1131, 703, new TileCacheOptions(TimeSpan.FromHours(1)));
    //    var fn = Path.Combine(Environment.CurrentDirectory, $"{fileName}x0_y0-z{(1f / zoomFactor).ToString(CultureInfo.InvariantCulture)}-rnder{rndr.Width}x{rndr.Height}.jpeg");


    //    await rndr.RenderBitmapAsync(tileDir, fn, 0, 0, zoomFactor);
    //}


    //[Test]
    //[TestCase(2.0f)]
    //[TestCase(1.5f)]
    //[TestCase(1.0f)]
    //[TestCase(0.5f)]
    //[TestCase(0.25f)]
    //[TestCase(0.125f)]
    //public async Task CanRenderAsyncAtOriginWithDifferentZoomFactors(float zoomFactor)
    //{
    //    var fileName = SmallFileName;
    //    var tileDir = await ArrangeTiles(fileName);
    //    var rndr = new TileRenderer(800, 600, new TileCacheOptions(TimeSpan.FromHours(1)));
    //    var fn = Path.Combine(Environment.CurrentDirectory, $"x0_y0-z{(1f / zoomFactor).ToString(CultureInfo.InvariantCulture)}-rnder{rndr.Width}x{rndr.Height}.jpeg");


    //    await rndr.RenderBitmapAsync(tileDir, fn, 0, 0, zoomFactor);
    //}

    //[Test]
    //[TestCase(2.0f)]
    //[TestCase(1.5f)]
    //[TestCase(1.0f)]
    //[TestCase(0.5f)]
    //[TestCase(0.25f)]
    //public async Task CanRenderAsyncAtImageCenterWithDifferentZoomFactors(float zoomFactor)
    //{
    //    var fileName = SmallFileName;
    //    var tileDir = await ArrangeTiles(fileName);

    //    var rndr = new TileRenderer(800, 600, new TileCacheOptions(TimeSpan.FromHours(1)));
    //    var fn = Path.Combine(Environment.CurrentDirectory, $"x1700_y900z-{(1f / zoomFactor).ToString(CultureInfo.InvariantCulture)}-rnder{rndr.Width}x{rndr.Height}.jpeg");

    //    var cx = (4000 / 2) - (rndr.Width / 2);
    //    var cy = (1800 / 2) - (rndr.Height / 2);

    //    var x = cx;// * zoomFactor;
    //    var y = cy;// * zoomFactor;

    //    await rndr.RenderBitmapAsync(tileDir, fn, x, y, zoomFactor);
    //}


    //[Test]
    //[TestCase(4.0f)]
    //[TestCase(2.0f)]
    //[TestCase(1.5f)]
    //[TestCase(1.0f)]
    //[TestCase(0.5f)]
    //[TestCase(0.25f)]
    //[TestCase(0.125f)]
    //[TestCase(0.0625f)]
    //[TestCase(0.03125f)]
    //public async Task CanRenderAsyncHugeImageAtOriginWithDifferentZoomFactors(float zoomFactor)
    //{
    //    var fileName = LargeFileName;
    //    var tileDir = await ArrangeTiles(fileName);

    //    var rndr = new TileRenderer(800, 600, new TileCacheOptions(TimeSpan.FromHours(1)));
    //    var fn = Path.Combine(Environment.CurrentDirectory, $"{fileName}x0_y0-z{(zoomFactor).ToString(CultureInfo.InvariantCulture)}-rnder{rndr.Width}x{rndr.Height}.jpeg");

    //    await rndr.RenderBitmapAsync(tileDir, fn, 0, 0, zoomFactor);
    //}


    //[Test]
    //[TestCase(2.0f)]
    //[TestCase(1.5f)]
    //[TestCase(1.0f)]
    //[TestCase(0.5f)]
    //[TestCase(0.25f)]
    //[TestCase(0.125f)]
    //public async Task CanRenderAsyncAtOriginWithDifferentZoomFactors_UsingCache(float zoomFactor)
    //{
    //    var fileName = SmallFileName;
    //    var tileDir = await ArrangeTiles(fileName);

    //    using var rndr = new TileRenderer(800, 600, new TileCacheOptions(TimeSpan.FromHours(1)));
    //    var fn = Path.Combine(Environment.CurrentDirectory, $"x0_y0-z{(1f / zoomFactor).ToString(CultureInfo.InvariantCulture)}-rnder{rndr.Width}x{rndr.Height}.jpeg");


    //    var sw = new Stopwatch();
    //    sw.Start();

    //    // first render loads all tiles
    //    await rndr.RenderBitmapAsync(tileDir, fn, 0, 0, zoomFactor);

    //    sw.Stop();
    //    var nonCached = sw.Elapsed;

    //    sw.Reset();
    //    sw.Start();

    //    // second render already has all tiles => should be magnitudes faster
    //    await rndr.RenderBitmapAsync(tileDir, fn, 0, 0, zoomFactor);

    //    sw.Stop();
    //    var cached = sw.Elapsed;

    //    _output.WriteLine($"noncached: {nonCached}\ncached:   {cached}");

    //    cached.ShouldBeLessThan(nonCached);

    //}

    //private async Task<string> ArrangeTiles(string fileName)
    //{
    //    SvgPlatform.Init();

    //    var file = Path.Combine(Environment.CurrentDirectory, $"{fileName}.jpg");
    //    var tileDir = Path.Combine(Environment.CurrentDirectory, $"tiles_{fileName}");
    //    var td = new DirectoryInfo(tileDir);
    //    if (!td.Exists)
    //    {
    //        await new TileGenerator().GenerateTilesAsync(file, tileDir, maxParallelTasks: int.MaxValue);
    //    }
    //    return tileDir;
    //}
}