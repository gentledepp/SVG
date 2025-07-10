using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Reactive.Testing;
using NUnit.Framework;
using Svg.Editor.Core.Test.Mocks;
using Svg.Editor.Events;
using Svg.Editor.Interfaces;
using Svg.Editor.Tools;
using Svg.Interfaces;

namespace Svg.Editor.Core.Test
{
    [TestFixture]
    public class UndoRedoToolTests : SvgDrawingCanvasTestBase
    {
        private MockColorInputService _colorMock;
        private MockTextInputService _textMock;
        private MockStrokeStyleOptionsInputService _mockStrokeStyle;

        [SetUp]
        protected override void SetupOverride()
        {
            var textToolProperties = new Dictionary<string, object>
            {
                { "fontsizes", new [] { 12f, 16f, 20f, 24f, 36f, 48f } },
                { "selectedfontsizeindex", 1 },
                { "fontsizenames", new [] { "12px", "16px", "20px", "24px", "36px", "48px" } }
            };

            var undoRedoService = SvgEngine.Resolve<IUndoRedoService>();

            Canvas.LoadTools(
                () => new GridTool(null, undoRedoService),
                () => new MoveTool(undoRedoService),
                () => new PanTool(null),
                () => new RotationTool(null, undoRedoService),
                () => new ZoomTool(null),
                () => new SelectionTool(undoRedoService),
                () => new TextTool(textToolProperties, undoRedoService),
                () => new UndoRedoTool(undoRedoService),
                () => new ArrangeTool(undoRedoService),
                () => new LineTool(null, undoRedoService),
                () => new FreeDrawingTool(null, undoRedoService),
                () => new StrokeStyleTool(new Dictionary<string, object>
                {
                    {StrokeStyleTool.StrokeDashesKey, new[] {"1", "3"}}
                }, undoRedoService),
                () => new ColorTool(new Dictionary<string, object>
                {
                    {ColorTool.SelectableColorsKey, new[] {"#000000", "#FF0000"}}
                }, undoRedoService));

            _colorMock = new MockColorInputService();
            SvgEngine.Register<IColorInputService>(() => _colorMock);

            _textMock = new MockTextInputService();
            SvgEngine.Register<ITextInputService>(() => _textMock);

            _mockStrokeStyle = new MockStrokeStyleOptionsInputService();
            SvgEngine.Register<IStrokeStyleOptionsInputService>(() => _mockStrokeStyle);

        }

        [Test]
        public async Task WhenUserCreatesText_FillAndStrokeHaveSelectedColor_ThenUndo_FillAndStrokeHaveOldColors()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var colorTool = Canvas.Tools.OfType<ColorTool>().Single();
            var text = new SvgText("hello");
            text.Stroke = new SvgColourServer(Color.Create("#8e8e8e"));
            colorTool.SelectedColorIndex = 1;
            var color = Color.Create(colorTool.SelectableColors[1]);
            var oldStroke = text.Stroke?.ToString();
            var oldFill = text.Fill?.ToString();
            _textMock.F = (x, y) => null;

            await Canvas.AddItemInScreenCenter(text);

            // Preassert
            Assert.AreEqual(color, ((SvgColourServer) text.Fill)?.Colour);
            Assert.AreEqual(color, ((SvgColourServer) text.Stroke)?.Colour);

            // Act
            var undoredoTool = Canvas.Tools.OfType<UndoRedoTool>().Single();
            undoredoTool.Commands.First(x => x.Name == "Undo").Execute(null);

            // Assert
            Assert.AreEqual(oldStroke, text.Stroke?.ToString());
            Assert.AreEqual(oldFill, text.Fill?.ToString());
        }

        [Test]
        public async Task WhenUserCreatesRectangle_StrokeHasSelectedColor_ThenUndo_StrokeHasOldColor()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var colorTool = Canvas.Tools.OfType<ColorTool>().Single();
            var rectangle = new SvgRectangle();
            rectangle.Stroke = new SvgColourServer(Color.Create("#8e8e8e"));
            var oldStroke = rectangle.Stroke?.ToString();
            await Canvas.AddItemInScreenCenter(rectangle);

            // Preassert
            var color = colorTool.SelectableColors[colorTool.SelectedColorIndex];
            Assert.AreEqual(color, ((SvgColourServer) rectangle.Stroke)?.Colour.ToString());

            // Act
            var undoredoTool = Canvas.Tools.OfType<UndoRedoTool>().Single();
            undoredoTool.Commands.First(x => x.Name == "Undo").Execute(null);

