using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Svg.Tests.Win
{
    [TestFixture]
    public class SvgColorParsingTests
    {

        [SetUp]
        public void SetUp()
        {
            SvgPlatform.Init();
        }

        [TestCase("named", "red")]
        [TestCase("3 digits","#F00")]
        [TestCase("4 digits", "#F00F")]
        [TestCase("6 digits", "#FF0000")]
        [TestCase("8 digits", "#FF0000FF")]
        public void CanParseColor(string _, string color)
        {

            // Arrange
            var rawSvg = $@"
<svg height=""500"" width=""500"">
  <rect width=""100"" height=""50"" fill=""{color}""/>
</svg>";
            var svg = SvgDocument.FromSvg<SvgDocument>(rawSvg);

            // Act
            using var bitMap = svg.DrawAllContents();

            // Assert
            // should not throw exception
            var r = (SvgRectangle)svg.Children[0];
            r.Fill.Should().NotBeNull();
            r.Fill.ToString().Should().Be("#ff0000","all is red");
        }
    }
}
