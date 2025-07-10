using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Svg.Editor.Interfaces;
using Svg.Editor.Tools;
using Svg.Editor.UndoRedo;
using Svg.Interfaces;

namespace Svg.Editor.Core.Test
{
    [TestFixture]
    public class ColorToolTests : SvgDrawingCanvasTestBase
    {
        private MockColorInputService _colorMock;

        [SetUp]
        protected override void SetupOverride()
        {
            // register the tool provder with a single color tool
            var selectableColors = new[] { "#000000", "#FF0000", "#00FF00", "#0000FF", "#FFFF00", "#FF00FF", "#00FFFF" };
            Canvas.LoadTools(() => new ColorTool(new Dictionary<string, object>
                {
                    { ColorTool.SelectableColorsKey, selectableColors },
                    { ColorTool.SelectableColorNamesKey, new [] { "Black","Red","Green","Blue", "Yellow", "Magenta", "Cyan" } }
                }, SvgEngine.Resolve<IUndoRedoService>()));

            // register the mock color input service
            _colorMock = new MockColorInputService();
            SvgEngine.Register<IColorInputService>(() => _colorMock);
        }

        [Test]
        public async Task WhenUserCreatesText_FillAndStrokeHasSelectedColor()
        {
            // Arrange

            await Canvas.EnsureInitialized();
            var colorTool = Canvas.Tools.OfType<ColorTool>().Single();
            
            var text = new SvgText("hello");
            text.Stroke = new SvgColourServer(Color.Create("#8e8e8e"));
            colorTool.SelectedColorIndex = 2;
            var color = colorTool.SelectableColors[2];

            // Preassert
            Assert.AreNotEqual(color, ((SvgColourServer) text.Fill)?.Colour.ToString());
            Assert.AreNotEqual(color, ((SvgColourServer) text.Stroke)?.Colour.ToString());

            // Act
            await Canvas.AddItemInScreenCenter(text);

            // Assert
            Assert.AreEqual(color, ((SvgColourServer) text.Fill)?.Colour.ToString());
            Assert.AreEqual(color, ((SvgColourServer) text.Stroke)?.Colour.ToString());
        }

        [Test]
        public async Task WhenUserCreatesRectangle_StrokeHasSelectedColor()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var colorTool = Canvas.Tools.OfType<ColorTool>().Single();
            var rectangle = new SvgRectangle();
            rectangle.Stroke = new SvgColourServer(Color.Create("#8e8e8e"));

            // Act
            await Canvas.AddItemInScreenCenter(rectangle);

            // Assert
            var color = colorTool.SelectableColors[0];
            Assert.AreEqual(color, ((SvgColourServer) rectangle.Stroke).Colour.ToString());
        }
        
        [Test]
        public async Task WhenUserCreatesRectangle_ThatHasNoStrokeSet_StrokeIsNotSetByColorTool()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var colorTool = Canvas.Tools.OfType<ColorTool>().Single();
            var rectangle = new SvgRectangle();

            // Act
            await Canvas.AddItemInScreenCenter(rectangle);

