using Moq;
using NUnit.Framework;
using Svg.Editor.Core.Test;
using Svg.Editor.Core.Test.Mocks;
using Svg.Editor.Events;
using Svg.Editor.Interfaces;
using Svg.Editor.Tools;
using Svg.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Svg.Editor.Core.Tests
{
    // Regression: colorizing a selected pin used to branch on Canvas.ActiveTool being
    // ISupportTextColor (only the PinTool is). Because pins are normally selected with the
    // Move/Selection tool, the color tool then colorized the group instead of the shape child -
    // and since the shape carries its own fill, the pin visibly kept its old color. Coloring must
    // work regardless of which tool is active.
    [TestFixture]
    public class PinColorSelectionTests : SvgDrawingCanvasTestBase
    {
        private Mock<IPinInputService> _pinInputServiceMock;
        private MockColorInputService _colorMock;

        protected override void SetupOverride()
        {
            _pinInputServiceMock = new Mock<IPinInputService>();

            var pinToolProperties = new Dictionary<string, object>
            {
                {"pinsizenames", new[] {"Small", "Medium", "Large", "ExtraLarge" } }
            };

            Canvas.LoadTools(
                () => new MoveTool(SvgEngine.Resolve<IUndoRedoService>()),
                () => new PinTool(pinToolProperties, SvgEngine.Resolve<IUndoRedoService>()),
                () => new ColorTool(new Dictionary<string, object>(), SvgEngine.Resolve<IUndoRedoService>()));

            _colorMock = new MockColorInputService();
            SvgEngine.Register<IColorInputService>(() => _colorMock);
            SvgEngine.Register<ITextInputService>(() => new MockTextInputService());
            SvgEngine.Register<IPinInputService>(() => _pinInputServiceMock.Object);
        }

        [Test]
        public async Task WhenPinColoredWhileSelectToolActive_ShapeGetsChosenColor()
        {
            // Arrange - create a pin (with the PinTool active)
            await Canvas.EnsureInitialized();
            Canvas.ActiveTool = Canvas.Tools.OfType<PinTool>().Single();
            await LongPress(PointF.Create(50, 50));
            var pin = (SvgVisualElement)Canvas.Document.Children[Canvas.Document.Children.Count - 1];
            var shape = pin.Children[0];

            // Act - select the pin with a NON-pin tool active and colorize it red
            Canvas.ActiveTool = Canvas.Tools.OfType<MoveTool>().Single();
            _colorMock.Hex = "#FF0000";
            Canvas.SelectedElements.Clear();
            Canvas.SelectedElements.Add(pin);
            var changeColorCommand = Canvas.Tools.OfType<ColorTool>().Single()
                .Commands.Single(c => c.Name == "Change color");
            changeColorCommand.Execute(null);
            await Task.Delay(50); // Execute is async void; let it settle

            // Assert - the pin's shape (not just the group) must take the chosen color
            var fill = shape.Fill as SvgColourServer;
            Assert.AreEqual(Color.Create("#FF0000").ToString(), fill?.Colour.ToString(),
                "Pin shape was not colored when selected with a non-pin tool active");
        }

        private async Task LongPress(PointF position)
        {
            await Canvas.OnEvent(new PointerEvent(EventType.PointerDown, position, position, position, 1));
            await Task.Delay(TimeSpan.FromMilliseconds(900));
            await Canvas.OnEvent(new PointerEvent(EventType.PointerUp, position, position, position, 1));
        }

        private class MockColorInputService : IColorInputService
        {
            public string Hex { get; set; } = "#000000";
            public Task<string> GetHexaColorFromUserInput(string title) => Task.FromResult(Hex);
        }
    }
}
