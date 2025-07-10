using System.Linq;
using NUnit.Framework;
using Shouldly;
using Svg.Interfaces;

namespace Svg.Tests.Win
{
    [TestFixture]
    public class SvgHitTest_Path
    {
        [SetUp]
        public void SetUp()
        {
            SvgPlatform.Init();
        }

        [TestCase("outside tap w/o fill", SelectionType.Intersect, 120, 350, 10, "none", false)]
        [TestCase("outside tap w fill", SelectionType.Intersect, 120, 350, 10, "lime", false)]
        [TestCase("first line tap w/o fill", SelectionType.Intersect, 50, 150, 10, "none", true)]
        [TestCase("~first line tap w/o fill", SelectionType.Intersect, 48, 150, 10, "none", true)]
        [TestCase("second line tap w/o fill", SelectionType.Intersect, 150, 50, 10, "none", true)]
        [TestCase("~second line tap w/o fill", SelectionType.Intersect, 150, 46, 10, "none", true)]
        [TestCase("third line tap w/o fill", SelectionType.Intersect, 300, 150, 10, "none", true)]
        [TestCase("~third line tap w/o fill", SelectionType.Intersect, 298, 150, 10, "none", true)]
        [TestCase("fourth line tap w/o fill", SelectionType.Intersect, 150, 250, 10, "none", true)]
        [TestCase("~fourth line tap w/o fill", SelectionType.Intersect, 150, 247, 10, "none", true)]
        [TestCase("fifth line tap w/o fill", SelectionType.Intersect, 150, 300, 10, "none", true)]
        [TestCase("~fifth line tap w/o fill", SelectionType.Intersect, 152, 300, 10, "none", true)]
        [TestCase("Between M and Z", SelectionType.Intersect, 50, 260, 10, "none", false)]
        public void IsHit(string ___, SelectionType selectionType, float x, float y, float wh, string fill, bool expectsHitSuccessful)
        {
            // Arrange
            var rawSvg = $@"
<svg height=""500"" width=""500"">
  <path d=""M50 250 L50 50 H300 V250 Z M150 300 L400 400"" fill=""none"" stroke =""black""/>
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

        [TestCase("outside tap w/o fill", SelectionType.Intersect, 120, 350, 10, "none", false)]
        [TestCase("outside tap w fill", SelectionType.Intersect, 120, 350, 10, "lime", false)]
        [TestCase("first line tap w/o fill", SelectionType.Intersect, 50, 150, 10, "none", true)]
        [TestCase("~first line tap w/o fill", SelectionType.Intersect, 48, 150, 10, "none", true)]
        [TestCase("second line tap w/o fill", SelectionType.Intersect, 150, 50, 10, "none", true)]
        [TestCase("~second line tap w/o fill", SelectionType.Intersect, 150, 46, 10, "none", true)]
        [TestCase("third line tap w/o fill", SelectionType.Intersect, 300, 150, 10, "none", true)]
        [TestCase("~third line tap w/o fill", SelectionType.Intersect, 298, 150, 10, "none", true)]
        [TestCase("fourth line tap w/o fill", SelectionType.Intersect, 150, 250, 10, "none", true)]
        [TestCase("~fourth line tap w/o fill", SelectionType.Intersect, 150, 247, 10, "none", true)]
        [TestCase("fifth line tap w/o fill", SelectionType.Intersect, 150, 300, 10, "none", true)]
        [TestCase("~fifth line tap w/o fill", SelectionType.Intersect, 152, 300, 10, "none", true)]
        [TestCase("Between M and Z", SelectionType.Intersect, 50, 260, 10, "none", false)]

        public void OnPannedZoomedCanvas_IsHit(string ___, SelectionType selectionType, float x, float y, float wh, string fill, bool expectsHitSuccessful)
        {
            // Arrange
            var rawSvg = $@"
<svg height=""500"" width=""500"">
  <path d=""M50 250 L50 50 H300 V250 Z M150 300 L400 400"" fill=""none"" stroke =""black""/>
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
            var result = svg.HitTest<SvgVisualElement>(rect, selectionType, matrix: canvasMatrix);

            // Assert
            if (!expectsHitSuccessful)
                result.ShouldBeEmpty();
            else
                result.Count().ShouldBe(1);
        }
    }
}