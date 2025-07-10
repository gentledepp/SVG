using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Svg.Editor.Interfaces;
using Svg.Editor.Tools;
using Svg.Interfaces;

namespace Svg.Editor.Core.Test
{
    [TestFixture]
    public class SvgDrawingCanvasTests : SvgDrawingCanvasTestBase
    {
        private MockTextInputService _textMock;

        [SetUp]
        protected override void SetupOverride()
        {
            // set up tools
            var textToolProperties = new Dictionary<string, object>
            {
                {"fontsizes", new[] {12f, 16f, 20f, 24f, 36f, 48f}},
                {"selectedfontsizeindex", 1},
                {"fontsizenames", new[] {"12px", "16px", "20px", "24px", "36px", "48px"}}
            };

            var undoRedoService = SvgEngine.Resolve<IUndoRedoService>();

            Canvas.LoadTools(
                () => new GridTool(null, undoRedoService),
                () => new MoveTool(undoRedoService),
                () => new PanTool(null),
                () => new RotationTool(null, undoRedoService),
                () => new ZoomTool(null),
                () => new SelectionTool(undoRedoService),
                () => new TextTool(textToolProperties, undoRedoService));

            // register mock text input service for text tool
            _textMock = new MockTextInputService();
            SvgEngine.Register<ITextInputService>(() => _textMock);
            
        }

        [Test]
        public async Task HasOneActiveExplicitTool()
        {
            // Act
            await Canvas.EnsureInitialized();

            // Assert
            var t = Canvas.ActiveTool;

            Assert.IsNotNull(t);
            Assert.AreEqual(ToolUsage.Explicit, t.ToolUsage);

            var activeTools = Canvas.Tools.Where(to => to.IsActive && to.ToolUsage == ToolUsage.Explicit).ToList();
            Assert.AreEqual(1, activeTools.Count);
            Assert.AreSame(t, activeTools.Single());
        }

        [Test]
        public void SvgDocumentIsNeverNull()
        {
            Assert.IsNotNull(Canvas.Document);
        }

        [Test]
        public async Task CanChangeActiveTool()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var txtTool = Canvas.Tools.OfType<TextTool>().Single();

            // Act
            Canvas.ActiveTool = txtTool;

            // Assert
            Assert.IsTrue(txtTool.IsActive);

            var activeTools = Canvas.Tools.Where(to => to.IsActive && to.ToolUsage == ToolUsage.Explicit).ToList();
            Assert.AreEqual(1, activeTools.Count);
            Assert.AreSame(txtTool, activeTools.Single());
        }

        [Test]
        public async Task CanAddElementAtScreenCenter()
        {
            // Arrange
            await Canvas.EnsureInitialized();

            var d = LoadDocument("nested_transformed_text.svg");
            var element = d.Children.OfType<SvgVisualElement>().Single(c => c.Visible && c.Displayable);
            Canvas.ScreenWidth = 800;
            Canvas.ScreenHeight = 500;
            var gt = Canvas.Tools.OfType<GridTool>().Single();
            gt.IsSnappingEnabled = false; // disable snapping in this case

            // Act
            await Canvas.AddItemInScreenCenter(element);

            // Assert 
            var children = Canvas.Document.Children.OfType<SvgVisualElement>().ToList();
            Assert.AreEqual(1, children.Count);
            var child = children.Single();
            var b = child.GetBoundingBox(Canvas.GetCanvasTransformationMatrix());

            var e = RectangleF.Create(192.1539f, 224.3942f, 415.6923f, 51.21167f);
            EnsureRectanglesEqual(e, b);
        }

        private void EnsureRectanglesEqual(RectangleF expected, RectangleF actual)
        {
            Assert.AreEqual(Math.Round(expected.X, 2), Math.Round(actual.X, 2), $"{expected} \nvs {actual}");
            Assert.AreEqual(Math.Round(expected.Y, 2), Math.Round(actual.Y, 2), $"{expected} \nvs {actual}");
            Assert.AreEqual(Math.Round(expected.Width, 2), Math.Round(actual.Width, 2), $"{expected} \nvs {actual}");
            Assert.AreEqual(Math.Round(expected.Height, 2), Math.Round(actual.Height, 2), $"{expected} \nvs {actual}");
        }

