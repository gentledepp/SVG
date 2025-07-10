using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SkiaSharp;
using Svg.Editor.Services;
using Svg.Editor.Tools;
using Svg.Interfaces;

namespace Svg.Editor.Core.Test
{
    [TestFixture]
    public class PerformanceTests : SvgDrawingCanvasTestBase
    {
        [Test]
        [Ignore("")]
        public async Task IfPointerIsMoved_AndNoElementIsSelected_NothingIsMoved()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var tool = Canvas.Tools.OfType<SelectionTool>().Single();
            Canvas.ActiveTool = tool;


            var d = LoadDocument("iso_sketch_large.svg");
            var child = d.Children.OfType<SvgVisualElement>().First(c => c.Visible && c.Displayable);
            Canvas.ScreenWidth = 800;
            Canvas.ScreenHeight = 500;
            Canvas.Document.Children.Add(child);
            var transforms = child.Transforms.Clone();

            Canvas.CanvasInvalidated += async (sender, args) =>
            {
                
                using (var surface = SkiaSharp.SKSurface.Create(800, 600, SKImageInfo.PlatformColorType, SKAlphaType.Premul))
                {
                    await Canvas.OnDraw(new SKCanvasRenderer(surface, 800, 600));
                }
            };

            // Preassert
            Assert.AreEqual(transforms, child.Transforms);

            // Act
            await Move(PointF.Create(100, 200), PointF.Create(200, 100), 2);

            // Assert
            Assert.AreEqual(transforms, child.Transforms);
        }


        [Test]
        [Ignore("")]
        public async Task IfPointerIsMoved_AndNoElementIsSelected_NothingIsMoved_OpenGL()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var tool = Canvas.Tools.OfType<SelectionTool>().Single();
            Canvas.ActiveTool = tool;


            var d = LoadDocument("iso_sketch_large.svg");
            var child = d.Children.OfType<SvgVisualElement>().First(c => c.Visible && c.Displayable);
            Canvas.ScreenWidth = 800;
            Canvas.ScreenHeight = 500;
            Canvas.Document.Children.Add(child);
            var transforms = child.Transforms.Clone();

            Canvas.CanvasInvalidated += async (sender, args) =>
            {

                using (var surface = SkiaSharp.SKSurface.Create(GRContext.Create(GRBackend.OpenGL), new GRBackendRenderTargetDesc()))
                {
                    await Canvas.OnDraw(new SKCanvasRenderer(surface, 800, 600));
                }
            };

            // Preassert
            Assert.AreEqual(transforms, child.Transforms);

            // Act
            await Move(PointF.Create(100, 200), PointF.Create(200, 100), 2);

            // Assert
            Assert.AreEqual(transforms, child.Transforms);
        }


        [Test]
        public async Task CachesSvgRendererAndUpdatesGraphics()
        {
            // Arrange
            await Canvas.EnsureInitialized();

            var d = new TestableSvgDocument();
            Canvas.ScreenWidth = 800;
            Canvas.ScreenHeight = 500;
            Canvas.Document = d;

            // Act: draw 2x with different SKCanvasRenderers
            using var surface1 = SkiaSharp.SKSurface.Create(800, 600, SKImageInfo.PlatformColorType, SKAlphaType.Premul);
            var renderer1 = new SKCanvasRenderer(surface1, 800, 600);
            await Canvas.OnDraw(renderer1);
            var svgR1 = d.UsedRenderers[0];
            var svgR1Graphics = svgR1.Graphics;

            using var surface2 = SkiaSharp.SKSurface.Create(800, 600, SKImageInfo.PlatformColorType, SKAlphaType.Premul);
            var renderer2 = new SKCanvasRenderer(surface2, 800, 600);
            await Canvas.OnDraw(renderer2);
            var svgR2 = d.UsedRenderers[1];
            var svgR2Graphics = svgR2.Graphics;

            // Assert
            Assert.AreEqual(2, d.UsedRenderers.Count, "was rendered twice!");

            Assert.AreSame(svgR1, svgR2, "must be the same as the SvgRenderer must be cached for performance reasons");

            Assert.AreSame(renderer1.Graphics, svgR1Graphics, "SvgRenderer must first have the graphics of renderer1");
            Assert.AreSame(renderer2.Graphics, svgR2Graphics, "SvgRenderer must then be updated with the graphics of renderer2");
        }

        public class TestableSvgDocument : SvgDocument
        {
            public List<ISvgRenderer> UsedRenderers { get; } = new List<ISvgRenderer>();

            protected override void Render(ISvgRenderer renderer)
            {
                UsedRenderers.Add(renderer);
                base.Render(renderer);
            }
        }
    }
}
