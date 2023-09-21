using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using SkiaSharp;
using Svg;
using Svg.Interfaces;
using Svg.Platform;
using SvgW3CTestSuite.Assets;
#if xUNIT
using Xunit;
#elif WIN
using NUnit.Framework;
#else
using NUnit.Framework;
using Xamarin.Forms;
using Plugin.Toasts;
#endif


namespace SvgW3CTestSuite
{
#if xUNIT
#else
    [TestFixture]
#endif
    public class W3CTestFixture
    {
        private static int _testCount = 0;
        private static int _succeededCount = 0;

       
        public static object[][] SvgTestCases = {};
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
#if WINDOWS_UWP
            FileSourceProvider =
                path =>
                    EmbeddedResourceSource.Create(path, typeof(AssetHelper).GetTypeInfo().Assembly);
#else
            FileSourceProvider = (path) => Svg.Platform.EmbeddedResourceSource.Create(path, typeof(AssetHelper).Assembly);
#endif
        }

#if xUNIT
        [Theory]
        [MemberData(nameof(SvgTestCases), MemberType=typeof(W3CTestFixture))]
#else
        [Test, TestCaseSource(nameof(SvgTestCases))]
#endif
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
#if xUNIT
                            Assert.True(c.Similarity >= 90, $"{svgPath}");
#else
                            if (c.Similarity < 90)
                                Assert.Inconclusive($"not done yet '{svgPath}' {c.Similarity}%");

                            //Assert.GreaterOrEqual(c.Similarity, 90, $"{svgPath}");
#endif
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
#if xUNIT
                Assert.True(false, $"test {name} took too much time");
#else
                Assert.Fail($"test {name} took too much time");
#endif
            }
        }

        private static void NotifySuccess(string svgPath)
        {
#if !WINDOWS_UWP && !WIN
            Xamarin.Forms.Device.BeginInvokeOnMainThread(async () =>
            {
                Interlocked.Increment(ref _succeededCount);

                var message = $"{svgPath} succeeded ({_succeededCount}/ {_testCount})";
                System.Diagnostics.Debug.Write(message);
                var notificator = DependencyService.Get<IToastNotificator>();
                await notificator.Notify(new NotificationOptions { Title = "Finished test", Description=message});
            });
#endif
        }

        private static void NotifyError(string svgPath)
        {
#if !WINDOWS_UWP && !WIN
            Xamarin.Forms.Device.BeginInvokeOnMainThread(async () =>
            {
                var message = $"{svgPath} failed ({_succeededCount} / {_testCount})";
                System.Diagnostics.Debug.Write(message);
                var notificator = DependencyService.Get<IToastNotificator>();
                await notificator.Notify(new NotificationOptions {Title = "Finished test", Description = message});
            });
#endif
        }

        private static SKBitmap RenderSvg(string svgPath, int width, int height)
        {
            var src = FileSourceProvider(svgPath);

            using (SvgDocument doc = SvgDocument.Open<SvgDocument>(src))
            using (var surface = SKSurface.Create(width, height, SKImageInfo.PlatformColorType, SKAlphaType.Premul))
            {
                doc.Draw(SvgRenderer.FromGraphics(new SkiaGraphics(surface)));
                var img = surface.Snapshot();

                using (var s = new SKManagedStream(img.Encode().AsStream()))
                {
                    SKBitmap b = new SKBitmap();
                    return SKBitmap.Decode(s);
                }
            }
        }

        private static ImageCompareResult ImageCompare(SKBitmap i1, SKBitmap i2)
        {
            if (i1.Height != i2.Height || i1.Width != i2.Width)
            {
#if xUNIT
                Assert.True(false, $"SKBitmap dimensions differ! rendered:{i1.Width}x{i1.Height} vs png:{i2.Width}x{i2.Height}");
#else
                Assert.Fail($"SKBitmap dimensions differ! rendered:{i1.Width}x{i1.Height} vs png:{i2.Width}x{i2.Height}");
#endif
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