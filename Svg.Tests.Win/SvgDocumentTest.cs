using FluentAssertions;
using NUnit.Framework;

namespace Svg.Tests.Win
{
    public class SvgDocumentTest
    {

        [SetUp]
        public void SetUp()
        {
            SvgPlatform.Init();
        }

        [Test]
        public void WhenSvgDocumentDrawsAllContent_ThenBitmapHasBoundsSize()
        {
            // Arrange
            var rawSvg = $@"
<svg height=""500"" width=""500"">
  <path d=""M50 250 L50 50 H300 V250 Z M150 300 L400 400"" fill=""none"" stroke =""black""/>
</svg>";
            var svg = SvgDocument.FromSvg<SvgDocument>(rawSvg);
            var bounds = svg.CalculateDocumentBounds();
            // Act
            var bitMap =  svg.DrawAllContents();

            // Assert
            bitMap.Width.Should().Be((int)bounds.Width);
            bitMap.Height.Should().Be((int)bounds.Height);
        }

        [Test]
        public void WhenSvgDocumentDrawsAllDocument_ThenBitmapHasOriginalSize()
        {
            // Arrange
            var rawSvg = $@"
<svg height=""500"" width=""500"">
  <path d=""M50 250 L50 50 H300 V250 Z M150 300 L400 400"" fill=""none"" stroke =""black""/>
</svg>";
            var svg = SvgDocument.FromSvg<SvgDocument>(rawSvg);

            // Act
            var bitMap =  svg.DrawDocument();

            // Assert
            bitMap.Width.Should().Be((int)svg.Width.Value);
            bitMap.Height.Should().Be((int)svg.Height.Value);
        }

        [Test]
        public void WhenSvgDocumentDrawsAllDocument_WithMaxWidthHeight_ThenBitmapHaScaledSize()
        {
            // Arrange
            var rawSvg = $@"
<svg height=""500"" width=""500"">
  <path d=""M50 250 L50 50 H300 V250 Z M150 300 L400 400"" fill=""none"" stroke =""black""/>
</svg>";
            var svg = SvgDocument.FromSvg<SvgDocument>(rawSvg);

            // Act
            var bitMap =  svg.DrawDocument(maxWidthHeight:100);
            var bitMap2 =  svg.DrawDocument(maxWidthHeight:1000);

            // Assert
            bitMap.Width.Should().Be((int)100);
            bitMap.Height.Should().Be((int)100);
            bitMap2.Width.Should().Be((int)svg.Width);
            bitMap2.Height.Should().Be((int)svg.Height);
        }

        [Test]
        public void WhenSvgDocumentDrawsAllDocumentWithoutSize_ThenBitmapHasCalculatedBoundSize()
        {
            // Arrange
            var rawSvg = $@"
<svg>
  <path d=""M50 250 L50 50 H300 V250 Z M150 300 L400 400"" fill=""none"" stroke =""black""/>
</svg>";
            var svg = SvgDocument.FromSvg<SvgDocument>(rawSvg);
            var bounds = svg.CalculateDocumentBounds();

            // Act
            var bitMap =  svg.DrawDocument();

            // Assert
            bitMap.Width.Should().Be((int)bounds.Width);
            bitMap.Height.Should().Be((int)bounds.Height);
        }
    }
}