            // Assert
            Assert.IsNull(rectangle.Stroke);
        }

        [Test]
        public async Task WhenUserCreatesRectangle_ThatHasNoFillSet_StrokeIsNotSetByColorTool()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var colorTool = Canvas.Tools.OfType<ColorTool>().Single();
            var rectangle = new SvgRectangle();

            // Act
            await Canvas.AddItemInScreenCenter(rectangle);

            // Assert
            Assert.AreSame(SvgColourServer.NotSet, rectangle.Fill);
        }

        [Test]
        public async Task WhenUserSelectsColorAndCreatesText_StrokeAndFillHasSelectedColor()
        {
            // Arrange

            await Canvas.EnsureInitialized();
            var colorTool = Canvas.Tools.OfType<ColorTool>().Single();
            var text = new SvgText("hello");
            text.Stroke = new SvgColourServer(Color.Create("#8e8e8e"));

            // Act

            colorTool.SelectedColorIndex = 1;
            await Canvas.AddItemInScreenCenter(text);

            // Assert

            Assert.AreEqual(Color.Create(colorTool.SelectableColors[1]), ((SvgColourServer) text.Stroke).Colour);
            Assert.AreEqual(Color.Create(colorTool.SelectableColors[1]), ((SvgColourServer) text.Fill).Colour);
        }

        [Test]
        public async Task WhenUserSelectsColorAndCreatesRectangle_StrokeHasSelectedColor()
        {
            // Arrange

            await Canvas.EnsureInitialized();
            var colorTool = Canvas.Tools.OfType<ColorTool>().Single();
            var rectangle = new SvgRectangle();
            rectangle.Stroke = new SvgColourServer(Color.Create("#8e8e8e"));

            // Act

            colorTool.SelectedColorIndex = 1;
            await Canvas.AddItemInScreenCenter(rectangle);

            // Assert

            Assert.AreEqual(((SvgColourServer) rectangle.Stroke).Colour.ToString(),
                colorTool.SelectableColors[colorTool.SelectedColorIndex]);
        }

        [Test]
        public async Task WhenUserSelectsTextAndSelectsColor_StrokeAndFillHasSelectedColor()
        {
            // Arrange

            await Canvas.EnsureInitialized();
            var color = Color.Create(Canvas.Tools.OfType<ColorTool>().Single().SelectableColors[1]);
            _colorMock.F = () => 1;
            var text = new SvgText("hello");
            text.Stroke = new SvgColourServer(Color.Create("#8e8e8e"));
            await Canvas.AddItemInScreenCenter(text);
            var changeColorCommand = Canvas.ToolCommands.Single(x => x.FirstOrDefault()?.Name == "Change color").First();

            // Act

            Canvas.SelectedElements.Add(text);
            changeColorCommand.Execute(null);

            // Assert

            Assert.True(color.Equals(((SvgColourServer) text.Stroke).Colour));
            Assert.True(color.Equals(((SvgColourServer) text.Fill).Colour));
        }

        [Test]
        public async Task WhenUserSelectsRectangleAndSelectsColor_StrokeHasSelectedColor()
        {
            // Arrange

            await Canvas.EnsureInitialized();
            var color = Color.Create(Canvas.Tools.OfType<ColorTool>().Single().SelectableColors[1]);
            _colorMock.F = () => 1;
            var rectangle = new SvgRectangle();
            rectangle.Stroke = new SvgColourServer(Color.Create("#8e8e8e"));
            await Canvas.AddItemInScreenCenter(rectangle);
            var changeColorCommand = Canvas.ToolCommands.Single(x => x.FirstOrDefault()?.Name == "Change color").First();

            // Act

            Canvas.SelectedElements.Add(rectangle);
            changeColorCommand.Execute(null);

            // Assert

            Assert.True(color.Equals(((SvgColourServer) rectangle.Stroke).Colour));
        }

        [Test]
        public async Task ChildStrokeSetToNone_WhenUserSelectsParentAndSelectsColor_ChildStrokeIsStillNone()
        {
            // Arrange

            await Canvas.EnsureInitialized();
            _colorMock.F = () => 1;
            var parent = new SvgGroup();
            var child = new SvgRectangle { X = 10, Y = 10, Width = 60, Height = 40, Stroke = SvgPaintServer.None };
            parent.Children.Add(child);
            await Canvas.AddItemInScreenCenter(parent);
            var changeColorCommand = Canvas.ToolCommands.Single(x => x.FirstOrDefault()?.Name == "Change color").First();

            // Act

            Canvas.SelectedElements.Add(parent);
            changeColorCommand.Execute(null);

            // Assert

            Assert.True(child.Stroke == SvgPaintServer.None);
        }

        [Test]
        public async Task ChildStrokeNotSet_WhenUserSelectsParentAndSelectsColor_ChildStrokeEqualsParentStroke()
        {
            // Arrange

            await Canvas.EnsureInitialized();
            _colorMock.F = () => 1;
            var parent = new SvgGroup();
            parent.Stroke = new SvgColourServer(Color.Create("#8e8e8e"));
            var child = new SvgRectangle { X = 10, Y = 10, Width = 60, Height = 40, Stroke = SvgColourServer.NotSet };
            parent.Children.Add(child);
            await Canvas.AddItemInScreenCenter(parent);
            var changeColorCommand = Canvas.ToolCommands.Single(x => x.FirstOrDefault()?.Name == "Change color").First();

            // Act

            Canvas.SelectedElements.Add(parent);
            changeColorCommand.Execute(null);

            // Assert

            Assert.True(child.Stroke.Equals(parent.Stroke));
        }

        [Test]
        public async Task ChildStrokeInherit_WhenUserSelectsParentAndSelectsColor_ChildStrokeEqualsParentStroke()
        {
            // Arrange

            await Canvas.EnsureInitialized();
            _colorMock.F = () => 1;
            var parent = new SvgGroup();
            parent.Stroke = new SvgColourServer(Color.Create("#8e8e8e"));
            var child = new SvgRectangle { X = 10, Y = 10, Width = 60, Height = 40, Stroke = SvgColourServer.Inherit };
            parent.Children.Add(child);
            await Canvas.AddItemInScreenCenter(parent);
            var changeColorCommand = Canvas.ToolCommands.Single(x => x.FirstOrDefault()?.Name == "Change color").First();

            // Act

            Canvas.SelectedElements.Add(parent);
            changeColorCommand.Execute(null);

            // Assert

            Assert.True(child.Stroke.Equals(parent.Stroke));
        }

        [Test]
        public async Task SetSelectableColorsButNoNamesInProperties_DefaultNamesWillBeUsed()
        {
            // Arrange

            var selectableColors = new[] {"#000000", "#FF0000", "#00FF00", "#0000FF", "#FFFF00", "#FF00FF", "#00FFFF"};

            Canvas = new SvgDrawingCanvas();
            Canvas.LoadTools(() => new ColorTool(new Dictionary<string, object>
                {
                    { "selectablecolors", selectableColors }
                }, new UndoRedoService()));

            await Canvas.EnsureInitialized();

            // Act

            var selectableColorNames = Canvas.Tools.OfType<ColorTool>().First().SelectableColorNames;

            // Assert

            Assert.AreEqual(selectableColors.Length, selectableColorNames.Length);
            Assert.True(selectableColors.All(s => selectableColorNames.Contains(s)));
        }

        private class MockColorInputService : IColorInputService
        {
            public Func<int> F { get; set; } = () => 0;

            public Task<int> GetIndexFromUserInput(string title, string[] items, string[] colors, int defaultIndex = 0)
            {
                return Task.FromResult(F());
            }
        }
    }
}
