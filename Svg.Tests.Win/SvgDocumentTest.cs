using NUnit.Framework;
using System.Linq;
using Shouldly;

namespace Svg.Tests.Win
{
    [TestFixture]
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
            bitMap.Width.ShouldBe((int)bounds.Width);
            bitMap.Height.ShouldBe((int)bounds.Height);
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
            bitMap.Width.ShouldBe((int)svg.Width.Value);
            bitMap.Height.ShouldBe((int)svg.Height.Value);
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
            bitMap.Width.ShouldBe((int)100);
            bitMap.Height.ShouldBe((int)100);
            bitMap2.Width.ShouldBe((int)svg.Width);
            bitMap2.Height.ShouldBe((int)svg.Height);
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
            bitMap.Width.ShouldBe((int)bounds.Width);
            bitMap.Height.ShouldBe((int)bounds.Height);
        }

        [Test]
        public void WhenSvgDocumentDrawsAllDocumentWithoutSize_ThenSvgDocHasBoundSize()
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
            bitMap.Width.ShouldBe((int)svg.Width);
            bitMap.Height.ShouldBe((int)svg.Height);
        }

        [Test]
        public void WhenSvgDocumentAdaptSize_ThenSvgDocHasIncreasedWidthHeightBy20()
        {
            // Arrange
            var rawSvg = $@"
<svg>
  <path d=""M50 250 L50 50 H300 V250 Z M150 300 L400 400"" fill=""none"" stroke =""black""/>
</svg>";
            var svg = SvgDocument.FromSvg<SvgDocument>(rawSvg);
            var bounds = svg.CalculateDocumentBounds();

            // Act
            svg.AdaptCanvasSizeToElementBounds();

            // Assert
            svg.Width.Value.ShouldBe((int)bounds.Width + 20);
            svg.Height.Value.ShouldBe((int)bounds.Height + 20);
        }

        [Test]
        public void WhenDocumentOpenWithCss_ElementsAdaptStyle()
        {
            // Arrange
            var rawSvg = $@"
<svg height=""500"" width=""500"">
<style>
    .class1{{font:16px; font-family: sans-serif}}
    .class2{{font:11px; fill: #00f}}
</style>
  <text x=""100"" y=""250"" class=""class1"">01</text>
  <text x=""360"" y=""179"" class=""class2"">02</text>
  <text x=""400"" y=""20"" class=""class3"">03</text>
</svg>";

            // Act
            var svg = SvgDocument.FromSvg<SvgDocument>(rawSvg);

            // Assert
            var children = svg.GetDescendants().OfType<SvgTextBase>().ToArray();
            var actualFontSize = children.Select(text => text.FontSize).ToArray();
            actualFontSize[0].ShouldBe(new SvgUnit(SvgUnitType.Pixel, 16));
            actualFontSize[1].ShouldBe(new SvgUnit(SvgUnitType.Pixel, 11));

            var actualFontFamily = children.Select(text => text.FontFamily).ToArray();
            actualFontFamily[0].ShouldBe("sans-serif");
            actualFontFamily[1].ShouldBe("");
        }

        [Test]
        public void WhenGettingDocumentBounds_AndChildrenHaveClipPath_GetBoundsBasedOnChildrenWithClipPath()
        {
            // Arrange
            var svgPath = "ClipPathBounds.svg";

            var svgDoc = SvgDocument.Open<SvgDocument>("Assets\\" + svgPath);

            svgDoc.CalculateDocumentBounds().Width.ShouldBe(200);
            svgDoc.CalculateDocumentBounds().Height.ShouldBe(50);
        }
        
        [Test]
        public void WhenContainsSvgUse_ReferencedElementIsIncludedInCalculatingBounds()
        {
            var svgtxt = """"
                         <?xml version="1.0" encoding="utf-8"?>
                         <svg width="1191" height="842" preserveAspectRatio="xMidYMid meet" viewBox="0 0 1191 842" xmlns="http://www.w3.org/2000/svg">
                             <defs>
                                <rect id="imKXxjs"  width="200" height="100" x="10" y="10" rx="20" ry="20" fill="blue" />
                             </defs>
                         <g>
                             <g>
                                <use href="#imKXxjs" />
                             </g>
                         </g>
                         </svg>
                         """";
            var svg = SvgDocument.FromSvg<SvgDocument>(svgtxt);

            var bounds = svg.CalculateDocumentBounds();

            bounds.Height.ShouldBe(100);
            bounds.Width.ShouldBe(200);
        }
    }
}