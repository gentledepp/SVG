using FluentAssertions;
using NUnit.Framework;
using System.Linq;

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
            bitMap.Width.Should().Be((int)svg.Width);
            bitMap.Height.Should().Be((int)svg.Height);
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
            svg.Width.Value.Should().Be((int)bounds.Width + 20);
            svg.Height.Value.Should().Be((int)bounds.Height + 20);
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
            actualFontSize[0].Should().Be(new SvgUnit(SvgUnitType.Pixel, 16));
            actualFontSize[1].Should().Be(new SvgUnit(SvgUnitType.Pixel, 11));

            var actualFontFamily = children.Select(text => text.FontFamily).ToArray();
            actualFontFamily[0].Should().Be("sans-serif");
            actualFontFamily[1].Should().Be("");
        }

        [Test]
        public void WhenGettingDocumentBounds_AndChildrenHaveClipPath_GetBoundsBasedOnChildrenWithClipPath()
        {
            // Arrange
            var svgPath = "ClipPathBounds.svg";

            var svgDoc = SvgDocument.Open<SvgDocument>("Assets\\" + svgPath);

            Assert.AreEqual(200, svgDoc.CalculateDocumentBounds().Width);
            Assert.AreEqual(50, svgDoc.CalculateDocumentBounds().Height);
        }
        
        [Test]
        public void WhenSvgUse_ReferencedElementIsIncludedInCalculatingBounds()
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

            bounds.Height.Should().Be(100);
            bounds.Width.Should().Be(200);
        }


        [Test]
        public void WhenSvgUse_UsesNoClipPathAndNoTranslations_CalculatesBoundsCorrectly()
        {
            var svgXml = """
                         <svg height="200" width="200" xmlns="http://www.w3.org/2000/svg">
                         	<defs class="gt4EC2H">
                         		<clipPath id="clZ12Pv">
                         			<rect x="0" y="0" width="200" height="200"/>
                         		</clipPath>
                         	</defs>
                         	<symbol id="imICNRZ">
                         		<rect x="150" y="150" width="50" height="50" style="fill:yellow;stroke:green;stroke-width:3"/>
                         	</symbol>
                         	<rect width="50" height="50" style="fill:yellow;stroke:green;stroke-width:3"/>
                         	<g clip-path="url(#clZ12Pv)">
                         		<g transform="translate(0,0)">
                         			<use href="#imICNRZ"/>
                         		</g>
                         	</g>
                         </svg>
                         """;

            var svg = SvgDocument.FromSvg<SvgDocument>(svgXml);



            var bounds = svg.CalculateDocumentBounds();

            bounds.Height.Should().Be(200);
            bounds.Width.Should().Be(200);
        }

        [Test]
        public void WhenSvgUse_UsesNoClipPath_CalculatesBoundsCorrectly()
        {
            var svgXml = """
                         <svg height="200" width="200" xmlns="http://www.w3.org/2000/svg">
                         	<defs class="gt4EC2H">
                         		<clipPath id="clZ12Pv">
                         			<rect x="0" y="0" width="200" height="200"/>
                         		</clipPath>
                         	</defs>
                         	<symbol id="imICNRZ">
                         		<rect x="100" y="100" width="50" height="50" style="fill:yellow;stroke:green;stroke-width:3"/>
                         	</symbol>
                         	<rect width="50" height="50" style="fill:yellow;stroke:green;stroke-width:3"/>
                         	<g clip-path="url(#clZ12Pv)">
                         		<g transform="translate(50,50)">
                         			<use href="#imICNRZ"/>
                         		</g>
                         	</g>
                         </svg>
                         """;

            var svg = SvgDocument.FromSvg<SvgDocument>(svgXml);

            var bounds = svg.CalculateDocumentBounds();

            bounds.Height.Should().Be(200);
            bounds.Width.Should().Be(200);
        }

        [Test]
        public void WhenSvgUse_UsesClipPathWithoutTransform_AndContentIsClipped_CalculatesBoundsCorrectly()
        {
            var svgXml = """
                         <svg height="200" width="200" xmlns="http://www.w3.org/2000/svg">
                            	<defs class="gt4EC2H">
                               		<clipPath id="clZ12Pv">
                                  			<rect x="0" y="0" width="180" height="170"/>
                               		</clipPath>
                         	        <symbol id="imICNRZ">
                            		        <rect x="150" y="150" width="50" height="50" style="fill:yellow;stroke:green;stroke-width:3"/>
                         	        </symbol>
                            	</defs>
                            	<rect width="50" height="50" style="fill:yellow;stroke:green;stroke-width:3"/>
                            	<g clip-path="url(#clZ12Pv)">
                               	    <g>
                                  	    <use href="#imICNRZ"/>
                               	    </g>
                            	</g>
                         </svg>
                         """;

            var svg = SvgDocument.FromSvg<SvgDocument>(svgXml);

            var bounds = svg.CalculateDocumentBounds();

            bounds.Height.Should().Be(170);
            bounds.Width.Should().Be(180);
        }


        [Test]
        public void WhenSvgUse_UsesClipPathAndTransform_ButContentIsNotClipped_CalculatesBoundsCorrectly()
        {
            var svgXml = """
                      <svg height="200" width="200" xmlns="http://www.w3.org/2000/svg">
                         	<defs class="gt4EC2H">
                            		<clipPath id="clZ12Pv">
                               			<rect x="0" y="0" width="200" height="200"/>
                            		</clipPath>
                         	</defs>
                         	<symbol id="imICNRZ">
                            		<rect x="100" y="100" width="50" height="50" style="fill:yellow;stroke:green;stroke-width:3"/>
                         	</symbol>
                         	<rect width="50" height="50" style="fill:yellow;stroke:green;stroke-width:3"/>
                         	<g clip-path="url(#clZ12Pv)">
                            	<g transform="translate(50,50)">
                               		<use href="#imICNRZ"/>
                            	</g>
                         	</g>
                      </svg>
                      """;

            var svg = SvgDocument.FromSvg<SvgDocument>(svgXml);

            var bounds = svg.CalculateDocumentBounds();

            bounds.Height.Should().Be(200);
            bounds.Width.Should().Be(200);
        }

        [Test]
        public void WhenSvgUse_UsesClipPathAndTransform_AndContentIsClipped_CalculatesBoundsCorrectly()
        {
            var svgXml = """
                         <svg height="200" width="200" xmlns="http://www.w3.org/2000/svg">
                               	<defs class="gt4EC2H">
                                     	<clipPath id="clZ12Pv">
                                           		<rect x="0" y="0" width="180" height="170"/>
                                     	</clipPath>
                         	        <symbol id="imICNRZ">
                               		        <rect x="100" y="100" width="50" height="50" style="fill:yellow;stroke:green;stroke-width:3"/>
                         	        </symbol>
                               	</defs>
                               	<rect width="50" height="50" style="fill:yellow;stroke:green;stroke-width:3"/>
                               	<g clip-path="url(#clZ12Pv)">
                                  	<g transform="translate(50,50)">
                                     	<use href="#imICNRZ"/>
                                  	</g>
                               	</g>
                         </svg>
                         """;

            var svg = SvgDocument.FromSvg<SvgDocument>(svgXml);

            var bounds = svg.CalculateDocumentBounds();

            bounds.Height.Should().Be(170);
            bounds.Width.Should().Be(180);
        }
    }
}