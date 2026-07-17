using NUnit.Framework;
using SkiaSharp;
using Svg.Editor.Core.Test;
using Svg.Editor.Services;
using Svg.Interfaces;
using System.Threading.Tasks;

namespace Svg.Editor.Core.Tests
{
    // Regression for the reported "coloring is inconsistent / the first time I change the color
    // nothing happens" bug. The element model updated immediately, but each SvgVisualElement caches
    // its fill/stroke brush on the renderer and only rebuilds it when its attribute-change token
    // changes. That token starts as Guid.Empty and only starts tracking after the first render, so
    // the very first color change (Empty -> real token) was swallowed by the "do nothing initially"
    // branch and the stale (old-color) brush kept being drawn until a second change occurred.
    [TestFixture]
    public class RenderCacheColorTests : SvgDrawingCanvasTestBase
    {
        [Test]
        public async Task WhenElementFillChangesForTheFirstTime_ItIsRenderedInTheNewColor()
        {
            // Arrange - a shape that is initially black, sized to fill the view
            const int w = 200, h = 200;
            await Canvas.EnsureInitialized();
            Canvas.ScreenWidth = w;
            Canvas.ScreenHeight = h;

            var doc = new SvgDocument { ViewBox = new SvgViewBox(0, 0, w, h) };
            var rect = new SvgRectangle
            {
                X = 40, Y = 40, Width = 120, Height = 120,
                Fill = new SvgColourServer(Color.Create("#000000"))
            };
            doc.Children.Add(rect);
            Canvas.Document = doc;

            using var surface = SKSurface.Create(new SKImageInfo(w, h, SKImageInfo.PlatformColorType, SKAlphaType.Premul));
            var renderer = new SKCanvasRenderer(surface, w, h);

            // First render caches the (black) brush for the rectangle on the renderer.
            await Canvas.OnDraw(renderer);
            var blackBefore = CountPixels(surface, isBlack: true);
            var redBefore = CountPixels(surface, isBlack: false);

            // Act - change the fill for the FIRST time, then render again on the SAME canvas
            // (the render cache lives on the canvas' renderer and persists across draws).
            rect.Fill = new SvgColourServer(Color.Create("#FF0000"));
            await Canvas.OnDraw(renderer);
            var redAfter = CountPixels(surface, isBlack: false);

            // Assert
            Assert.That(blackBefore, Is.GreaterThan(100), "sanity: the shape should be visible and black initially");
            Assert.That(redBefore, Is.EqualTo(0), "sanity: nothing should be red before the color change");
            Assert.That(redAfter, Is.GreaterThan(100),
                "The first color change was not rendered - the stale cached brush was reused");
        }

        [Test]
        public async Task WhenElementStrokeChangesForTheFirstTime_ItIsRenderedInTheNewColor()
        {
            // Arrange - an unfilled shape with a thick black stroke, sized to fill the view
            const int w = 200, h = 200;
            await Canvas.EnsureInitialized();
            Canvas.ScreenWidth = w;
            Canvas.ScreenHeight = h;

            var doc = new SvgDocument { ViewBox = new SvgViewBox(0, 0, w, h) };
            var rect = new SvgRectangle
            {
                X = 40, Y = 40, Width = 120, Height = 120,
                Fill = SvgPaintServer.None,
                Stroke = new SvgColourServer(Color.Create("#000000")),
                StrokeWidth = new SvgUnit(SvgUnitType.Pixel, 12)
            };
            doc.Children.Add(rect);
            Canvas.Document = doc;

            using var surface = SKSurface.Create(new SKImageInfo(w, h, SKImageInfo.PlatformColorType, SKAlphaType.Premul));
            var renderer = new SKCanvasRenderer(surface, w, h);

            // First render caches the (black) stroke brush for the rectangle on the renderer.
            await Canvas.OnDraw(renderer);
            var blackBefore = CountPixels(surface, isBlack: true);
            var redBefore = CountPixels(surface, isBlack: false);

            // Act - change the stroke for the FIRST time, then render again on the SAME canvas.
            rect.Stroke = new SvgColourServer(Color.Create("#FF0000"));
            await Canvas.OnDraw(renderer);
            var redAfter = CountPixels(surface, isBlack: false);

            // Assert
            Assert.That(blackBefore, Is.GreaterThan(100), "sanity: the stroke should be visible and black initially");
            Assert.That(redBefore, Is.EqualTo(0), "sanity: nothing should be red before the color change");
            Assert.That(redAfter, Is.GreaterThan(100),
                "The first stroke color change was not rendered - the stale cached brush was reused");
        }

        private static int CountPixels(SKSurface surface, bool isBlack)
        {
            using var image = surface.Snapshot();
            using var bmp = SKBitmap.FromImage(image);
            var count = 0;
            for (var y = 0; y < bmp.Height; y++)
                for (var x = 0; x < bmp.Width; x++)
                {
                    var p = bmp.GetPixel(x, y);
                    if (isBlack)
                    {
                        if (p.Red < 60 && p.Green < 60 && p.Blue < 60 && p.Alpha > 200) count++;
                    }
                    else
                    {
                        if (p.Red > 150 && p.Green < 90 && p.Blue < 90) count++;
                    }
                }
            return count;
        }
    }
}
