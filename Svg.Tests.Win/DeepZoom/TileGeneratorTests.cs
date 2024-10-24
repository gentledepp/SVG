using NUnit.Framework;
using Svg.DeepZoom;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Svg.Tests.Win
{
    //public class TileGeneratorTests
    //{
    //    const string SmallFileName = "Assets\\mountain_4000x1800";
    //    private const string LargeFileName = "Assets\\landscape_12000x6000";
    //    private ITileGenerator _tileService;

    //    [SetUp]
    //    public void SetUp()
    //    {
    //        SvgPlatform.Init();

    //        _tileService = new TileGenerator();
    //    }

    //    [Test]
    //    public async Task L_CanCreateTiles()
    //    {
    //        var file = Path.Combine(Environment.CurrentDirectory, $"{SmallFileName}.jpg");
    //        var tileDir = Path.Combine(Environment.CurrentDirectory, $"tiles_{SmallFileName}");
    //        var progressValue = 0;
    //        var progress = new Progress<int>();
    //        progress.ProgressChanged += (sender, i) =>
    //        {
    //            progressValue = i;
    //        };

    //        var td = new DirectoryInfo(tileDir);
    //        if (td.Exists)
    //            td.Delete(true);

    //        await _tileService.GenerateTilesAsync(file, null, progress);

    //        var tiles = Directory.EnumerateFiles(tileDir, "*.*", SearchOption.AllDirectories);
    //        Assert.True(tiles.Any());
    //        Assert.True(progressValue == 100);

    //    }

    //    [Test]
    //    public async Task XL_CanCreateTiles()
    //    {
    //        var file = Path.Combine(Environment.CurrentDirectory, $"{LargeFileName}.jpg");
    //        var tileDir = Path.Combine(Environment.CurrentDirectory, $"tiles_{LargeFileName}");
    //        var td = new DirectoryInfo(tileDir);
    //        if (td.Exists)
    //            td.Delete(true);
    //        var progressValue = 0;
    //        var progress = new Progress<int>();
    //        progress.ProgressChanged += (sender, i) =>
    //        {
    //            progressValue = i;
    //        };

    //        var gen = new TileGenerator();

    //        await gen.GenerateTilesAsync(file, null, progress);

    //        var tiles = Directory.EnumerateFiles(tileDir, "*.*", SearchOption.AllDirectories);
    //        Assert.True(tiles.Any());
    //        Assert.True(progressValue == 100);
    //    }

    //    [Test]
    //    public async Task XL_CanCreateTilesAsync()
    //    {

    //        var file = Path.Combine(Environment.CurrentDirectory, $"{LargeFileName}.jpg");
    //        var tileDir = Path.Combine(Environment.CurrentDirectory, $"tiles_{LargeFileName}");
    //        var td = new DirectoryInfo(tileDir);
    //        if (td.Exists)
    //            td.Delete(true);
    //        var progressValue = 0;
    //        var progress = new Progress<int>();
    //        progress.ProgressChanged += (sender, i) =>
    //        {
    //            progressValue = i;
    //        };

    //        var gen = new TileGenerator();

    //        await gen.GenerateTilesAsync(file, null, progress);

    //        var tiles = Directory.EnumerateFiles(tileDir, "*.*", SearchOption.AllDirectories);
    //        Assert.True(tiles.Any());
    //        Assert.True(progressValue == 100);
    //    }

    //    [Test]
    //    public async Task XL_CanCreateTilesAsyncInParallel()
    //    {
    //        var file = Path.Combine(Environment.CurrentDirectory, $"{LargeFileName}.jpg");
    //        var tileDir = Path.Combine(Environment.CurrentDirectory, $"tiles_{LargeFileName}");
    //        var td = new DirectoryInfo(tileDir);
    //        if (td.Exists)
    //            td.Delete(true);
    //        var progressValue = 0;
    //        var progress = new Progress<int>();
    //        progress.ProgressChanged += (sender, i) =>
    //        {
    //            progressValue = i;
    //        };
    //        var gen = new TileGenerator();

    //        using var fStream = File.OpenWrite(file);

    //        await gen.GenerateTilesAsync(fStream, null, progress: progress, backgroundColor: "#ffffff", maxParallelTasks: -1);

    //        var tiles = Directory.EnumerateFiles(tileDir, "*.*", SearchOption.AllDirectories);
    //        Assert.True(tiles.Any());
    //        Assert.True(progressValue == 100);
    //    }

    //    [Test]
    //    public async Task XL_CanCreateTilesAsyncInParallel_LimitingParallelizationToOne()
    //    {
    //        var file = Path.Combine(Environment.CurrentDirectory, $"{LargeFileName}.jpg");
    //        var tileDir = Path.Combine(Environment.CurrentDirectory, $"tiles_{LargeFileName}");
    //        var td = new DirectoryInfo(tileDir);
    //        if (td.Exists)
    //            td.Delete(true);
    //        var progressValue = 0;
    //        var progress = new Progress<int>();
    //        progress.ProgressChanged += (sender, i) =>
    //        {
    //            progressValue = i;
    //        };
    //        var gen = new TileGenerator();

    //        using var fStream = File.OpenWrite(file);

    //        await gen.GenerateTilesAsync(fStream, null, progress: progress, backgroundColor: "#ffffff", maxParallelTasks: -1);

    //        var tiles = Directory.EnumerateFiles(tileDir, "*.*", SearchOption.AllDirectories);
    //        Assert.True(tiles.Any());
    //        Assert.True(progressValue == 100);
    //    }
    //}
}