        [Test]
        [TestCase(0, 0, 0, 0, 1, 0, 0, 0, 0)]
        [TestCase(0, 0, 100, 100, 1, 0, 0, -100, -100)]
        [TestCase(0, 0, -100, 100, 1, 0, 0, 100, -100)]
        [TestCase(0, 0, -100, -100, 1, 0, 0, 100, 100)]
        [TestCase(0, 0, 100, -100, 1, 0, 0, -100, 100)]
        [TestCase(0, 0, 100, 100, 2, 0, 0, -50, -50)]
        [TestCase(0, 0, 100, 100, 0.5f, 0, 0, -200, -200)]
        [TestCase(0, 0, 0, 0, 0.5f, 200, 200, -200, -200)]
        [TestCase(0, 0, 0, 0, 0.5f, -200, 200, 200, -200)]
        [TestCase(0, 0, 0, 0, 0.5f, -200, -200, 200, 200)]
        [TestCase(0, 0, 0, 0, 0.5f, 200, -200, -200, 200)]
        [TestCase(0, 0, 0, 0, 2, 200, 200, 100, 100)]
        [TestCase(0, 0, 0, 0, 2, -200, 200, -100, 100)]
        [TestCase(0, 0, 0, 0, 2, -200, -200, -100, -100)]
        [TestCase(0, 0, 0, 0, 2, 200, -200, 100, -100)]
        public async Task ScreenToCanvasReturnsTransformedPoint(float screenX, float screenY, float translateX, float translateY, float zoomFactor,
            float zoomFocusX, float zoomFocusY, float expectedCanvasX, float expectedCanvasY)
        {
            // Arrange
            await Canvas.EnsureInitialized();
            Canvas.Translate = PointF.Create(translateX, translateY);
            Canvas.ZoomFactor = zoomFactor;
            Canvas.ZoomFocus = PointF.Create(zoomFocusX, zoomFocusY);

            // Act
            var canvasPoint = Canvas.ScreenToCanvas(PointF.Create(screenX, screenY));

            // Assert
            Assert.AreEqual(expectedCanvasX, canvasPoint.X);
            Assert.AreEqual(expectedCanvasY, canvasPoint.Y);
        }

        [Test]
        [TestCase(0, 0, 0, 0, 1, 0, 0, 0, 0)]
        [TestCase(0, 0, 100, 100, 1, 0, 0, 100, 100)]
        [TestCase(0, 0, -100, 100, 1, 0, 0, -100, 100)]
        [TestCase(0, 0, -100, -100, 1, 0, 0, -100, -100)]
        [TestCase(0, 0, 100, -100, 1, 0, 0, 100, -100)]
        [TestCase(0, 0, 100, 100, 2, 0, 0, 100, 100)]
        [TestCase(0, 0, 100, 100, 0.5f, 0, 0, 100, 100)]
        [TestCase(0, 0, 0, 0, 0.5f, 200, 200, 100, 100)]
        [TestCase(0, 0, 0, 0, 0.5f, -200, 200, -100, 100)]
        [TestCase(0, 0, 0, 0, 0.5f, -200, -200, -100, -100)]
        [TestCase(0, 0, 0, 0, 0.5f, 200, -200, 100, -100)]
        [TestCase(0, 0, 0, 0, 2, 200, 200, -200, -200)]
        [TestCase(0, 0, 0, 0, 2, -200, 200, 200, -200)]
        [TestCase(0, 0, 0, 0, 2, -200, -200, 200, 200)]
        [TestCase(0, 0, 0, 0, 2, 200, -200, -200, 200)]
        public async Task CanvasToScreenReturnsRetransformedPoint(float canvasX, float canvasY, float translateX, float translateY, float zoomFactor,
            float zoomFocusX, float zoomFocusY, float expectedScreenX, float expectedScreenY)
        {
            // Arrange
            await Canvas.EnsureInitialized();
            Canvas.Translate = PointF.Create(translateX, translateY);
            Canvas.ZoomFactor = zoomFactor;
            Canvas.ZoomFocus = PointF.Create(zoomFocusX, zoomFocusY);

            // Act
            var screenPoint = Canvas.CanvasToScreen(PointF.Create(canvasX, canvasY));

            // Assert
            Assert.AreEqual(expectedScreenX, screenPoint.X);
            Assert.AreEqual(expectedScreenY, screenPoint.Y);
        }

        [Test]
        public async Task GetToolPropertiesJson_ReturnsJson()
        {
            await Canvas.EnsureInitialized();

            var json = Canvas.GetToolPropertiesJson();

            Assert.False(string.IsNullOrWhiteSpace(json));
        }

        private class MockTextInputService : ITextInputService
        {
            public Func<string, string, TextTool.TextProperties> F { get; set; } = (x, y) => null;

            public Task<TextTool.TextProperties> GetUserInput(string title, string textValue, IEnumerable<string> textSizeOptions, int textSizeSelected, int maxTextLength)
            {
                return Task.FromResult(F(title, textValue));
            }
        }
    }
}
