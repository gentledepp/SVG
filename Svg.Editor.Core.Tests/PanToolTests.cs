using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Svg.Editor.Interfaces;
using Svg.Editor.Tools;

namespace Svg.Editor.Core.Test
{
    [TestFixture]
    public class PanToolTests : SvgDrawingCanvasTestBase
    {
        [SetUp]
        protected override void SetupOverride()
        {
            Canvas.LoadTools(
                () => new SelectionTool(SvgEngine.Resolve<IUndoRedoService>()),
                () => new PanTool(new Dictionary<string, object>()));
        }

        [Test]
        public async Task WhenInitialized_PanToolIsAutomaticallyEnabled()
        {
            // Arrange
            var tool = Canvas.Tools.OfType<PanTool>().Single();

            // Act
            await Canvas.EnsureInitialized();

            // Assert
            Assert.True(tool.IsActive, "should be active by default");
        }
    }
}
