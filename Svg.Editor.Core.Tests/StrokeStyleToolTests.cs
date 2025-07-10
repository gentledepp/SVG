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
    public class StrokeStyleToolTests : SvgDrawingCanvasTestBase
    {
        private MockStrokeStyleOptionsInputService _mockStrokeStyle;

        protected override void SetupOverride()
        {
            Canvas.LoadTools(
                () => new SelectionTool(SvgEngine.Resolve<IUndoRedoService>()),
                () => new StrokeStyleTool(new Dictionary<string, object>
                {
                    {StrokeStyleTool.StrokeDashesKey, new[] {"none", "3 3", "17 17", "34 34"}},
                    {
                        StrokeStyleTool.StrokeDashNamesKey,
                        new[] {"----------", "- - - - - -", "--  --  --", "---   ---"}
                    },
                    {StrokeStyleTool.StrokeWidthsKey, new[] {2, 6, 12, 24}},
                    {StrokeStyleTool.StrokeWidthNamesKey, new[] {"thin", "normal", "thick", "ultra-thick"}}
                }, SvgEngine.Resolve<IUndoRedoService>()));

            // register stroke style options input service
            _mockStrokeStyle = new MockStrokeStyleOptionsInputService();
            SvgEngine.Register<IStrokeStyleOptionsInputService>(() => _mockStrokeStyle);
        }

        [Test]
        public async Task OneElementSelected_StrokeStyleCommandExecuted_ChildStrokeIsChanged()
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

            // Act
            tool.Commands.First().Execute(null);

            // Assert
            Assert.AreEqual("3 3", ellipse.StrokeDashArray.ToString());
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
