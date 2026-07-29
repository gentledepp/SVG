using Shouldly;
using NUnit.Framework;
using SkiaSharp;
using Svg.Editor.Tests;
using Svg.Interfaces;
using Svg.Platform;
using System.IO;
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

        // When the embedded font has no glyph for a character,
        // that character must be measured/drawn via a fallback font, not
        // as the primary font's blank .notdef glyph
        [Test]
        public void EmbeddedSubsetFont_MissingGlyph_IsMeasuredViaFallback_NotNotdef()
        {
            // Top3_1.svg embeds a PdfToSvg.NET subset font (family "f37kpsI") that only contains the
            // handful of glyphs the source PDF used, so most characters are absent from it.
            var svgPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Assets", "Top3_1.svg");
            using var src = File.OpenRead(svgPath);
            using var doc = SvgDocument.Open<SvgDocument>(src);

            var fontFamily = (SkiaFontFamily)SvgEngine.Factory.LoadCustomFontFamily(
                "f37kpsI", SvgFontWeight.Normal, SvgFontStyle.Normal, doc);
            var primary = fontFamily.Typeface;

            using var paint = new SKPaint { Typeface = primary, TextSize = 20f };

            // Find a character absent from the subset font but present in a system fallback, whose
            // fallback advance differs from the primary's blank .notdef advance (so the test is
            // meaningful). BuildTextRuns resolves fallbacks with SKFontManager.Default.MatchCharacter,
            // so we mirror that here to compute the expected width.
            char missing = default;
            float expectedFallbackWidth = 0f, notdefWidth = 0f;
            var found = false;
            foreach (var c in "QWXYZqwxyz@#€§µ")
            {
                if (primary.GetGlyph(c) != 0) continue;                 // primary HAS it -> not missing
                var fb = SKFontManager.Default.MatchCharacter(c);
                if (fb == null || fb.GetGlyph(c) == 0) continue;        // no usable fallback
                using var fbFont = new SKFont(fb, paint.TextSize);
                var fw = fbFont.MeasureText(c.ToString(), paint);
                var nd = paint.MeasureText(c.ToString());               // primary .notdef advance
                if (System.Math.Abs(fw - nd) < 0.5f) continue;          // not discriminating
                missing = c; expectedFallbackWidth = fw; notdefWidth = nd; found = true;
                break;
            }

            found.ShouldBeTrue("expected a character absent from the subset font but present in a fallback");

            var (fixedWidth, _) = paint.MeasureTextWithWhiteSpace(missing.ToString());
            
            fixedWidth.ShouldBe(expectedFallbackWidth, 0.6f);
            // Red before the fix: it would equal the primary .notdef advance instead.
            System.Math.Abs(fixedWidth - notdefWidth).ShouldBeGreaterThan(0.5f);
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
            actual.ShouldBe(3);
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
            actual.ShouldBe(new SvgUnit(SvgUnitType.Pixel, 16));
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
            texts.FirstOrDefault(element => element.Content == "TOP ")?.X.FirstOrDefault().Value.ShouldBe(0); 
            texts.FirstOrDefault(element => element.Content == "TOP ")?.Y.FirstOrDefault().Value.ShouldBe(0); 
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