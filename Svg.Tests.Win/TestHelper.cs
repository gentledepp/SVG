using NUnit.Framework;
using SkiaSharp;
using Svg.Interfaces;
using Svg.Platform;
using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace Svg.Tests.Win
{
    public static class TestHelper
    {
        public static SKBitmap RenderSvg(string svgPath, int width, int height)
        {
            return RenderSvg(svgPath, width, height, null);
        }
        public static SKBitmap RenderSvg(string svgPath, int width, int height, Color backgroundColor)
        {
            if(!Path.IsPathRooted(svgPath))
                svgPath = System.IO.Path.Combine(TestContext.CurrentContext.TestDirectory, "Assets", svgPath);

            using var src = File.OpenRead(svgPath);

            using SvgDocument doc = SvgDocument.Open<SvgDocument>(src);
            using var surface = SKSurface.Create(new SKImageInfo(width, height));

            using var renderer = SvgRenderer.FromGraphics(new SkiaGraphics(surface));
            if(backgroundColor != null)
                renderer.FillBackground(backgroundColor);

            doc.Draw(renderer);
            using var img = surface.Snapshot();
            return SKBitmap.FromImage(img);
        }

        public static ImageCompareResult ImageCompare(SKBitmap actual, SKBitmap expected)
        {
            float correctPixel = 0;
            float pixelAmount = Math.Max(actual.Height, expected.Height) * Math.Max(actual.Width, expected.Width);
            var bitmap = new SKBitmap(Math.Max(actual.Width, expected.Width),
                Math.Max(actual.Height, expected.Height),
                SKColorType.Rgb565,
                SKAlphaType.Opaque);
            var red = SKColor.Parse("#FF0000");
            var white = SKColor.Parse("#FFFFFF");
            var yellow = SKColor.Parse("#FFFF00");
            for (var y = 0; y < bitmap.Height; ++y)
            {
                for (var x = 0; x < bitmap.Width; ++x)
                {
                    // color heat map yellow if the image sizes differ
                    if (x >= actual.Width || y >= actual.Height)
                    {
                        bitmap.SetPixel(x, y, yellow);
                        continue;
                    }
                    if (x >= expected.Width || y >= expected.Height)
                    {
                        bitmap.SetPixel(x, y, yellow);
                        continue;
                    }


                    var c1 = actual.GetPixel(x, y);
                    var c2 = expected.GetPixel(x, y);

                    if (object.Equals(c1.Alpha, c2.Alpha) &&
                        object.Equals(c1.Green, c2.Green) &&
                        object.Equals(c1.Blue, c2.Blue) &&
                        object.Equals(c1.Red, c2.Red))
                    {
                        correctPixel++;
                        bitmap.SetPixel(x, y, white);
                    }
                }
            }

            return new ImageCompareResult((correctPixel / pixelAmount) * 100, bitmap, actual);
        }

        public static SKBitmap GetBitmap(string pngPath)
        {
            if (!Path.IsPathRooted(pngPath))
                pngPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Assets", pngPath);

            using var stream = File.OpenRead(pngPath);
            return SKBitmap.Decode(stream);
        }
    }

    public class ImageCompareResult : IDisposable
    {
        public ImageCompareResult(float similarity, SKBitmap heatmap, SKBitmap actualResult)
        {
            Similarity = similarity;
            Heatmap = heatmap;
            ActualResult = actualResult;
        }

        public float Similarity { get; private set; }
        public SKBitmap Heatmap { get; private set; }
        public SKBitmap ActualResult { get; private set; }

        public void Dispose()
        {
            Heatmap?.Dispose();
            ActualResult?.Dispose();
        }
    }

    public static class ImageCompareResultExtensions
    {
        public static void AssertAreSimilar(this ImageCompareResult res, 
            float similarity, 
            string svgPath, 
            string postFix = null,
            [CallerMemberName] string testMethodName = null)
        {
            if (res.Similarity < similarity)
            {
                using (var heatmapImage = SKImage.FromBitmap(res.Heatmap))
                using (var heatmapData = heatmapImage.Encode(SKEncodedImageFormat.Png, 100))
                {
                    File.WriteAllBytes($"{testMethodName}{postFix}_difference.png", heatmapData.ToArray());
                }
                using (var actualImage = SKImage.FromBitmap(res.ActualResult))
                using (var actualData = actualImage.Encode(SKEncodedImageFormat.Png, 100))
                {
                    File.WriteAllBytes($"{testMethodName}{postFix}_actual.png", actualData.ToArray());
                }
                Console.WriteLine($"Saved heatmap in {Path.Combine(Environment.CurrentDirectory, $"{testMethodName}{postFix}_difference.png")}");
            }
            Assert.GreaterOrEqual(res.Similarity, similarity);
        }
    }

}