using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SkiaSharp;
using Svg.Editor.Services;
using Svg.Editor.Tools;
using Svg.Interfaces;

namespace Svg.Editor.Tests
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
    }
}
