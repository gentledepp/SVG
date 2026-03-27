using System;
using System.IO;
using System.IO.Compression;
using NUnit.Framework;
using SkiaSharp;
using Svg;

namespace Svg.Tests.Win.DeepZoom;

/// <summary>
/// Verifies that the zip tile provider in SvgImage opens the archive exactly once
/// per render pass (not once per tile).
/// </summary>
public class SvgImageZipTileProviderTests
{
    private static ZipArchive BuildInMemoryZipWithTile(string entryName, out byte[] pngBytes)
    {
        // Create a tiny 1x1 red PNG in memory.
        using var bmp = new SKBitmap(1, 1);
        bmp.SetPixel(0, 0, SKColors.Red);
        using var pngStream = new MemoryStream();
        using (var w = new SKManagedWStream(pngStream))
            bmp.Encode(w, SKEncodedImageFormat.Png, 100);
        pngBytes = pngStream.ToArray();

        var zipStream = new MemoryStream();
        using (var zip = new ZipArchive(zipStream, ZipArchiveMode.Create, leaveOpen: true))
        {
            var entry = zip.CreateEntry(entryName);
            using var es = entry.Open();
            es.Write(pngBytes, 0, pngBytes.Length);
        }

        zipStream.Position = 0;
        return new ZipArchive(zipStream, ZipArchiveMode.Read, leaveOpen: true);
    }

    [Test]
    public void CreateZipTileProvider_ReturnsTileStreamForExistingEntry()
    {
        // Act: call the internal factory — will fail to compile until SvgImage.CreateZipTileProvider exists.
        using var archive = BuildInMemoryZipWithTile("z0/y0_x0.png", out var pngBytes);

        var provider = SvgImage.CreateZipTileProvider(archive, Path.Combine);

        using var stream = provider("z0", "y0_x0.png");

        Assert.IsNotNull(stream, "Provider should return a non-null stream for an existing tile entry.");
        var decoded = SKBitmap.Decode(stream);
        Assert.IsNotNull(decoded, "Returned stream must be decodable as a bitmap.");
    }

    [Test]
    public void CreateZipTileProvider_ReturnsNullForMissingEntry()
    {
        using var archive = BuildInMemoryZipWithTile("z0/y0_x0.png", out _);

        var provider = SvgImage.CreateZipTileProvider(archive, Path.Combine);

        var stream = provider("z0", "y99_x99.png");
        Assert.IsNull(stream, "Provider should return null for a tile that does not exist in the archive.");
    }

    [Test]
    public void CreateZipTileProvider_DoesNotReopenArchiveOnEachCall()
    {
        // Pre-populate zip with two tiles so we can call the provider twice.
        var zipStream = new MemoryStream();
        using (var zip = new ZipArchive(zipStream, ZipArchiveMode.Create, leaveOpen: true))
        {
            foreach (var name in new[] { "z0/y0_x0.png", "z0/y1_x0.png" })
            {
                using var bmp = new SKBitmap(1, 1);
                using var pngMem = new MemoryStream();
                using (var w = new SKManagedWStream(pngMem))
                    bmp.Encode(w, SKEncodedImageFormat.Png, 100);
                var entry = zip.CreateEntry(name);
                using var es = entry.Open();
                pngMem.WriteTo(es);
            }
        }

        zipStream.Position = 0;
        using var archive = new ZipArchive(zipStream, ZipArchiveMode.Read, leaveOpen: true);

        // The provider captures the already-open archive; calling it N times must NOT throw
        // ObjectDisposedException, which is what would happen if the archive were disposed after the first call.
        var provider = SvgImage.CreateZipTileProvider(archive, Path.Combine);

        Assert.DoesNotThrow(() =>
        {
            using var s1 = provider("z0", "y0_x0.png");
            using var s2 = provider("z0", "y1_x0.png");
            Assert.IsNotNull(s1);
            Assert.IsNotNull(s2);
        }, "Provider should work for multiple calls on the same archive without reopening it.");
    }
}
