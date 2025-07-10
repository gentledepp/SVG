using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Svg.Editor.Events;
using Svg.Editor.Extensions;
using Svg.Editor.Interfaces;
using Svg.Editor.Services;
using Svg.Editor.Tools;
using Svg.Interfaces;

namespace Svg.Editor.Tests
{
    [TestFixture]
    public class LineToolTests : SvgDrawingCanvasTestBase
    {
        [SetUp]
        protected override void SetupOverride()
        {

            SvgEngine.Register<ToolFactoryProvider>(() => new ToolFactoryProvider(new Func<ITool>[]
            {
                () => new SelectionTool(SvgEngine.Resolve<IUndoRedoService>()), 
                () => new GridTool(new Dictionary<string, object>
                {
                    { GridTool.AlphaAngleKey, 30.0f },
                    { GridTool.StepSizeYKey, 20.0f },
                    { GridTool.IsSnappingEnabledKey, true }
                }, SvgEngine.Resolve<IUndoRedoService>()),

                () => new LineTool(new Dictionary<string, object>
                {
                    { LineTool.MarkerStartIdsKey, new [] { "none", "arrowStart", "circle" } },
                    { LineTool.MarkerStartNamesKey, new [] { "---", "<--", "O--" } },
                    { LineTool.MarkerEndIdsKey, new [] { "none", "arrowEnd", "circle" } },
                    { LineTool.MarkerEndNamesKey, new [] { "---", "-->", "--O" } }
                }, SvgEngine.Resolve<IUndoRedoService>()),
            }));

        }

        [Test]
        public async Task IfUserTapsCanvas_AndDoesNotMove_NoLineIsDrawn()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var lineTool = Canvas.Tools.OfType<LineTool>().Single();
            Canvas.ActiveTool = lineTool;

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
        public async Task IfUserDrawsLine_AndMovesTooLess_NoLineIsCreated()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var lineTool = Canvas.Tools.OfType<LineTool>().Single();
            Canvas.ActiveTool = lineTool;

            // Act
            var start = PointF.Create(10, 10);
            var end = PointF.Create(10, 19);
            await Canvas.OnEvent(new PointerEvent(EventType.PointerDown, start, start, start, 1));
            await Canvas.OnEvent(new MoveEvent(start, start, end, end - start, 1));
            await Canvas.OnEvent(new PointerEvent(EventType.PointerUp, end, end, end, 1));

            // Assert
            Assert.False(Canvas.Document.Descendants().OfType<SvgLine>().Any());
        }

        [Test]
        public async Task IfUserDrawsLine_LineIsCreated()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var lineTool = Canvas.Tools.OfType<LineTool>().Single();
            Canvas.ActiveTool = lineTool;

            // Act
            var start = PointF.Create(10, 10);
            var end = PointF.Create(10, 100);
            await Canvas.OnEvent(new PointerEvent(EventType.PointerDown, start, start, start, 1));
            await Canvas.OnEvent(new MoveEvent(start, start, end, end - start, 1));
            await Canvas.OnEvent(new PointerEvent(EventType.PointerUp, end, end, end, 1));

