using FluentAssertions;
using NUnit.Framework;
using Svg.Editor.Tests;
using Svg.Interfaces;
using System.Linq;

namespace Svg.Tests.Win
{
    public class SvgTextTests
    {
        [SetUp]
        public void SetUp()
        {
            SvgPlatform.Init();
            Svg.SvgEngine.Register<IFileLoader>(() => new FileLoader());
        }

        [Test]
        public void SvgTextSpan_ShouldBeTextBase()
        {
            // Arrange
            var rawSvg = $@"
<svg height=""500"" width=""500"">
  <text x=""360.7884"" y=""179.3841""><tspan>TOP </tspan><tspan dx=""-0.1019"">01</tspan></text>
</svg>";
            var svg = SvgDocument.FromSvg<SvgDocument>(rawSvg);

            // Act
            var actual = svg.GetDescendants().ToArray().OfType<SvgVisualElement>().Count(element => element is SvgTextBase);

            // Assert
            actual.Should().Be(3);
        }

        [Test]
        public void WhenDocumentOpenWithCss_TextHasFontSize()
        {
            // Arrange
            var rawSvg = $@"
<svg height=""500"" width=""500"">
<style>
    .tx3bCQc{{font:16px}}
</style>
  <text x=""360.7884"" y=""179.3841"" class=""tx3bCQc""><tspan class=""tx3bCQc"">TOP </tspan><tspan class=""tx3bCQc"" dx=""-0.1019"">01</tspan></text>
</svg>";

            // Act
            var svg = SvgDocument.FromSvg<SvgDocument>(rawSvg);
            svg.DrawDocument();

            // Assert
            var actual = svg.GetDescendants().OfType<SvgTextBase>().ToArray().Select(text => text.FontSize).FirstOrDefault();
            actual.Should().Be(new SvgUnit(SvgUnitType.Pixel, 16));
        }

        [Test]
        public void WhenLoadingDocument_TextSpanHasNoFontSize()
        {
            // Arrange
            var fileLoader = SvgEngine.Resolve<IFileLoader>();

            // Act
            var document = fileLoader.Load("59e4e3cb-0b9e-4a93-9d10-26d3ebea0369.svg");

            // Assert
            var texts = document.GetDescendants().OfType<SvgTextBase>().ToArray();
            Assert.True(texts.Any(element => element.Content == "m"));
            Assert.True(texts.Any(element => element.Content == "01"));
            Assert.True(texts.Any(element => element.Content == "TOP "));
            texts.FirstOrDefault(element => element.Content == "TOP ")?.X.FirstOrDefault().Value.Should()
                .Be(0); 
            texts.FirstOrDefault(element => element.Content == "TOP ")?.Y.FirstOrDefault().Value.Should()
                .Be(0); 
        }

        [Test]
        public void CanRenderComplexDocumentWithCustomFont()
        {
            var pngPath = "59e4e3cb-0b9e-4a93-9d10-26d3ebea0369.png";
            var svgPath = "59e4e3cb-0b9e-4a93-9d10-26d3ebea0369.svg";

            using var pngBitmap = TestHelper.GetBitmap(pngPath);

            // Act
            using var svgBitmap = TestHelper.RenderSvg(svgPath, pngBitmap.Width, pngBitmap.Height, Color.Create(255, 255, 255));
                
            // Assert
            using var c = TestHelper.ImageCompare(svgBitmap, pngBitmap);


            c.AssertAreSimilar(99, svgPath);
        }

        [Test]
        public void CanRenderTSpan_X()
        {
            var pngPath = "tspan_x.png";
            var svgPath = "tspan_x.svg";

            using var pngBitmap = TestHelper.GetBitmap(pngPath);

            // Act
            using var svgBitmap = TestHelper.RenderSvg(svgPath, pngBitmap.Width, pngBitmap.Height);

            // Assert
            using var c = TestHelper.ImageCompare(svgBitmap, pngBitmap);


            c.AssertAreSimilar(93.3f, svgPath);
        }

        [Test]
        public void CanRenderTSpan_DX()
        {
            var pngPath = "tspan_dx.png";
            var svgPath = "tspan_dx.svg";

            using var pngBitmap = TestHelper.GetBitmap(pngPath);

            // Act
            using var svgBitmap = TestHelper.RenderSvg(svgPath, pngBitmap.Width, pngBitmap.Height);

            // Assert
            using var c = TestHelper.ImageCompare(svgBitmap, pngBitmap);


            c.AssertAreSimilar(92.6f, svgPath);
        }
        

        [Test]
        public void CanRenderTSpan_DX_of_DX()
        {
            var pngPath = "tspan_dx_2.png";
            var svgPath = "tspan_dx_2.svg";

            using var pngBitmap = TestHelper.GetBitmap(pngPath);

            // Act
            using var svgBitmap = TestHelper.RenderSvg(svgPath, pngBitmap.Width, pngBitmap.Height);

            // Assert
            using var c = TestHelper.ImageCompare(svgBitmap, pngBitmap);


            c.AssertAreSimilar(91.1f, svgPath);
        }
        
        [Test]
        public void CanRenderTSpan_DX_WithTransforms()
        {
            var pngPath = "tspan_dx_transform.png";
            var svgPath = "tspan_dx_transform.svg";

            using var pngBitmap = TestHelper.GetBitmap(pngPath);

            // Act
            using var svgBitmap = TestHelper.RenderSvg(svgPath, pngBitmap.Width, pngBitmap.Height);

            // Assert
            using var c = TestHelper.ImageCompare(svgBitmap, pngBitmap);


            c.AssertAreSimilar(91.14f, svgPath);
        }
        
        [Test]
        public void CanRenderTSpan_TakesXYFromParent_IgnoresWhiteSpaces()
        {
            var pngPath = "tspan_dx_withparentx.png";
            var svgPath = "tspan_dx_withparentx.svg";

            using var pngBitmap = TestHelper.GetBitmap(pngPath);

            // Act
            using var svgBitmap = TestHelper.RenderSvg(svgPath, pngBitmap.Width, pngBitmap.Height);

            // Assert
            using var c = TestHelper.ImageCompare(svgBitmap, pngBitmap);


            c.AssertAreSimilar(94.688f, svgPath);
        }
        
        [Test]
        public void CanRenderTSpan_WithWhiteSpaces()
        {
            var pngPath = "tspan_whitespace.png";
            var svgPath = "tspan_whitespace.svg";

            using var pngBitmap = TestHelper.GetBitmap(pngPath);

            // Act
            using var svgBitmap = TestHelper.RenderSvg(svgPath, pngBitmap.Width, pngBitmap.Height);

            // Assert
            using var c = TestHelper.ImageCompare(svgBitmap, pngBitmap);


            c.AssertAreSimilar(74.8936f, svgPath);
        }
    }
}