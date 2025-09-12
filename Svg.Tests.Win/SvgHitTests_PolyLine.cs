using System.Linq;
using Shouldly;
using NUnit.Framework;
using Svg.Interfaces;

namespace Svg.Tests.Win
{
    /// <summary>
    /// Hint: Try online in editor of w3schools
    /// https://www.w3schools.com/graphics/tryit.asp?filename=trysvg_polyline
    /// </summary>
    [TestFixture]
    public class SvgHitTests_PolyLine
    {
        [SetUp]
        public void SetUp()
        {
            SvgPlatform.Init();
        }

        [TestCase("outside tap w/o fill", SelectionType.Intersect, 75, 75, 10, "none", false)]
        [TestCase("outside tap w fill", SelectionType.Intersect, 200, 10, 10, "lime", false)]
        [TestCase("center tap w/o fill", SelectionType.Intersect, 100, 200, 10, "none", false)]
        [TestCase("center tap w fill", SelectionType.Intersect, 100, 200, 10, "lime", true)]
        [TestCase("first line tap w/o fill", SelectionType.Intersect, 150, 150, 10, "none", true)]
        [TestCase("~first line tap w/o fill", SelectionType.Intersect, 155, 150, 10, "none", true)]
        [TestCase("second line tap w/o fill", SelectionType.Intersect, 145, 250, 10, "none", true)]
        [TestCase("~second line tap w/o fill", SelectionType.Intersect, 140, 250, 10, "none", true)]
        [TestCase("third line tap w/o fill", SelectionType.Intersect, 45, 250, 10, "none", true)]
        [TestCase("~third line tap w/o fill", SelectionType.Intersect, 50, 250, 10, "none", true)]
        [TestCase("NOT EXISTNG fourth line tap w/o fill", SelectionType.Intersect, 45, 150, 10, "none", false)]
        [TestCase("NOT EXISTNG ~fourth line tap w/o fill", SelectionType.Intersect, 40, 150, 10, "none", false)]
        public void IsHit(string ___, SelectionType selectionType, float x, float y, float wh, string fill, bool expectsHitSuccessful)
        {
            // Arrange
            var rawSvg = $@"
<svg height=""500"" width=""500"" id=""karo"">
  <polyline points=""100,100 200,200 100,300 0,200"" style=""fill:{fill};stroke:purple;stroke-width:1"" />
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
        [TestCase("center tap w/o fill", SelectionType.Intersect, 100, 200, 10, "none", false)]
        [TestCase("center tap w fill", SelectionType.Intersect, 100, 200, 10, "lime", true)]
        [TestCase("first line tap w/o fill", SelectionType.Intersect, 150, 150, 10, "none", true)]
        [TestCase("~first line tap w/o fill", SelectionType.Intersect, 155, 150, 10, "none", true)]
        [TestCase("second line tap w/o fill", SelectionType.Intersect, 145, 250, 10, "none", true)]
        [TestCase("~second line tap w/o fill", SelectionType.Intersect, 140, 250, 10, "none", true)]
        [TestCase("third line tap w/o fill", SelectionType.Intersect, 45, 250, 10, "none", true)]
        [TestCase("~third line tap w/o fill", SelectionType.Intersect, 50, 250, 10, "none", true)]
        [TestCase("NOT EXISTNG fourth line tap w/o fill", SelectionType.Intersect, 45, 150, 10, "none", false)]
        [TestCase("NOT EXISTNG ~fourth line tap w/o fill", SelectionType.Intersect, 40, 150, 10, "none", false)]
        public void OnZoomedPannedCanvas_IsHit(string ___, SelectionType selectionType, float x, float y, float wh, string fill, bool expectsHitSuccessful)
        {
            // Arrange
            var rawSvg = $@"
<svg height=""500"" width=""500"" id=""karo"">
  <polyline points=""100,100 200,200 100,300 0,200"" style=""fill:{fill};stroke:purple;stroke-width:1"" />
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