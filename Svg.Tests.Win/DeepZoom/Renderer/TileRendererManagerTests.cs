using System;
using System.IO;
using System.IO.Compression;
using NUnit.Framework;
using SkiaSharp;
using Svg;
using Svg.DeepZoom;

namespace Svg.Tests.Win.DeepZoom.Renderer;

/// <summary>
/// Verifies that <see cref="TileRendererManager"/> caches the open zip archive per path
/// (opened/enumerated once per plan, not per render) and disposes it on teardown.
/// </summary>
public class TileRendererManagerTests
{
    private static string WriteTempZipWithTile(string entryName)
    {
        var path = Path.Combine(Path.GetTempPath(), $"tiles_{Guid.NewGuid():N}.zip");
        using var fs = File.Create(path);
        using var zip = new ZipArchive(fs, ZipArchiveMode.Create);

        using var bmp = new SKBitmap(1, 1);
        using var png = new MemoryStream();
        using (var w = new SKManagedWStream(png))
            bmp.Encode(w, SKEncodedImageFormat.Png, 100);

        var entry = zip.CreateEntry(entryName);
        using var es = entry.Open();
        png.WriteTo(es);
        return path;
    }

    [Test]
    public void GetOrCreateZipTileProvider_ReusesOpenArchive_AndDisposesOnTeardown()
    {
        SvgPlatform.Init();
        var zipPath = WriteTempZipWithTile("z0/y0_x0.png");
        var manager = new TileRendererManager();

        try
        {
            var provider1 = manager.GetOrCreateZipTileProvider(zipPath);
            var provider2 = manager.GetOrCreateZipTileProvider(zipPath);

            // Both providers read from the same cached, still-open archive across calls.
            using (var s1 = provider1("z0", "y0_x0.png"))
                Assert.IsNotNull(s1, "first provider should read an existing tile");
            using (var s2 = provider2("z0", "y0_x0.png"))
                Assert.IsNotNull(s2, "second provider should read from the same cached archive");

            // Tearing the manager down disposes the cached archive: a previously handed-out
            // provider now fails, proving it shared one persistent open archive (not a reopen-per-call).
            manager.DisposeTileRenderer();
            Assert.Catch(() => provider1("z0", "y0_x0.png"),
                "after teardown the cached archive must be disposed");

            // A fresh request reopens the archive and works again.
            var provider3 = manager.GetOrCreateZipTileProvider(zipPath);
            using var s3 = provider3("z0", "y0_x0.png");
            Assert.IsNotNull(s3, "a fresh request should reopen the archive");
        }
        finally
        {
            manager.DisposeTileRenderer();
            File.Delete(zipPath);
        }
    }

    [Test]
    public void GetOrCreateZipTileProvider_ReturnsNullForMissingEntry()
    {
        SvgPlatform.Init();
        var zipPath = WriteTempZipWithTile("z0/y0_x0.png");
        var manager = new TileRendererManager();

        try
        {
            var provider = manager.GetOrCreateZipTileProvider(zipPath);
            Assert.IsNull(provider("z0", "y99_x99.png"), "missing tile must yield null");
        }
        finally
        {
            manager.DisposeTileRenderer();
            File.Delete(zipPath);
        }
    }
}
