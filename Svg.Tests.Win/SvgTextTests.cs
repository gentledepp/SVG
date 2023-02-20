using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Svg.Converters;
using Svg.Editor.Tests;
using Svg.Interfaces;

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

    }
}