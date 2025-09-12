using System.Linq;
using Shouldly;
using NUnit.Framework;
using Svg.Interfaces;

namespace Svg.Tests.Win
{
    /// <summary>
    /// Hint: Try online in editor of w3schools
    /// https://www.w3schools.com/graphics/tryit.asp?filename=trysvg_rect
    /// </summary>
    [TestFixture]
    public class SvgHitTests_Rectangle
    {
        [SetUp]
        public void SetUp()
        {
            SvgPlatform.Init();
        }

        [TestCase("outside tap w/o fill", SelectionType.Intersect, 75, 75, 10, "none", false)]
        [TestCase("outside tap w fill", SelectionType.Intersect, 200, 10, 10, "lime", false)]
        [TestCase("center tap w/o fill", SelectionType.Intersect, 150, 150, 10, "none", false)]
        [TestCase("center tap w fill", SelectionType.Intersect, 150, 150, 10, "lime", true)]
        [TestCase("top line tap w/o fill", SelectionType.Intersect, 150, 95, 10, "none", true)]
        [TestCase("~top line tap w/o fill", SelectionType.Intersect, 155, 100, 10, "none", true)]
        [TestCase("right line tap w/o fill", SelectionType.Intersect, 195, 150, 10, "none", true)]
        [TestCase("~right line tap w/o fill", SelectionType.Intersect, 199, 150, 10, "none", true)]
        [TestCase("lower line tap w/o fill", SelectionType.Intersect, 150, 195, 10, "none", true)]
        [TestCase("~lower line tap w/o fill", SelectionType.Intersect, 150, 199, 10, "none", true)]
        [TestCase("left line tap w/o fill", SelectionType.Intersect, 95, 150, 10, "none", true)]
        [TestCase("~left line tap w/o fill", SelectionType.Intersect, 100, 150, 10, "none", true)]
        public void IsHit(string ___, SelectionType selectionType, float x, float y, float wh, string fill, bool expectsHitSuccessful)
        {
            // Arrange
            var rawSvg = $@"
<svg height=""500"" width=""500"" id=""karo"">
  <rect x=""100"" y=""100"" width=""100"" height=""100"" style=""fill:{fill};stroke:purple;stroke-width:1"" />
</svg>";
            var svg = SvgDocument.FromSvg<SvgDocument>(rawSvg);
            var rect = RectangleF.Create(x, y, wh, wh);

            // Act
            var result = svg.HitTest<SvgVisualElement>(rect, selectionType);

            // Assert
            if (!expectsHitSuccessful)
                result.ShouldBeEmpty();
            else
                result.Count().ShouldBe(1);
        }
    
        [TestCase("outside tap w/o fill", SelectionType.Intersect, 75, 75, 10, "none", false)]
        [TestCase("outside tap w fill", SelectionType.Intersect, 200, 10, 10, "lime", false)]
        [TestCase("center tap w/o fill", SelectionType.Intersect, 150, 150, 10, "none", false)]
        [TestCase("center tap w fill", SelectionType.Intersect, 150, 150, 10, "lime", true)]
        [TestCase("top line tap w/o fill", SelectionType.Intersect, 150, 95, 10, "none", true)]
        [TestCase("~top line tap w/o fill", SelectionType.Intersect, 155, 100, 10, "none", true)]
        [TestCase("right line tap w/o fill", SelectionType.Intersect, 195, 150, 10, "none", true)]
        [TestCase("~right line tap w/o fill", SelectionType.Intersect, 199, 150, 10, "none", true)]
        [TestCase("lower line tap w/o fill", SelectionType.Intersect, 150, 195, 10, "none", true)]
        [TestCase("~lower line tap w/o fill", SelectionType.Intersect, 150, 199, 10, "none", true)]
        [TestCase("left line tap w/o fill", SelectionType.Intersect, 95, 150, 10, "none", true)]
        [TestCase("~left line tap w/o fill", SelectionType.Intersect, 100, 150, 10, "none", true)]
        public void OnZoomedPannedCanvas_IsHit(string ___, SelectionType selectionType, float x, float y, float wh, string fill, bool expectsHitSuccessful)
        {
            // Arrange
            var rawSvg = $@"
<svg height=""500"" width=""500"" id=""karo"">
  <rect x=""100"" y=""100"" width=""100"" height=""100"" style=""fill:{fill};stroke:purple;stroke-width:1"" />
</svg>";
            var svg = SvgDocument.FromSvg<SvgDocument>(rawSvg);
            var rect = RectangleF.Create(x, y, wh, wh);

            // simulate user zooming and panning
            var canvasMatrix = Matrix.Create();
            canvasMatrix.Translate(100, 200);
            canvasMatrix.Scale(3, 3);
            // and selecting some rectangle on panned&zoomed canvas
            rect = canvasMatrix.TransformRectangle(rect);

            // Act
            var result = svg.HitTest<SvgVisualElement>(rect, selectionType, matrix:canvasMatrix);

            // Assert
            if (!expectsHitSuccessful)
                result.ShouldBeEmpty();
            else
                result.Count().ShouldBe(1);
        }
    }
}