using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Svg.Editor.Events;
using Svg.Editor.Interfaces;
using Svg.Editor.Services;
using Svg.Editor.Tools;
using Svg.Interfaces;

namespace Svg.Editor.Core.Test
{
    [TestFixture]
    public class FreeDrawingToolTests : SvgDrawingCanvasTestBase
    {
        [SetUp]
        protected override void SetupOverride()
        {
            Canvas.LoadTools(() => new FreeDrawingTool(null, SvgEngine.Resolve<IUndoRedoService>()));
        }

        [Test]
        public async Task IfUserTapsCanvas_AndDoesNotMove_NoPathIsDrawn()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var tool = Canvas.Tools.OfType<FreeDrawingTool>().Single();
            Canvas.ActiveTool = tool;

            // Preassert
            Assert.AreEqual(0, Canvas.SelectedElements.Count);

            // Act
            var pt1 = PointF.Create(10, 10);
            await Canvas.OnEvent(new PointerEvent(EventType.PointerDown, pt1, pt1, pt1, 1));
            await Canvas.OnEvent(new PointerEvent(EventType.PointerUp, pt1, pt1, pt1, 1));

            // Assert
            Assert.False(Canvas.Document.Descendants().OfType<SvgLine>().Any());
        }

        [Test]
        public async Task IfUserDrawsPath_AndMovesTooLess_NoPathIsCreated()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var tool = Canvas.Tools.OfType<FreeDrawingTool>().Single();
            Canvas.ActiveTool = tool;

            // Act
            var start = PointF.Create(10, 10);
            var end = PointF.Create(10, 15);
            await Canvas.OnEvent(new PointerEvent(EventType.PointerDown, start, start, start, 1));
            await Canvas.OnEvent(new MoveEvent(start, start, end, end - start, 1));
            await Canvas.OnEvent(new PointerEvent(EventType.PointerUp, end, end, end, 1));

            // Assert
            Assert.False(Canvas.Document.Children.OfType<SvgPath>().Any());
        }

        [Test]
        public async Task IfUserDrawsPath_PathIsCreated()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var tool = Canvas.Tools.OfType<FreeDrawingTool>().Single();
            Canvas.ActiveTool = tool;

            // Act
            var start = PointF.Create(10, 10);
            var end = PointF.Create(10, 100);
            await Canvas.OnEvent(new PointerEvent(EventType.PointerDown, start, start, start, 1));
            await Canvas.OnEvent(new MoveEvent(start, start, end, end - start, 1));
            await Canvas.OnEvent(new PointerEvent(EventType.PointerUp, end, end, end, 1));

            // Assert
            Assert.AreEqual(1, Canvas.Document.Children.OfType<SvgPath>().Count());
        }
    }
}
