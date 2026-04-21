using NUnit.Framework;
using Svg.Editor.Core.Test;
using Svg.Editor.Interfaces;
using Svg.Editor.Tools;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Svg.Editor.Core.Tests
{
    [TestFixture]
    public class PlaceAsBackgroundToolTests : SvgDrawingCanvasTestBase
    {
        [SetUp]
        protected override void SetupOverride()
        {
            var imagePath = System.IO.Path.Combine(TestContext.CurrentContext.TestDirectory, "Assets", "iso_sketch_large.svg");
            var placeAsBackgroundToolProperties = new Dictionary<string, object>
            {
                { PlaceAsBackgroundTool.ImagePathKey, imagePath },
                { PlaceAsBackgroundTool.ChooseBackgroundEnabledKey, false }
            };

            Canvas.LoadTools(
                () => new PlaceAsBackgroundTool(placeAsBackgroundToolProperties, SvgEngine.Resolve<IUndoRedoService>()));
        }

        [Test]
        public async Task WhenInitialized_SetsConstraints()
        {
            // Arrange
            await Canvas.EnsureInitialized();

            Assert.That(Canvas.Constraints != null, "Using a background image should constrain the editor to that image");
        }
    }
}