            // Assert
            Assert.AreEqual(1, Canvas.Document.Descendants().OfType<SvgLine>().Count());
        }

        [Test]
        [TestCase(0.0f, 0.0f, 34.64098f, 20.0f, 150.0f, 150.0f, 160.0f, 160.0f, 0.0f, 0.0f, 34.64098f, 20.0f)]
        [TestCase(0.0f, 0.0f, 34.64098f, 20.0f, 34.64098f, 20.0f, 35.0f, 40.0f, 0.0f, 0.0f, 34.64098f, 40.0f)]
        [TestCase(0.0f, 0.0f, 34.64098f, 20.0f, 34.64098f, 20.0f, 65.0f, 20.0f, 0.0f, 0.0f, 51.96147f, 10.0f)]
        [TestCase(0.0f, 0.0f, 51.96147f, 10.0f, 51.96147f, 10.0f, 70.0f, 20.0f, 0.0f, 0.0f, 69.28196f, 20.0f)]
        [TestCase(0.0f, 0.0f, 34.64098f, 20.0f, 34.64098f, 20.0f, 52.0f, 31.0f, 0.0f, 0.0f, 51.96147f, 30.0f)]
        [TestCase(0.0f, 0.0f, 34.64098f, 20.0f, 34.64098f, 20.0f, 46.0f, 33.0f, 0.0f, 0.0f, 17.32051f, 30.0f)]
        [TestCase(0.0f, 0.0f, 51.96147f, 10.0f, 51.96147f, 10.0f, 70.0f, 21.0f, 0.0f, 0.0f, 69.28196f, 20.0f)]
        [TestCase(0.0f, 0.0f, 86.60254f, 50.0f, 86.60254f, 50.0f, 104.0f, 60.0f, 0.0f, 0.0f, 103.92304f, 60.0f)]
        public async Task IfUserEditLine_LineSnapsToGrid(float lineStartX, float lineStartY, float lineEndX, float lineEndY, float pointerDownX, float pointerDownY, float pointerPositionX, float pointerPositionY, float assertLineStartX, float assertLineStartY, float assertLineEndX, float assertLineEndY)
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var lineTool = Canvas.Tools.OfType<LineTool>().Single();
            var line = new SvgLine { StartX = lineStartX, StartY = lineStartY, EndX = lineEndX, EndY = lineEndY };
            Canvas.Document.Children.Add(line);
            Canvas.SelectedElements.Add(line);
            Canvas.ActiveTool = lineTool;

            // Act
            var start = PointF.Create(pointerDownX, pointerDownY);
            var end = PointF.Create(pointerPositionX, pointerPositionY);
            await Canvas.OnEvent(new PointerEvent(EventType.PointerDown, start, start, start, 1));
            await Canvas.OnEvent(new MoveEvent(start, start, end, end - start, 1));
            await Canvas.OnEvent(new PointerEvent(EventType.PointerUp, end, end, end, 1));

            // Assert
            var points = line.GetTransformedLinePoints();
            Assert.AreEqual(assertLineStartX, points[0].X, 0.001f);
            Assert.AreEqual(assertLineStartY, points[0].Y, 0.001f);
            Assert.AreEqual(assertLineEndX, points[1].X, 0.001f);
            Assert.AreEqual(assertLineEndY, points[1].Y, 0.001f);
        }

        [Test]
        [TestCase(0.0f, 0.0f, 34.64098f, 20.0f, 150.0f, 150.0f, 160.0f, 160.0f, 34.64098f, 0.0f, 0.0f, 20.0f)]
        [TestCase(0.0f, 0.0f, 34.64098f, 20.0f, 0.0f, 20.0f, 35.0f, 40.0f, 34.64098f, 0.0f, 34.64098f, 40.0f)]
        [TestCase(0.0f, 0.0f, 34.64098f, 20.0f, 0.0f, 20.0f, 35.0f, 20.0f, 34.64098f, 0.0f, 34.64098f, 20.0f)]
        [TestCase(0.0f, 0.0f, 34.64098f, 20.0f, 0.0f, 20.0f, 55.0f, 20.0f, 34.64098f, 0.0f, 51.96147f, 10.0f)]
        [TestCase(0.0f, 0.0f, 34.64098f, 20.0f, 0.0f, 20.0f, 70.0f, 20.0f, 34.64098f, 0.0f, 69.28196f, 20.0f)]
        [TestCase(0.0f, 0.0f, 34.64098f, 20.0f, 0.0f, 20.0f, 87.0f, 20.0f, 34.64098f, 0.0f, 86.60254f, 10.0f)]
        [TestCase(0.0f, 0.0f, 34.64098f, 20.0f, 0.0f, 20.0f, 104.0f, 20.0f, 34.64098f, 0.0f, 103.92304f, 20.0f)]
        [TestCase(0.0f, 0.0f, 34.64098f, 20.0f, 0.0f, 20.0f, 122.0f, 20.0f, 34.64098f, 0.0f, 121.24355f, 10.0f)]
        [TestCase(0.0f, 0.0f, 34.64098f, 20.0f, 0.0f, 20.0f, 139.0f, 20.0f, 34.64098f, 0.0f, 138.56402f, 20.0f)]
        [TestCase(0.0f, 0.0f, 34.64098f, 20.0f, 0.0f, 20.0f, 156.0f, 20.0f, 34.64098f, 0.0f, 155.88453f, 10.0f)]
        [TestCase(0.0f, 0.0f, 34.64098f, 20.0f, 0.0f, 20.0f, 35.0f, -20.0f, 34.64098f, 0.0f, 34.64098f, -20.0f)]
        [TestCase(0.0f, 0.0f, 34.64098f, 20.0f, 0.0f, 20.0f, 55.0f, -20.0f, 34.64098f, 0.0f, 51.96147f, -30.0f)]
        [TestCase(0.0f, 0.0f, 34.64098f, 20.0f, 0.0f, 20.0f, 70.0f, -20.0f, 34.64098f, 0.0f, 69.28196f, -20.0f)]
        [TestCase(0.0f, 0.0f, 34.64098f, 20.0f, 0.0f, 20.0f, 87.0f, -20.0f, 34.64098f, 0.0f, 86.60254f, -30.0f)]
        [TestCase(0.0f, 0.0f, 34.64098f, 20.0f, 0.0f, 20.0f, 104.0f, -20.0f, 34.64098f, 0.0f, 103.92304f, -20.0f)]
        [TestCase(0.0f, 0.0f, 34.64098f, 20.0f, 0.0f, 20.0f, 122.0f, -20.0f, 34.64098f, 0.0f, 121.24355f, -30.0f)]
        [TestCase(0.0f, 0.0f, 34.64098f, 20.0f, 0.0f, 20.0f, 139.0f, -20.0f, 34.64098f, 0.0f, 138.56402f, -20.0f)]
        [TestCase(0.0f, 0.0f, 34.64098f, 20.0f, 0.0f, 20.0f, 156.0f, -20.0f, 34.64098f, 0.0f, 155.88453f, -30.0f)]
        [TestCase(0.0f, 0.0f, 34.64098f, 20.0f, 0.0f, 20.0f, -18.0f, 20.0f, 34.64098f, 0.0f, -17.32051f, 10.0f)]
        [TestCase(0.0f, 0.0f, 34.64098f, 20.0f, 0.0f, 20.0f, -35.0f, 20.0f, 34.64098f, 0.0f, -34.64098f, 20.0f)]
        [TestCase(0.0f, 0.0f, 34.64098f, 20.0f, 0.0f, 20.0f, -55.0f, 20.0f, 34.64098f, 0.0f, -51.96147f, 10.0f)]
        [TestCase(0.0f, 0.0f, 34.64098f, 20.0f, 0.0f, 20.0f, -70.0f, 20.0f, 34.64098f, 0.0f, -69.28196f, 20.0f)]
        [TestCase(0.0f, 0.0f, 34.64098f, 20.0f, 0.0f, 20.0f, -18.0f, -20.0f, 34.64098f, 0.0f, -17.32051f, -30.0f)]
        [TestCase(0.0f, 0.0f, 34.64098f, 20.0f, 0.0f, 20.0f, -35.0f, -20.0f, 34.64098f, 0.0f, -34.64098f, -20.0f)]
        [TestCase(0.0f, 0.0f, 34.64098f, 20.0f, 0.0f, 20.0f, -55.0f, -20.0f, 34.64098f, 0.0f, -51.96147f, -30.0f)]
        [TestCase(0.0f, 0.0f, 34.64098f, 20.0f, 0.0f, 20.0f, -70.0f, -20.0f, 34.64098f, 0.0f, -69.28196f, -20.0f)]
        public async Task RotatedLine120Degrees_IfUserEditLine_LineSnapsToGrid(float lineStartX, float lineStartY, float lineEndX, float lineEndY, float pointerDownX, float pointerDownY, float pointerPositionX, float pointerPositionY, float assertLineStartX, float assertLineStartY, float assertLineEndX, float assertLineEndY)
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var lineTool = Canvas.Tools.OfType<LineTool>().Single();
            var line = new SvgLine { StartX = lineStartX, StartY = lineStartY, EndX = lineEndX, EndY = lineEndY };
            line.SetTransformationMatrix(line.CreateOriginRotation(120));
            Canvas.Document.Children.Add(line);
            Canvas.SelectedElements.Add(line);
            Canvas.ActiveTool = lineTool;

            // Act
            var start = PointF.Create(pointerDownX, pointerDownY);
            var end = PointF.Create(pointerPositionX, pointerPositionY);
            await Canvas.OnEvent(new PointerEvent(EventType.PointerDown, start, start, start, 1));
            await Canvas.OnEvent(new MoveEvent(start, start, end, end - start, 1));
            await Canvas.OnEvent(new PointerEvent(EventType.PointerUp, end, end, end, 1));

            // Assert
            var points = line.GetTransformedLinePoints();
            Assert.AreEqual(assertLineStartX, points[0].X, 0.001f);
            Assert.AreEqual(assertLineStartY, points[0].Y, 0.001f);
            Assert.AreEqual(assertLineEndX, points[1].X, 0.001f);
            Assert.AreEqual(assertLineEndY, points[1].Y, 0.001f);
        }

        [Test]
        [TestCase(0.0f, 0.0f, 34.64098f, 20.0f, 150.0f, 150.0f, 160.0f, 160.0f, 34.64098f, 20.0f, 0.0f, 0.0f)]
        [TestCase(0.0f, 0.0f, 34.64098f, 20.0f, 34.64098f, 20.0f, 35.0f, 40.0f, 34.64098f, 40.0f, 0.0f, 0.0f)]
        [TestCase(0.0f, 0.0f, 34.64098f, 20.0f, 34.64098f, 20.0f, 65.0f, 20.0f, 51.96147f, 10.0f, 0.0f, 0.0f)]
        [TestCase(0.0f, 0.0f, 51.96147f, 10.0f, 51.96147f, 10.0f, 70.0f, 20.0f, 69.28196f, 20.0f, 0.0f, 0.0f)]
        [TestCase(0.0f, 0.0f, 34.64098f, 20.0f, 34.64098f, 20.0f, 52.0f, 31.0f, 51.96147f, 30.0f, 0.0f, 0.0f)]
        [TestCase(0.0f, 0.0f, 34.64098f, 20.0f, 34.64098f, 20.0f, 46.0f, 33.0f, 17.32051f, 30.0f, 0.0f, 0.0f)]
        [TestCase(0.0f, 0.0f, 51.96147f, 10.0f, 51.96147f, 10.0f, 70.0f, 21.0f, 69.28196f, 20.0f, 0.0f, 0.0f)]
        [TestCase(0.0f, 0.0f, 86.60254f, 50.0f, 86.60254f, 50.0f, 104.0f, 60.0f, 103.92304f, 60.0f, 0.0f, 0.0f)]
        public async Task RotatedLine180Degrees_IfUserEditLine_LineSnapsToGrid(float lineStartX, float lineStartY, float lineEndX, float lineEndY, float pointerDownX, float pointerDownY, float pointerPositionX, float pointerPositionY, float assertLineStartX, float assertLineStartY, float assertLineEndX, float assertLineEndY)
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var lineTool = Canvas.Tools.OfType<LineTool>().Single();
            var line = new SvgLine { StartX = lineStartX, StartY = lineStartY, EndX = lineEndX, EndY = lineEndY };
            line.SetTransformationMatrix(line.CreateOriginRotation(180));
            Canvas.Document.Children.Add(line);
            Canvas.SelectedElements.Add(line);
            Canvas.ActiveTool = lineTool;

            // Act
            var start = PointF.Create(pointerDownX, pointerDownY);
            var end = PointF.Create(pointerPositionX, pointerPositionY);
            await Canvas.OnEvent(new PointerEvent(EventType.PointerDown, start, start, start, 1));
            await Canvas.OnEvent(new MoveEvent(start, start, end, end - start, 1));
            await Canvas.OnEvent(new PointerEvent(EventType.PointerUp, end, end, end, 1));

            // Assert
            var points = line.GetTransformedLinePoints();
            Assert.AreEqual(assertLineStartX, points[0].X, 0.001f);
            Assert.AreEqual(assertLineStartY, points[0].Y, 0.001f);
            Assert.AreEqual(assertLineEndX, points[1].X, 0.001f);
            Assert.AreEqual(assertLineEndY, points[1].Y, 0.001f);
        }
    }
}
