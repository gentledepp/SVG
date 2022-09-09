using System.Linq;
using FluentAssertions;
using NUnit.Framework;
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
        [TestCase("~first line tap w/o fill", SelectionType.Intersect, 50, 150, 10, "none", true)]
        [TestCase("second line tap w/o fill", SelectionType.Intersect, 150, 50, 10, "none", true)]
        [TestCase("~second line tap w/o fill", SelectionType.Intersect, 150, 50, 10, "none", true)]
        [TestCase("third line tap w/o fill", SelectionType.Intersect, 300, 150, 10, "none", true)]
        [TestCase("~third line tap w/o fill", SelectionType.Intersect, 300, 150, 10, "none", true)]
        //[TestCase("NOT EXISTNG fourth line tap w/o fill", SelectionType.Intersect, 45, 150, 10, "none", false)]
        //[TestCase("NOT EXISTNG ~fourth line tap w/o fill", SelectionType.Intersect, 40, 150, 10, "none", false)]
        public void IsHit(string ___, SelectionType selectionType, float x, float y, float wh, string fill, bool expectsHitSuccessful)
        {
            // Arrange
            var rawSvg = $@"
<svg height=""500"" width=""500"">
  <path d=""M50 250 L50 50 L300 50 L300 250"" fill=""{fill}"" stroke =""black""/>
</svg>";
            var svg = SvgDocument.FromSvg<SvgDocument>(rawSvg);
            var rect = RectangleF.Create(x, y, wh, wh);

            var path = new SvgPath();

            // Act
            var result = svg.HitTest<SvgVisualElement>(rect, selectionType);

            // Assert
            if (!expectsHitSuccessful)
                result.Should().BeEmpty();
            else
                result.Should().HaveCount(1);
        }

        [TestCase("outside tap w/o fill", SelectionType.Intersect, 120, 350, 10, "none", false)]
        [TestCase("outside tap w fill", SelectionType.Intersect, 120, 350, 10, "lime", false)]
        [TestCase("first line tap w/o fill", SelectionType.Intersect, 50, 150, 10, "none", true)]
        [TestCase("~first line tap w/o fill", SelectionType.Intersect, 50, 150, 10, "none", true)]
        [TestCase("second line tap w/o fill", SelectionType.Intersect, 150, 50, 10, "none", true)]
        [TestCase("~second line tap w/o fill", SelectionType.Intersect, 150, 50, 10, "none", true)]
        [TestCase("third line tap w/o fill", SelectionType.Intersect, 300, 150, 10, "none", true)]
        [TestCase("~third line tap w/o fill", SelectionType.Intersect, 300, 150, 10, "none", true)]
        public void OnPannedZoomedCanvas_IsHit(string ___, SelectionType selectionType, float x, float y, float wh, string fill, bool expectsHitSuccessful)
        {
            // Arrange
            var rawSvg = $@"
<svg height=""500"" width=""500"">
  <path d=""M50 250 L50 50 L300 50 L300 250"" fill=""{fill}"" stroke =""black""/>
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
                result.Should().BeEmpty();
            else
                result.Should().HaveCount(1);
        }
    }
}