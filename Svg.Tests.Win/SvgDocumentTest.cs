using NUnit.Framework;
using Shouldly;
using System;
using System.Collections.Generic;
using System.IO;
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
            bitMap.Width.ShouldBe((int)bounds.Width);
            bitMap.Height.ShouldBe((int)bounds.Height);
        }

        [Test]
        public void WhenSvgDocumentDrawsAllContent_ThenBitmapHasBoundsSizeOfImage()
        {
            // Arrange
            var rawSvg = $@"
<svg height=""500"" width=""500"">
  <path d=""M50 250 L50 50 H300 V250 Z M150 300 L400 400"" fill=""none"" stroke =""black""/>
  <image x=""50"" y=""100"" width=""300"" height=""200""/>
</svg>";
            var svg = SvgDocument.FromSvg<SvgDocument>(rawSvg);
            // Act
            var bounds = svg.CalculateDocumentImageBounds();

            // Assert
            ((int)bounds.Width).ShouldBe(300);
            ((int)bounds.Height).ShouldBe(200);
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

            var path = Path.Combine(TestContext.CurrentContext.WorkDirectory, "Assets", svgPath);

            var svgDoc = SvgDocument.Open<SvgDocument>(path);
            var bounds = svgDoc.CalculateDocumentBounds();
            Assert.AreEqual(200, bounds.Width);
            Assert.AreEqual(50, bounds.Height);
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

            bounds.Height.ShouldBe(100);
            bounds.Width.ShouldBe(200);
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

            bounds.Height.ShouldBe(200);
            bounds.Width.ShouldBe(200);
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

            bounds.Height.ShouldBe(200);
            bounds.Width.ShouldBe(200);
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

            bounds.Height.ShouldBe(170);
            bounds.Width.ShouldBe(180);
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

            bounds.Height.ShouldBe(200);
            bounds.Width.ShouldBe(200);
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

            bounds.Height.ShouldBe(170);
            bounds.Width.ShouldBe(180);
        }

        [Test]
        public void WhenCallingDescendants_ThenAllElementsAreReturnedInDocumentOrder()
        {
            // Arrange
            var rawSvg = $@"
<svg height=""500"" width=""500"">
  <g id=""group1"">
    <rect id=""rect1"" width=""10"" height=""10""/>
    <rect id=""rect2"" width=""10"" height=""10""/>
  </g>
  <g id=""group2"">
    <circle id=""circle1"" r=""5""/>
  </g>
</svg>";
            var svg = SvgDocument.FromSvg<SvgDocument>(rawSvg);

            // Act
            var descendants = new[] { (SvgElement)svg }.Descendants().ToList();

            // Assert
            var expectedIds = new[] { "group1", "rect1", "rect2", "group2", "circle1" };
            descendants.Select(e => e.ID).ShouldBe(expectedIds);
        }

        [Test]
        public void WhenCallingDescendants_ThenTheRootDocumentItselfIsNotIncluded()
        {
            // Arrange
            var rawSvg = $@"
<svg height=""500"" width=""500"">
  <rect id=""rect1"" width=""10"" height=""10""/>
</svg>";
            var svg = SvgDocument.FromSvg<SvgDocument>(rawSvg);

            // Act
            var descendants = new[] { (SvgElement)svg }.Descendants().ToList();

            // Assert
            descendants.ShouldNotContain(svg);
            descendants.Select(e => e.ID).ShouldBe(new[] { "rect1" });
        }

        [Test]
        public void WhenCallingDescendants_OnElementWithNoChildren_ThenResultIsEmpty()
        {
            // Arrange
            var rawSvg = $@"
<svg height=""500"" width=""500"">
  <rect id=""rect1"" width=""10"" height=""10""/>
</svg>";
            var svg = SvgDocument.FromSvg<SvgDocument>(rawSvg);
            var rect = svg.Children.OfType<SvgRectangle>().Single();

            // Act
            var descendants = new[] { (SvgElement)rect }.Descendants().ToList();

            // Assert
            descendants.ShouldBeEmpty();
        }

        [Test]
        public void WhenCallingDescendants_WithNestedGroups_ThenDeepDescendantsAreIncluded()
        {
            // Arrange
            var rawSvg = $@"
<svg height=""500"" width=""500"">
  <g id=""outer"">
    <g id=""inner"">
      <rect id=""deepRect"" width=""10"" height=""10""/>
    </g>
  </g>
</svg>";
            var svg = SvgDocument.FromSvg<SvgDocument>(rawSvg);

            // Act
            var descendants = new[] { (SvgElement)svg }.Descendants().ToList();

            // Assert
            descendants.Select(e => e.ID).ShouldBe(new[] { "outer", "inner", "deepRect" });
        }

        [Test]
        public void WhenCallingDescendants_OnCssStyledDocument_ThenAllTextElementsAreFound()
        {
            // Reuses the same document as WhenDocumentOpenWithCss_ElementsAdaptStyle,
            // but goes through the public extension method instead of GetDescendants()
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
            var svg = SvgDocument.FromSvg<SvgDocument>(rawSvg);

            // Act
            var texts = new[] { (SvgElement)svg }.Descendants().OfType<SvgTextBase>().ToArray();

            // Assert
            texts.Length.ShouldBe(3);
            texts.Select(t => t.Text.Trim()).ShouldBe(new[] { "01", "02", "03" });
        }

        [Test]
        public void WhenCallingDescendants_WithMultipleRootDocuments_ThenEachIsTraversedIndependently()
        {
            // Arrange
            var svg1 = SvgDocument.FromSvg<SvgDocument>(@"<svg><rect id=""a1""/></svg>");
            var svg2 = SvgDocument.FromSvg<SvgDocument>(@"<svg><rect id=""b1""/><rect id=""b2""/></svg>");

            // Act
            var descendants = new[] { (SvgElement)svg1, svg2 }.Descendants().ToList();

            // Assert
            descendants.Select(e => e.ID).ShouldBe(new[] { "a1", "b1", "b2" });
        }

        [Test]
        public void WhenCallingDescendants_WithNullSource_ThenArgumentNullExceptionIsThrown()
        {
            // Arrange
            IEnumerable<SvgElement> source = null;

            // Act / Assert
            Should.Throw<ArgumentNullException>(() => source.Descendants().ToList());
        }
    }
}