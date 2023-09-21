using FluentAssertions;
using NUnit.Framework;
using SkiaSharp;
using Svg.Interfaces;
using Svg.Platform;
using static System.Net.Mime.MediaTypeNames;

namespace Svg.Tests.Win
{
    [TestFixture]
    public class TestSkPaint
    {

        [SetUp]
        public void SetUp()
        {
            SvgPlatform.Init();
        }

        [Test]
        public void CreateSkPaint_DefaultTextSizeIs12()
        {
            //Arrange
            var paint = new SKPaint();

            //Act
            var actual = paint.TextSize;

            //Assert
            actual.Should().Be(12);

        }

        [Test]
        public void CreateSkiaPen_DefaultTextSizeIs12()
        {
            //Arrange
            var paint = (SkiaPen)SvgEngine.Factory.CreatePen(new SkiaSolidBrush(Color.Create("#FF0000")), 10);

            //Act
            var actual = paint.TextSize;

            //Assert
            actual.Should().Be(14);
        }
    }
}