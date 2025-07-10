using System.Linq;
using NUnit.Framework;
using Shouldly;
using Svg.Interfaces;

namespace Svg.Tests.Win
{
    /// <summary>
    /// Hint: Try online in editor of w3schools
    /// https://www.w3schools.com/graphics/tryit.asp?filename=trysvg_line
    /// </summary>
    [TestFixture]
    public class SvgHitTests_Line
    {
        [SetUp]
        public void SetUp()
        {
            SvgPlatform.Init();
        }

        [TestCase("outside tap", SelectionType.Intersect, 75, 75, 10, false)]
        [TestCase("center tap", SelectionType.Intersect, 150, 130, 10, false)]
        [TestCase("line line tap", SelectionType.Intersect, 145, 145, 10, true)]
        [TestCase("~line line tap", SelectionType.Intersect, 149, 144, 10, true)]
        public void IsHit(string ___, SelectionType selectionType, float x, float y, float wh, bool expectsHitSuccessful)
        {
            // Arrange
            var rawSvg = $@"
<svg height=""500"" width=""500"" id=""karo"">
  <line x1=""100"" y1=""100"" x2=""200"" y2=""200"" style=""stroke:purple;stroke-width:1"" />
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

        [TestCase("outside tap", SelectionType.Intersect, 75, 75, 10, false)]
        [TestCase("center tap", SelectionType.Intersect, 150, 130, 10, false)]
        [TestCase("line line tap", SelectionType.Intersect, 145, 145, 10, true)]
        [TestCase("~line line tap", SelectionType.Intersect, 149, 144, 10, true)]
        public void OnZoomedPannedCanvas_IsHit(string ___, SelectionType selectionType, float x, float y, float wh, bool expectsHitSuccessful)
        {
            // Arrange
            var rawSvg = $@"
<svg height=""500"" width=""500"" id=""karo"">
  <line x1=""100"" y1=""100"" x2=""200"" y2=""200"" style=""stroke:purple;stroke-width:1"" />
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

        [TestCase("outside tap", SelectionType.Intersect, 75, 75, 10, false)]
        [TestCase("line x tap", SelectionType.Intersect, 0, 100, 10, true)]
        [TestCase("line end tap", SelectionType.Intersect, -5, 200, 10, true)]
        public void VerticalLine(string ___, SelectionType selectionType, float x, float y, float wh, bool expectsHitSuccessful)
        {
            // Arrange
            var rawSvg = $@"
<svg height=""500"" width=""500"" id=""karo"">
  <line x1=""0"" y1=""0"" x2=""0"" y2=""200"" style=""stroke:purple;stroke-width:1"" />
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

        [TestCase("outside tap", SelectionType.Intersect, 75, 75, 10, false)]
        [TestCase("line x tap", SelectionType.Intersect, 0, 100, 10, true)]
        [TestCase("line end tap", SelectionType.Intersect, -5, 200, 10, true)]
        public void OnZoomedPannedCanvas_VerticalLine(string ___, SelectionType selectionType, float x, float y, float wh, bool expectsHitSuccessful)
        {
            // Arrange
            var rawSvg = $@"
<svg height=""500"" width=""500"" id=""karo"">
  <line x1=""0"" y1=""0"" x2=""0"" y2=""200"" style=""stroke:purple;stroke-width:1"" />
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

        [TestCase("outside tap", SelectionType.Intersect, 75, 75, 10, false)]
        [TestCase("~x start tap", SelectionType.Intersect, -5, 0, 10, true)]
        [TestCase("~y start tap", SelectionType.Intersect, 0, -5, 10, true)]
        [TestCase("center tap", SelectionType.Intersect, 100, 0, 10, true)]
        public void HorizontalLine(string ___, SelectionType selectionType, float x, float y, float wh, bool expectsHitSuccessful)
        {
            // Arrange
            var rawSvg = $@"
<svg height=""500"" width=""500"" id=""karo"">
  <line x1=""0"" y1=""0"" x2=""200"" y2=""0"" style=""stroke:purple;stroke-width:1"" />
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

        [TestCase("outside tap", SelectionType.Intersect, 75, 75, 10, false)]
        [TestCase("~x start tap", SelectionType.Intersect, -5, 0, 10, true)]
        [TestCase("~y start tap", SelectionType.Intersect, 0, -5, 10, true)]
        [TestCase("center tap", SelectionType.Intersect, 100, 0, 10, true)]
        public void OnZoomedPannedCanvas_HorizontalLine(string ___, SelectionType selectionType, float x, float y, float wh, bool expectsHitSuccessful)
        {
            // Arrange
            var rawSvg = $@"
<svg height=""500"" width=""500"" id=""karo"">
  <line x1=""0"" y1=""0"" x2=""200"" y2=""0"" style=""stroke:purple;stroke-width:1"" />
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