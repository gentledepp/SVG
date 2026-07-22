using NUnit.Framework;
using Svg;
using Svg.Editor.Core.Test;
using Svg.Editor.Interfaces;
using Svg.Editor.Tools;
using Svg.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Svg.Editor.Core.Tests
{
    // Proves the Open/Closed design: a brand-new composite shape can redirect coloring to a child
    // purely by having its tool implement IColorTargetProvider. The ColorTool is not modified and
    // has no knowledge of this shape.
    [TestFixture]
    public class ColorTargetExtensibilityTests : SvgDrawingCanvasTestBase
    {
        private MockColorInputService _colorMock;

        protected override void SetupOverride()
        {
            Canvas.LoadTools(
                () => new WidgetTool(),
                () => new ColorTool(new Dictionary<string, object>(), SvgEngine.Resolve<IUndoRedoService>()));

            _colorMock = new MockColorInputService();
            SvgEngine.Register<IColorInputService>(() => _colorMock);
        }

        [Test]
        public async Task ColorTool_ColorsChildTarget_ForAToolItHasNeverHeardOf()
        {
            // Arrange - a composite element whose visible shape is a child that carries its own fill
            await Canvas.EnsureInitialized();
            var shape = new SvgRectangle
            {
                X = 10, Y = 10, Width = 40, Height = 40,
                Fill = new SvgColourServer(Color.Create("#000000"))
            };
            var widget = new SvgGroup();
            widget.Children.Add(shape);
            widget.CustomAttributes.Add(WidgetTool.WidgetMarker, "");
            Canvas.Document.Children.Add(widget);

            // Act - colorize the selected widget red
            _colorMock.Hex = "#FF0000";
            Canvas.SelectedElements.Add(widget);
            var changeColorCommand = Canvas.Tools.OfType<ColorTool>().Single()
                .Commands.Single(c => c.Name == "Change color");
            changeColorCommand.Execute(null);
            await Task.Delay(50); // Execute is async void; let it settle

            // Assert - the child shape (the provider's target) took the color, not the group
            var fill = shape.Fill as SvgColourServer;
            Assert.AreEqual(Color.Create("#FF0000").ToString(), fill?.Colour.ToString(),
                "ColorTool should color the target reported by IColorTargetProvider");
        }

        /// <summary>A stand-in for "some future shape's tool" the ColorTool has never heard of.</summary>
        private class WidgetTool : ToolBase, IColorTargetProvider
        {
            public const string WidgetMarker = "data-widget";

            public WidgetTool() : base("Widget") { }

            public IEnumerable<SvgElement> GetColorTargets(SvgElement element)
                => element.CustomAttributes.ContainsKey(WidgetMarker) && element.Children.Count > 0
                    ? new[] { element.Children[0] }
                    : null;
        }

        private class MockColorInputService : IColorInputService
        {
            public string Hex { get; set; } = "#000000";
            public Task<string> GetHexaColorFromUserInput(string title) => Task.FromResult(Hex);
        }
    }
}
