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
        public void CanRenderTSpan_X()
        {
            var pngPath = "tspan_x.png";
            var svgPath = "tspan_x.svg";

            using var pngBitmap = TestHelper.GetBitmap(pngPath);

            // Act
            using var svgBitmap = TestHelper.RenderSvg(svgPath, pngBitmap.Width, pngBitmap.Height);

            // Assert
            using var c = TestHelper.ImageCompare(svgBitmap, pngBitmap);


            c.AssertAreSimilar(99f, svgPath);
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


            c.AssertAreSimilar(99f, svgPath);
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


            c.AssertAreSimilar(99f, svgPath);
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


            c.AssertAreSimilar(99f, svgPath);
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


            c.AssertAreSimilar(99f, svgPath);
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


            c.AssertAreSimilar(99f, svgPath);
        }

        [Test]
        public void CanRenderTSpan_WithWhiteSpaces2()
        {
            var pngPath = "tspan_whitespace2.png";
            var svgPath = "tspan_whitespace2.svg";

            using var pngBitmap = TestHelper.GetBitmap(pngPath);

            // Act
            using var svgBitmap = TestHelper.RenderSvg(svgPath, pngBitmap.Width, pngBitmap.Height);

            // Assert
            using var c = TestHelper.ImageCompare(svgBitmap, pngBitmap);


            c.AssertAreSimilar(99f, svgPath);
        }


        [Test]
        public void CanRenderTSpan_WithWhiteTspanPositionedRelativeToEmptyTspan()
        {
            var pngPath = "tspan_withdxon_emptytspan.png";
            var svgPath = "tspan_withdxon_emptytspan.svg";

            using var pngBitmap = TestHelper.GetBitmap(pngPath);

            // Act
            using var svgBitmap = TestHelper.RenderSvg(svgPath, pngBitmap.Width, pngBitmap.Height);

            // Assert
            using var c = TestHelper.ImageCompare(svgBitmap, pngBitmap);


            c.AssertAreSimilar(99f, svgPath);
        }

        [Test]
        public void CanRenderTSpan_WithWhiteTspanPositionedRelativeToEmptyTspan_WithEmbeddedFont()
        {
            var pngPath = "tspan_withdxon_emptytspan_withembeddedfont.png";
            var svgPath = "tspan_withdxon_emptytspan_withembeddedfont.svg";

            using var pngBitmap = TestHelper.GetBitmap(pngPath);

            // Act
            using var svgBitmap = TestHelper.RenderSvg(svgPath, pngBitmap.Width, pngBitmap.Height);

            // Assert
            using var c = TestHelper.ImageCompare(svgBitmap, pngBitmap);


            c.AssertAreSimilar(99f, svgPath);
        }

        [Test]
        public void CanRenderTSpans_WithCssStylingAndEmbeddedFont()
        {
            var pngPath = "Top3_1.png";
            var svgPath = "Top3_1.svg";

            using var pngBitmap = TestHelper.GetBitmap(pngPath);

            // Act
            using var svgBitmap = TestHelper.RenderSvg(svgPath, pngBitmap.Width, pngBitmap.Height);

            // Assert
            using var c = TestHelper.ImageCompare(svgBitmap, pngBitmap);

            c.AssertAreSimilar(92f, svgPath);
        }

        [Test]
        public void CanRenderSymbol()
        {
            var pngPath = "use_symbol.png";
            var svgPath = "use_symbol.svg";

            using var pngBitmap = TestHelper.GetBitmap(pngPath);

            // Act
            using var svgBitmap = TestHelper.RenderSvg(svgPath, pngBitmap.Width, pngBitmap.Height);

            // Assert
            using var c = TestHelper.ImageCompare(svgBitmap, pngBitmap);


            c.AssertAreSimilar(99f, svgPath);
        }

        [Test]
        public void CanRenderSymbol_WithViewBoxTransform()
        {
            var pngPath = "use_symbol_transforms.png";
            var svgPath = "use_symbol_transforms.svg";

            using var pngBitmap = TestHelper.GetBitmap(pngPath);

            // Act
            using var svgBitmap = TestHelper.RenderSvg(svgPath, pngBitmap.Width, pngBitmap.Height);

            
            // Assert
            using var c = TestHelper.ImageCompare(svgBitmap, pngBitmap);


            c.AssertAreSimilar(71.8133316f, svgPath);
        }
    }
}