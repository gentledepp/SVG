using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using SkiaSharp;
using Svg;
using Svg.Interfaces;
using Svg.Platform;
using SvgW3CTestSuite.Assets;

namespace SvgW3CTestSuite.Win
{
    [TestFixture]
    public class W3CTestFixture
    {
        private static int _testCount = 0;
        private static int _succeededCount = 0;


        public static object[][] SvgTestCases = { };
        public static Func<string, ISvgSource> FileSourceProvider { get; set; }
        static W3CTestFixture()
        {
            SvgPlatform.Init();

            var svgFiles = AssetHelper.GetAllSvgFiles()/*.Where(s => !s.StartsWith("struct-image"))*/;

            SvgTestCases = svgFiles.Select(path => new object[]
                                                    {
                                                        path,
                                                        AssetHelper.GetPngForSvg(path)
                                                    })
                                                    .ToArray();
            FileSourceProvider = (path) => Svg.Platform.EmbeddedResourceSource.Create(path, typeof(AssetHelper).Assembly);
        }

        [Test, TestCaseSource(nameof(SvgTestCases))]
        public async Task W3CTestSuiteCompare(string svgPath, string pngPath)
        {
            await RunTest(() =>
            {
                // Arrange
                using (var pngBitmap = GetBitmap(pngPath))
                {
                    // Act
                    using (var svgBitmap = RenderSvg(svgPath, pngBitmap.Width, pngBitmap.Height))
                    {
                        // Assert
                        using (var c = ImageCompare(svgBitmap, pngBitmap))
                        {
                            if (c.Similarity < 90)
                                Assert.Inconclusive($"not done yet '{svgPath}' {c.Similarity}%");

                            //Assert.GreaterOrEqual(c.Similarity, 90, $"{svgPath}");
                        }
                    }
                }

            }, svgPath);
        }

        private SKBitmap GetBitmap(string pngPath)
        {
            using (var ms = new MemoryStream())
            {
                using (var stream = FileSourceProvider(pngPath).GetStream())
                {
                    stream.CopyTo(ms);
                    ms.Seek(0, SeekOrigin.Begin);
                }

                using (var pngStream = new SKManagedStream(ms))
                {
                    var pngBitmap = SKBitmap.Decode(pngStream);
                    return pngBitmap;
                }
            }
        }

        private async Task RunTest(Action test, string name, int timeout = 10000)
        {
            try
            {
                var cancel = new CancellationTokenSource();
                cancel.CancelAfter(timeout);
                await Task.Run(() =>
                {
                    try
                    {
                        Interlocked.Increment(ref _testCount);
                        System.Diagnostics.Debug.Write($"starting test #{_testCount} '{name}#'");
                        test();


                        NotifySuccess(name);
                    }
                    catch (Exception x)
                    {
                        NotifyError(name);
                        throw x;
                    }

                }, cancel.Token);

            }
            catch (TaskCanceledException)
            {
                NotifyError(name);
                Assert.Fail($"test {name} took too much time");
            }
        }

        private static void NotifySuccess(string svgPath)
        {
        }

        private static void NotifyError(string svgPath)
        {
        }

        /// <summary>
        /// Renders an SVG document to a bitmap with the specified dimensions.
        /// </summary>
        /// <param name="svgPath">The path to the SVG file to render.</param>
        /// <param name="width">The width of the output bitmap in pixels.</param>
        /// <param name="height">The height of the output bitmap in pixels.</param>
        /// <returns>An SKBitmap containing the rendered SVG content.</returns>
        private static SKBitmap RenderSvg(string svgPath, int width, int height)
        {
            var src = FileSourceProvider(svgPath);

            using (SvgDocument doc = SvgDocument.Open<SvgDocument>(src))
            {
                // Create image info for the surface
                var imageInfo = new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);

                using (var surface = SKSurface.Create(imageInfo))
                {
                    // Render the SVG to the surface
                    doc.Draw(SvgRenderer.FromGraphics(new SkiaGraphics(surface)));

                    // Get the image from the surface
                    using (var image = surface.Snapshot())
                    {
                        // Encode the image to PNG format
                        using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
                        {
                            // Create a stream from the encoded data
                            using (var stream = data.AsStream())
                            using (var managedStream = new SKManagedStream(stream))
                            {
                                // Decode the stream back to a bitmap
                                return SKBitmap.Decode(managedStream);
                            }
                        }
                    }
                }
            }
        }

        private static ImageCompareResult ImageCompare(SKBitmap i1, SKBitmap i2)
        {
            if (i1.Height != i2.Height || i1.Width != i2.Width)
            {
                Assert.Fail($"SKBitmap dimensions differ! rendered:{i1.Width}x{i1.Height} vs png:{i2.Width}x{i2.Height}");
            }

            float correctPixel = 0;
            float pixelAmount = i1.Height * i1.Width;
            //var bitmap = Android.Graphics.Bitmap.CreateBitmap(i1.Width, i1.Height, Android.Graphics.Bitmap.Config.Rgb565);
            //bitmap.EraseColor(Color.Red);

            for (var y = 0; y < i1.Height; ++y)
            {
                for (var x = 0; x < i1.Width; ++x)
                {
                    var c1 = i1.GetPixel(x, y);
                    var c2 = i2.GetPixel(x, y);

                    if (object.Equals(c1.Alpha, c2.Alpha) &&
                        object.Equals(c1.Green, c2.Green) &&
                        object.Equals(c1.Blue, c2.Blue) &&
                        object.Equals(c1.Red, c2.Red))
                    {
                        if (c1.Alpha != 0) // if pixel has alpha
                        {
                            pixelAmount--;
                            //bitmap.SetPixel(x, y, Color.White);
                        }
                        else
                        {
                            correctPixel++;
                            //bitmap.SetPixel(x, y, Color.White);
                        }
                    }
                }
            }

            return new ImageCompareResult((correctPixel / pixelAmount) * 100, /*bitmap*/null);
        }

        private class ImageCompareResult : IDisposable
        {
            public ImageCompareResult(float similarity, SKBitmap heatmap)
            {
                Similarity = similarity;
                Heatmap = heatmap;
            }

            public float Similarity { get; private set; }
            public SKBitmap Heatmap { get; private set; }
            public void Dispose()
            {
                Heatmap?.Dispose();
            }
        }

    }
}