            // Assert
            Assert.AreEqual(oldStroke, rectangle.Stroke?.ToString());
        }

        [Test]
        public async Task WhenUserSelectsTextAndSelectsColor_StrokeAndFillHaveSelectedColor_ThenUndo_StrokeAndFillHaveOldColors()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var color = Color.Create(Canvas.Tools.OfType<ColorTool>().Single().SelectableColors[1]);
            _colorMock.F = () => 1;
            var text = new SvgText("hello");
            text.Stroke = new SvgColourServer(Color.Create("#8e8e8e"));
            await Canvas.AddItemInScreenCenter(text);
            var oldStroke = text.Stroke?.ToString();
            var oldFill = text.Fill?.ToString();
            var changeColorCommand = Canvas.ToolCommands.Single(x => x.FirstOrDefault()?.Name == "Change color").First();
            Canvas.SelectedElements.Add(text);
            changeColorCommand.Execute(null);

            // Pressert
            Assert.AreEqual(color, ((SvgColourServer) text.Stroke)?.Colour);
            Assert.AreEqual(color, ((SvgColourServer) text.Fill)?.Colour);

            // Act
            var undoredoTool = Canvas.Tools.OfType<UndoRedoTool>().Single();
            undoredoTool.Commands.First(x => x.Name == "Undo").Execute(null);

            // Assert
            Assert.AreEqual(oldStroke, text.Stroke?.ToString());
            Assert.AreEqual(oldFill, text.Fill?.ToString());
        }

        [Test]
        public async Task WhenUserSelectsRectangleAndSelectsColor_StrokeHasSelectedColor_ThenUndo_StrokeHasOldColor()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var color = Color.Create(Canvas.Tools.OfType<ColorTool>().Single().SelectableColors[1]);
            _colorMock.F = () => 1;
            var rectangle = new SvgRectangle();
            rectangle.Stroke = new SvgColourServer(Color.Create("#8e8e8e"));
            await Canvas.AddItemInScreenCenter(rectangle);
            var oldStroke = rectangle.Stroke?.ToString();
            var changeColorCommand = Canvas.ToolCommands.Single(x => x.FirstOrDefault()?.Name == "Change color").First();
            Canvas.SelectedElements.Add(rectangle);
            changeColorCommand.Execute(null);

            // Preassert
            Assert.True(color.Equals(((SvgColourServer) rectangle.Stroke)?.Colour));

            // Act
            var undoredoTool = Canvas.Tools.OfType<UndoRedoTool>().Single();
            undoredoTool.Commands.First(x => x.Name == "Undo").Execute(null);

            // Assert
            Assert.AreEqual(oldStroke, rectangle.Stroke?.ToString());
        }

        private class MockColorInputService : IColorInputService
        {
            public Func<int> F { get; set; } = () => 0;

            public Task<int> GetIndexFromUserInput(string title, string[] items, string[] colors, int defaultIndex = 0)
            {
                return Task.FromResult(F());
            }
        }

        [Test]
        public async Task IfUserDrawsPath_PathIsCreated_ThenUndo_PathIsRemoved()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var tool = Canvas.Tools.OfType<FreeDrawingTool>().Single();
            Canvas.ActiveTool = tool;
            await Move(PointF.Create(10, 10), PointF.Create(10, 100));

            // Preassert
            Assert.AreEqual(1, Canvas.Document.Children.OfType<SvgPath>().Count());

            // Act
            var undoredoTool = Canvas.Tools.OfType<UndoRedoTool>().Single();
            undoredoTool.Commands.First(x => x.Name == "Undo").Execute(null);

            // Assert
            Assert.IsEmpty(Canvas.Document.Children.OfType<SvgPath>());
        }

        [Test]
        public async Task IfUserDrawsLine_LineIsCreated_ThenUndo_LineIsRemoved()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var lineTool = Canvas.Tools.OfType<LineTool>().Single();
            Canvas.ActiveTool = lineTool;
            await Move(PointF.Create(10, 10), PointF.Create(10, 100));

            // Preassert
            Assert.AreEqual(1, Canvas.Document.Descendants().OfType<SvgLine>().Count());

            // Act
            var undoredoTool = Canvas.Tools.OfType<UndoRedoTool>().Single();
            undoredoTool.Commands.First(x => x.Name == "Undo").Execute(null);

            // Assert
            Assert.IsEmpty(Canvas.Document.Children.OfType<SvgLine>());
        }

        [Test]
        public async Task IfUserEditLine_LineSnapsToGrid_ThenUndo_LinePointsAreOnOldPositions()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var lineTool = Canvas.Tools.OfType<LineTool>().Single();
            const float lineStartX = 0.0f;
            const float lineStartY = 0.0f;
            const float lineEndX = 34.64098f;
            const float lineEndY = 20.0f;
            var line = new SvgLine { StartX = lineStartX, StartY = lineStartY, EndX = lineEndX, EndY = lineEndY };
            Canvas.Document.Children.Add(line);
            Canvas.SelectedElements.Add(line);
            Canvas.ActiveTool = lineTool;
            await Move(PointF.Create(34.64098f, 20.0f), PointF.Create(70.0f, 40.0f));

            // Preassert
            var points = line.GetTransformedLinePoints();
            Assert.AreEqual(0.0f, points[0].X, 0.001f);
            Assert.AreEqual(0.0f, points[0].Y, 0.001f);
            Assert.AreEqual(69.28196f, points[1].X, 0.001f);
            Assert.AreEqual(40.0f, points[1].Y, 0.001f);

            // Act
            var undoredoTool = Canvas.Tools.OfType<UndoRedoTool>().Single();
            undoredoTool.Commands.First(x => x.Name == "Undo").Execute(null);

            // Assert
            points = line.GetTransformedLinePoints();
            Assert.AreEqual(lineStartX, points[0].X, 0.01f);
            Assert.AreEqual(lineStartY, points[0].Y, 0.01f);
            Assert.AreEqual(lineEndX, points[1].X, 0.01f);
            Assert.AreEqual(lineEndY, points[1].Y, 0.01f);
        }

        [Test]
        public async Task IfPointerIsMoved_And1ElementIsSelected_ElementIsMoved_ThenUndo_ElementIsMovedBack()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var tool = Canvas.Tools.OfType<SelectionTool>().Single();
            Canvas.ActiveTool = tool;

            var child = new SvgRectangle { X = 50, Y = 50, Width = 50, Height = 50 };
            var child1 = new SvgRectangle { X = 250, Y = 150, Width = 100, Height = 25 };
            var child2 = new SvgRectangle { X = 150, Y = 250, Width = 150, Height = 150 };
            Canvas.ScreenWidth = 800;
            Canvas.ScreenHeight = 500;
            Canvas.Document.Children.Add(child);
            Canvas.Document.Children.Add(child1);
            Canvas.Document.Children.Add(child2);
            Canvas.SelectedElements.Add(child);
            var transforms = child.Transforms.Clone();
            var transforms1 = child1.Transforms.Clone();
            var transforms2 = child2.Transforms.Clone();
            await Move(PointF.Create(75, 75), PointF.Create(200, 100));

            // Preassert
            Assert.AreNotEqual(transforms, child.Transforms);
            Assert.AreEqual(transforms1, child1.Transforms);
            Assert.AreEqual(transforms2, child2.Transforms);

            // Act
            var undoredoTool = Canvas.Tools.OfType<UndoRedoTool>().Single();
            undoredoTool.Commands.First(x => x.Name == "Undo").Execute(null);

            // Assert
            Assert.AreEqual(transforms, child.Transforms);
        }

        [Test]
        public async Task IfPointerIsMoved_AndElementsAreSelected_ElementsAreMoved()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var tool = Canvas.Tools.OfType<SelectionTool>().Single();
            Canvas.ActiveTool = tool;

            var child = new SvgRectangle { X = 50, Y = 50, Width = 50, Height = 50 };
            var child1 = new SvgRectangle { X = 250, Y = 150, Width = 100, Height = 25 };
            var child2 = new SvgRectangle { X = 150, Y = 250, Width = 150, Height = 150 };
            Canvas.Document.Children.Add(child);
            Canvas.Document.Children.Add(child1);
            Canvas.Document.Children.Add(child2);
            Canvas.SelectedElements.Add(child);
            Canvas.SelectedElements.Add(child1);
            Canvas.SelectedElements.Add(child2);
            var transforms = child.Transforms.Clone();
            var transforms1 = child1.Transforms.Clone();
            var transforms2 = child2.Transforms.Clone();
            await Move(PointF.Create(75, 75), PointF.Create(200, 100));

            // Preassert
            Assert.AreNotEqual(transforms, child.Transforms);
            Assert.AreNotEqual(transforms1, child1.Transforms);
            Assert.AreNotEqual(transforms2, child2.Transforms);

            // Act
            var undoredoTool = Canvas.Tools.OfType<UndoRedoTool>().Single();
            undoredoTool.Commands.First(x => x.Name == "Undo").Execute(null);

            // Assert
            Assert.AreEqual(transforms, child.Transforms);
            Assert.AreEqual(transforms1, child1.Transforms);
            Assert.AreEqual(transforms2, child2.Transforms);
        }

        [Test]
        public async Task SingleElementSelected_Rotate_RotatesElementUsingSvgMatrix_ThenUndo_RotatesElementBack()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var element1 = new SvgRectangle()
            {
                X = new SvgUnit(SvgUnitType.Pixel, 0),
                Y = new SvgUnit(SvgUnitType.Pixel, 0),
                Width = new SvgUnit(SvgUnitType.Pixel, 30),
                Height = new SvgUnit(SvgUnitType.Pixel, 20),
            };
            Canvas.Document.Children.Add(element1);
            Canvas.SelectedElements.Add(element1);
            var matrix = element1.Transforms.GetMatrix();
            var formerMatrix = matrix.Clone();
            matrix.RotateAt(55f, PointF.Create(15, 10), MatrixOrder.Prepend);
            await Rotate(45, 10);

            // Preassert
            var actual = element1.Transforms.GetMatrix();
            AssertAreEqual(matrix, actual);
            Assert.AreEqual(1, Canvas.SelectedElements.Count);
            Assert.IsTrue(Canvas.SelectedElements.Single() == element1, "must still be selected");

            // Act
            var undoredoTool = Canvas.Tools.OfType<UndoRedoTool>().Single();
            undoredoTool.Commands.First(x => x.Name == "Undo").Execute(null);

            // Assert
            actual = element1.Transforms.GetMatrix();
            AssertAreEqual(formerMatrix, actual);
            Assert.AreEqual(0, Canvas.SelectedElements.Count, "must be deselected");
        }

        private void AssertAreEqual(Matrix expected, Matrix actual)
        {
            Assert.AreEqual(expected.ScaleX, actual.ScaleX, 0.05, $"{expected} but was \n{actual}");
            Assert.AreEqual(expected.ScaleY, actual.ScaleY, 0.05, $"{expected} but was \n{actual}");
            Assert.AreEqual(expected.OffsetX, actual.OffsetX, 0.05, $"{expected} but was \n{actual}");
            Assert.AreEqual(expected.OffsetY, actual.OffsetY, 0.05, $"{expected} but was \n{actual}");
            Assert.AreEqual(expected.SkewX, actual.SkewX, 0.05, $"{expected} but was \n{actual}");
            Assert.AreEqual(expected.SkewY, actual.SkewY, 0.05, $"{expected} but was \n{actual}");
        }

        [Test]
        public async Task ElementsAreSelected_AndDeleteCommandExecuted_ElementsAreRemoved_ThenUndo_ElementsAreAddedAgain()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var selectionTool = Canvas.Tools.OfType<SelectionTool>().Single();
            Canvas.ActiveTool = selectionTool;
            Canvas.ScreenWidth = 800;
            Canvas.ScreenHeight = 500;

            var element1 = new SvgRectangle()
            {
                X = new SvgUnit(SvgUnitType.Pixel, 200),
                Y = new SvgUnit(SvgUnitType.Pixel, 200),
                Width = new SvgUnit(SvgUnitType.Pixel, 30),
                Height = new SvgUnit(SvgUnitType.Pixel, 20),
            };
            Canvas.Document.Children.Add(element1);
            var d = LoadDocument("nested_transformed_text.svg");
            var element2 = d.Children.OfType<SvgVisualElement>().Single(c => c.Visible && c.Displayable);
            await Canvas.AddItemInScreenCenter(element2);
            Canvas.SelectedElements.Add(element1);
            Canvas.SelectedElements.Add(element2);
            selectionTool.Commands.First().Execute(null);

            // Preassert
            Assert.False(Canvas.Document.Children.Any(x => x == element1));
            Assert.False(Canvas.Document.Children.Any(x => x == element2));
            Assert.False(Canvas.SelectedElements.Any());

            // Act
            var undoredoTool = Canvas.Tools.OfType<UndoRedoTool>().Single();
            undoredoTool.Commands.First(x => x.Name == "Undo").Execute(null);

            // Assert
            Assert.True(Canvas.Document.Children.Any(x => x == element1));
            Assert.True(Canvas.Document.Children.Any(x => x == element2));
            Assert.False(Canvas.SelectedElements.Any());
        }

        [Test]
        public async Task OneElementSelected_StrokeStyleCommandExecuted_ChildStrokeIsChanged_ThenUndo_ChildStrokeIsRestored()
        {
            // Arrange
            var tool = Canvas.Tools.OfType<StrokeStyleTool>().Single();
            var ellipse = new SvgEllipse
            {
                CenterX = 10,
                CenterY = 10,
                RadiusX = 50,
                RadiusY = 50,
                Stroke = new SvgColourServer(Color.Create(0, 0, 0))
            };
            await Canvas.EnsureInitialized();
            _mockStrokeStyle.F = (arg1, arg2, arg3, arg4, arg5) => new StrokeStyleTool.StrokeStyleOptions { StrokeDashIndex = 1, StrokeWidthIndex = 1 };

            Canvas.Document.Children.Add(ellipse);
            Canvas.SelectedElements.Add(ellipse);
            var stroke = ellipse.StrokeDashArray?.ToString();

            tool.Commands.First().Execute(null);

            // Preassert
            Assert.AreEqual("3", ellipse.StrokeDashArray?.ToString());

            // Act
            var undoredoTool = Canvas.Tools.OfType<UndoRedoTool>().Single();
            undoredoTool.Commands.First(x => x.Name == "Undo").Execute(null);

            // Assert
            Assert.AreEqual(stroke, ellipse.StrokeDashArray?.ToString());
        }

        [Test]
        public async Task WhenUserTapsToBlankArea_CreatesText_ThenUndo_TextIsRemoved()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var txtTool = Canvas.Tools.OfType<TextTool>().Single();
            Canvas.ActiveTool = txtTool;
            _textMock.F = (x, y) => new TextTool.TextProperties { Text = "hello", FontSizeIndex = 0 };
            await Canvas.OnEvent(new PointerEvent(EventType.PointerDown, PointF.Create(10, 10), PointF.Create(10, 10), PointF.Create(10, 10), 1));
            await Canvas.OnEvent(new PointerEvent(EventType.PointerUp, PointF.Create(10, 10), PointF.Create(10, 10), PointF.Create(10, 10), 1));
            ((TestScheduler) SchedulerProvider.BackgroundScheduler).AdvanceBy(TimeSpan.FromSeconds(1).Ticks);

            // Preassert
            var texts = Canvas.Document.Children.OfType<SvgTextBase>().ToList();
            Assert.AreEqual(1, texts.Count);
            var txt = texts.First();
            Assert.AreEqual("hello", txt.Text);
            Assert.AreEqual(txtTool.FontSizes.First(), txt.FontSize.Value);
            Assert.AreEqual(SvgUnitType.Pixel, txt.FontSize.Type);

            // Act
            var undoredoTool = Canvas.Tools.OfType<UndoRedoTool>().Single();
            undoredoTool.Commands.First(x => x.Name == "Undo").Execute(null);

            // Assert
            texts = Canvas.Document.Children.OfType<SvgTextBase>().ToList();
            Assert.IsEmpty(texts);
        }

        [Test]
        public async Task WhenUserTapsOnExistingText_EditsText()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var txtTool = Canvas.Tools.OfType<TextTool>().Single();
            Canvas.ActiveTool = txtTool;
            const string formerText = "this is a test";
            var formerFontSize = new SvgUnit(SvgUnitType.Pixel, 20);
            Canvas.Document.Children.Add(new SvgText()
            {
                Text = formerText,
                X = new SvgUnitCollection() { new SvgUnit(SvgUnitType.Pixel, 5) },
                Y = new SvgUnitCollection() { new SvgUnit(SvgUnitType.Pixel, 5) },
                FontSize = formerFontSize
            });

            const string newText = "hello";
            _textMock.F = (x, y) => new TextTool.TextProperties { Text = newText, FontSizeIndex = 0 };
            await Canvas.OnEvent(new PointerEvent(EventType.PointerDown, PointF.Create(10, 10), PointF.Create(10, 10), PointF.Create(10, 10), 1));
            await Canvas.OnEvent(new PointerEvent(EventType.PointerUp, PointF.Create(10, 10), PointF.Create(10, 10), PointF.Create(10, 10), 1));
            ((TestScheduler) SchedulerProvider.BackgroundScheduler).AdvanceBy(TimeSpan.FromSeconds(1).Ticks);

            // Preassert
            var texts = Canvas.Document.Children.OfType<SvgTextBase>().ToList();
            Assert.AreEqual(1, texts.Count);
            var txt = texts.First();
            Assert.AreEqual(newText, txt.Text);
            Assert.AreEqual(txtTool.FontSizes.First(), txt.FontSize.Value);
            Assert.AreEqual(SvgUnitType.Pixel, txt.FontSize.Type);

            // Act
            var undoredoTool = Canvas.Tools.OfType<UndoRedoTool>().Single();
            undoredoTool.Commands.First(x => x.Name == "Undo").Execute(null);

            // Assert
            texts = Canvas.Document.Children.OfType<SvgTextBase>().ToList();
            Assert.AreEqual(1, texts.Count);
            txt = texts.First();
            Assert.AreEqual(formerText, txt.Text);
            Assert.AreEqual(formerFontSize.Value, txt.FontSize.Value);
            Assert.AreEqual(SvgUnitType.Pixel, txt.FontSize.Type);
        }

        [Test]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase("\t")]
        [TestCase((string) null)]
        public async Task WhenUserTapsOnExistingText_AndEntersEmpty_RemovesText(string theText)
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var txtTool = Canvas.Tools.OfType<TextTool>().Single();
            Canvas.ActiveTool = txtTool;
            Canvas.Document.Children.Add(new SvgText()
            {
                Text = "this is a test",
                X = new SvgUnitCollection() { new SvgUnit(SvgUnitType.Pixel, 5) },
                Y = new SvgUnitCollection() { new SvgUnit(SvgUnitType.Pixel, 5) },
                FontSize = new SvgUnit(SvgUnitType.Pixel, 20),
            });

            _textMock.F = (x, y) => new TextTool.TextProperties { Text = theText, FontSizeIndex = 0 };
            await Canvas.OnEvent(new PointerEvent(EventType.PointerDown, PointF.Create(10, 10), PointF.Create(10, 10), PointF.Create(10, 10), 1));
            await Canvas.OnEvent(new PointerEvent(EventType.PointerUp, PointF.Create(10, 10), PointF.Create(10, 10), PointF.Create(10, 10), 1));
            ((TestScheduler) SchedulerProvider.BackgroundScheduler).AdvanceBy(TimeSpan.FromSeconds(1).Ticks);

            // Preassert
            var texts = Canvas.Document.Children.OfType<SvgTextBase>().ToList();
            Assert.AreEqual(0, texts.Count);

            // Act
            var undoredoTool = Canvas.Tools.OfType<UndoRedoTool>().Single();
            undoredoTool.Commands.First(x => x.Name == "Undo").Execute(null);

            // Assert
            texts = Canvas.Document.Children.OfType<SvgTextBase>().ToList();
            Assert.AreEqual(1, texts.Count);
        }

        [Test]
        [TestCase("", "  ")]
        [TestCase(" ", "  ")]
        [TestCase("\t", "  ")]
        [TestCase(null, "  ")]
        [TestCase("hello from svg", "hello from svg")]
        public async Task WhenUserTapsNestedTextSpan_EditsTextSpan(string theText, string expectedText)
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var txtTool = Canvas.Tools.OfType<TextTool>().Single();
            Canvas.ActiveTool = txtTool;

            _textMock.F = (x, y) => new TextTool.TextProperties { Text = theText, FontSizeIndex = 0 };

            var d = LoadDocument("nested_transformed_text.svg");
            var child = d.Children.OfType<SvgVisualElement>().Single(c => c.Visible && c.Displayable);
            var formerText = child.Descendants().OfType<SvgTextSpan>().Single().Text;
            Canvas.ScreenWidth = 800;
            Canvas.ScreenHeight = 500;
            await Canvas.AddItemInScreenCenter(child);

            var childBoundingBox = child.GetBoundingBox();
            var childScreenLocation = Canvas.CanvasToScreen(childBoundingBox.Location);

            var pt1 = PointF.Create(childScreenLocation.X + childBoundingBox.Width / 2,
                childScreenLocation.Y + childBoundingBox.Height / 2);

            await Canvas.OnEvent(new PointerEvent(EventType.PointerDown, pt1, pt1, pt1, 1));
            await Canvas.OnEvent(new PointerEvent(EventType.PointerUp, pt1, pt1, pt1, 1));
            ((TestScheduler)SchedulerProvider.BackgroundScheduler).AdvanceBy(TimeSpan.FromSeconds(1).Ticks);

            // Preassert
            var texts = Canvas.Document.Children.OfType<SvgTextBase>().ToList();
            Assert.AreEqual(0, texts.Count);
            var nestedText = Canvas.Document.Descendants().OfType<SvgTextSpan>().Single();
            Assert.AreEqual(expectedText, nestedText.Text);

            // Act
            var undoredoTool = Canvas.Tools.OfType<UndoRedoTool>().Single();
            undoredoTool.Commands.First(x => x.Name == "Undo").Execute(null);

            // Assert
            nestedText = Canvas.Document.Descendants().OfType<SvgTextSpan>().Single();
            Assert.AreEqual(formerText, nestedText.Text);
        }

        [Test]
        public async Task If1ElementIsSelected_AndSendBackwardsIsSelected_ElementIsSwappedWithPrecursor()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var tool = Canvas.Tools.OfType<ArrangeTool>().Single();
            Canvas.ActiveTool = tool;

            var child = new SvgRectangle { X = 50, Y = 50, Width = 50, Height = 50 };
            var child1 = new SvgRectangle { X = 250, Y = 150, Width = 100, Height = 25 };
            var child2 = new SvgRectangle { X = 150, Y = 250, Width = 150, Height = 150 };
            Canvas.ScreenWidth = 800;
            Canvas.ScreenHeight = 500;
            Canvas.Document.Children.Add(child1);
            Canvas.Document.Children.Add(child);
            Canvas.Document.Children.Add(child2);
            Canvas.SelectedElements.Add(child);
            tool.Commands.First(x => x.Name == "Send backward").Execute(null);

            // Preassert
            Assert.True(Canvas.Document.Children.IndexOf(child) < Canvas.Document.Children.IndexOf(child1));
            Assert.True(Canvas.Document.Children.IndexOf(child) < Canvas.Document.Children.IndexOf(child2));

            // Act
            Canvas.Tools.OfType<UndoRedoTool>().Single().Commands.First(x => x.Name == "Undo").Execute(null);

            // Assert
            Assert.True(Canvas.Document.Children.IndexOf(child) > Canvas.Document.Children.IndexOf(child1));
            Assert.True(Canvas.Document.Children.IndexOf(child) < Canvas.Document.Children.IndexOf(child2));
        }

        [Test]
        public async Task If1ElementIsSelected_AndSendToBackIsSelected_ElementIsSentToBack()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var tool = Canvas.Tools.OfType<ArrangeTool>().Single();
            Canvas.ActiveTool = tool;

            var child = new SvgRectangle { X = 50, Y = 50, Width = 50, Height = 50 };
            var child1 = new SvgRectangle { X = 250, Y = 150, Width = 100, Height = 25 };
            var child2 = new SvgRectangle { X = 150, Y = 250, Width = 150, Height = 150 };
            Canvas.ScreenWidth = 800;
            Canvas.ScreenHeight = 500;
            Canvas.Document.Children.Add(child1);
            Canvas.Document.Children.Add(child2);
            Canvas.Document.Children.Add(child);
            Canvas.SelectedElements.Add(child);
            tool.Commands.First(x => x.Name == "Send to back").Execute(null);

            // Preassert
            Assert.True(Canvas.Document.Children.IndexOf(child) < Canvas.Document.Children.IndexOf(child1));
            Assert.True(Canvas.Document.Children.IndexOf(child) < Canvas.Document.Children.IndexOf(child2));

            // Act
            Canvas.Tools.OfType<UndoRedoTool>().Single().Commands.First(x => x.Name == "Undo").Execute(null);

            // Assert
            Assert.True(Canvas.Document.Children.IndexOf(child) > Canvas.Document.Children.IndexOf(child1));
            Assert.True(Canvas.Document.Children.IndexOf(child) > Canvas.Document.Children.IndexOf(child2));
        }

        [Test]
        public async Task If1ElementIsSelected_AndBringForwardIsSelected_ElementIsSwappedWithSuccessor()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var tool = Canvas.Tools.OfType<ArrangeTool>().Single();
            Canvas.ActiveTool = tool;

            var child = new SvgRectangle { X = 50, Y = 50, Width = 50, Height = 50 };
            var child1 = new SvgRectangle { X = 250, Y = 150, Width = 100, Height = 25 };
            var child2 = new SvgRectangle { X = 150, Y = 250, Width = 150, Height = 150 };
            Canvas.ScreenWidth = 800;
            Canvas.ScreenHeight = 500;
            Canvas.Document.Children.Add(child1);
            Canvas.Document.Children.Add(child);
            Canvas.Document.Children.Add(child2);
            Canvas.SelectedElements.Add(child);
            tool.Commands.First(x => x.Name == "Bring forward").Execute(null);

            // Preassert
            Assert.True(Canvas.Document.Children.IndexOf(child) > Canvas.Document.Children.IndexOf(child1));
            Assert.True(Canvas.Document.Children.IndexOf(child) > Canvas.Document.Children.IndexOf(child2));

            // Act
            Canvas.Tools.OfType<UndoRedoTool>().Single().Commands.First(x => x.Name == "Undo").Execute(null);

            // Assert
            Assert.True(Canvas.Document.Children.IndexOf(child) > Canvas.Document.Children.IndexOf(child1));
            Assert.True(Canvas.Document.Children.IndexOf(child) < Canvas.Document.Children.IndexOf(child2));
        }

        [Test]
        public async Task If1ElementIsSelected_AndBringToFrontIsSelected_ElementIsBroughtToFront()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var tool = Canvas.Tools.OfType<ArrangeTool>().Single();
            Canvas.ActiveTool = tool;

            var child = new SvgRectangle { X = 50, Y = 50, Width = 50, Height = 50 };
            var child1 = new SvgRectangle { X = 250, Y = 150, Width = 100, Height = 25 };
            var child2 = new SvgRectangle { X = 150, Y = 250, Width = 150, Height = 150 };
            Canvas.ScreenWidth = 800;
            Canvas.ScreenHeight = 500;
            Canvas.Document.Children.Add(child);
            Canvas.Document.Children.Add(child1);
            Canvas.Document.Children.Add(child2);
            Canvas.SelectedElements.Add(child);
            tool.Commands.First(x => x.Name == "Bring to front").Execute(null);

            // Preassert
            Assert.True(Canvas.Document.Children.IndexOf(child) > Canvas.Document.Children.IndexOf(child1));
            Assert.True(Canvas.Document.Children.IndexOf(child) > Canvas.Document.Children.IndexOf(child2));

            // Act
            Canvas.Tools.OfType<UndoRedoTool>().Single().Commands.First(x => x.Name == "Undo").Execute(null);

            // Assert
            Assert.True(Canvas.Document.Children.IndexOf(child) < Canvas.Document.Children.IndexOf(child1));
            Assert.True(Canvas.Document.Children.IndexOf(child) < Canvas.Document.Children.IndexOf(child2));
        }

        [Test]
        public async Task IfMoreElementsAreSelected_AndSendBackwardsIsSelected_ElementsAreSwappedWithPrecursor()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var tool = Canvas.Tools.OfType<ArrangeTool>().Single();
            Canvas.ActiveTool = tool;

            var child = new SvgRectangle { X = 50, Y = 50, Width = 50, Height = 50 };
            var child1 = new SvgRectangle { X = 250, Y = 150, Width = 100, Height = 25 };
            var child2 = new SvgRectangle { X = 150, Y = 250, Width = 150, Height = 150 };
            Canvas.ScreenWidth = 800;
            Canvas.ScreenHeight = 500;
            Canvas.Document.Children.Add(child1);
            Canvas.Document.Children.Add(child);
            Canvas.Document.Children.Add(child2);
            Canvas.SelectedElements.Add(child);
            Canvas.SelectedElements.Add(child2);
            tool.Commands.First(x => x.Name == "Send backward").Execute(null);

            // Preassert
            Assert.True(Canvas.Document.Children.IndexOf(child) < Canvas.Document.Children.IndexOf(child1));
            Assert.True(Canvas.Document.Children.IndexOf(child) < Canvas.Document.Children.IndexOf(child2));
            Assert.True(Canvas.Document.Children.IndexOf(child2) < Canvas.Document.Children.IndexOf(child1));

            // Act
            Canvas.Tools.OfType<UndoRedoTool>().Single().Commands.First(x => x.Name == "Undo").Execute(null);

            // Assert
            Assert.True(Canvas.Document.Children.IndexOf(child) > Canvas.Document.Children.IndexOf(child1));
            Assert.True(Canvas.Document.Children.IndexOf(child) < Canvas.Document.Children.IndexOf(child2));
            Assert.True(Canvas.Document.Children.IndexOf(child2) > Canvas.Document.Children.IndexOf(child1));
        }

        [Test]
        public async Task IfMoreElementsAreSelected_AndSendToBackIsSelected_ElementsAreSentToBack()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var tool = Canvas.Tools.OfType<ArrangeTool>().Single();
            Canvas.ActiveTool = tool;

            var child = new SvgRectangle { X = 50, Y = 50, Width = 50, Height = 50 };
            var child1 = new SvgRectangle { X = 250, Y = 150, Width = 100, Height = 25 };
            var child2 = new SvgRectangle { X = 150, Y = 250, Width = 150, Height = 150 };
            Canvas.ScreenWidth = 800;
            Canvas.ScreenHeight = 500;
            Canvas.Document.Children.Add(child1);
            Canvas.Document.Children.Add(child2);
            Canvas.Document.Children.Add(child);
            Canvas.SelectedElements.Add(child);
            Canvas.SelectedElements.Add(child2);
            tool.Commands.First(x => x.Name == "Send to back").Execute(null);

            // Preassert
            Assert.True(Canvas.Document.Children.IndexOf(child) < Canvas.Document.Children.IndexOf(child1));
            Assert.True(Canvas.Document.Children.IndexOf(child) < Canvas.Document.Children.IndexOf(child2));
            Assert.True(Canvas.Document.Children.IndexOf(child2) < Canvas.Document.Children.IndexOf(child1));

            // Act
            Canvas.Tools.OfType<UndoRedoTool>().Single().Commands.First(x => x.Name == "Undo").Execute(null);

            // Assert
            Assert.True(Canvas.Document.Children.IndexOf(child) > Canvas.Document.Children.IndexOf(child1));
            Assert.True(Canvas.Document.Children.IndexOf(child) > Canvas.Document.Children.IndexOf(child2));
            Assert.True(Canvas.Document.Children.IndexOf(child2) > Canvas.Document.Children.IndexOf(child1));
        }

        [Test]
        public async Task IfMoreElementsAreSelected_AndBringForwardIsSelected_ElementsAreSwappedWithSuccessor()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var tool = Canvas.Tools.OfType<ArrangeTool>().Single();
            Canvas.ActiveTool = tool;

            var child = new SvgRectangle { X = 50, Y = 50, Width = 50, Height = 50 };
            var child1 = new SvgRectangle { X = 250, Y = 150, Width = 100, Height = 25 };
            var child2 = new SvgRectangle { X = 150, Y = 250, Width = 150, Height = 150 };
            Canvas.ScreenWidth = 800;
            Canvas.ScreenHeight = 500;
            Canvas.Document.Children.Add(child1);
            Canvas.Document.Children.Add(child);
            Canvas.Document.Children.Add(child2);
            Canvas.SelectedElements.Add(child);
            Canvas.SelectedElements.Add(child1);
            tool.Commands.First(x => x.Name == "Bring forward").Execute(null);

            // Preassert
            Assert.True(Canvas.Document.Children.IndexOf(child) > Canvas.Document.Children.IndexOf(child1));
            Assert.True(Canvas.Document.Children.IndexOf(child) > Canvas.Document.Children.IndexOf(child2));
            Assert.True(Canvas.Document.Children.IndexOf(child1) > Canvas.Document.Children.IndexOf(child2));

            // Act
            Canvas.Tools.OfType<UndoRedoTool>().Single().Commands.First(x => x.Name == "Undo").Execute(null);

            // Assert
            Assert.True(Canvas.Document.Children.IndexOf(child) > Canvas.Document.Children.IndexOf(child1));
            Assert.True(Canvas.Document.Children.IndexOf(child) < Canvas.Document.Children.IndexOf(child2));
            Assert.True(Canvas.Document.Children.IndexOf(child1) < Canvas.Document.Children.IndexOf(child2));
        }

        [Test]
        public async Task IfMoreElementsAreSelected_AndBringToFrontIsSelected_ElementsAreBroughtToFront()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var tool = Canvas.Tools.OfType<ArrangeTool>().Single();
            Canvas.ActiveTool = tool;

            var child = new SvgRectangle { X = 50, Y = 50, Width = 50, Height = 50 };
            var child1 = new SvgRectangle { X = 250, Y = 150, Width = 100, Height = 25 };
            var child2 = new SvgRectangle { X = 150, Y = 250, Width = 150, Height = 150 };
            Canvas.ScreenWidth = 800;
            Canvas.ScreenHeight = 500;
            Canvas.Document.Children.Add(child);
            Canvas.Document.Children.Add(child1);
            Canvas.Document.Children.Add(child2);
            Canvas.SelectedElements.Add(child);
            Canvas.SelectedElements.Add(child1);
            tool.Commands.First(x => x.Name == "Bring to front").Execute(null);

            // Preassert
            Assert.True(Canvas.Document.Children.IndexOf(child) > Canvas.Document.Children.IndexOf(child1));
            Assert.True(Canvas.Document.Children.IndexOf(child) > Canvas.Document.Children.IndexOf(child2));
            Assert.True(Canvas.Document.Children.IndexOf(child1) > Canvas.Document.Children.IndexOf(child2));

            // Act
            Canvas.Tools.OfType<UndoRedoTool>().Single().Commands.First(x => x.Name == "Undo").Execute(null);

            // Assert
            Assert.True(Canvas.Document.Children.IndexOf(child) < Canvas.Document.Children.IndexOf(child1));
            Assert.True(Canvas.Document.Children.IndexOf(child) < Canvas.Document.Children.IndexOf(child2));
            Assert.True(Canvas.Document.Children.IndexOf(child1) < Canvas.Document.Children.IndexOf(child2));
        }

        private class MockStrokeStyleOptionsInputService : IStrokeStyleOptionsInputService
        {
            public Func<string, IEnumerable<string>, int, IEnumerable<string>, int, StrokeStyleTool.StrokeStyleOptions> F
            {
                get; set;
            } = (arg1, arg2, arg3, arg4, arg5) => null;

            public Task<StrokeStyleTool.StrokeStyleOptions> GetUserInput(string title, IEnumerable<string> strokeDashOptions, int strokeDashSelected, IEnumerable<string> strokeWidthOptions,
                int strokeWidthSelected)
            {
                return Task.FromResult(F(title, strokeDashOptions, strokeDashSelected, strokeWidthOptions, strokeWidthSelected));
            }
        }
    }
}