using Shouldly;
using NUnit.Framework;
using Svg.Interfaces;

namespace Svg.Tests.Win
{
    [TestFixture]
    public class AttributeCachingTests
    {
        [SetUp]
        public void SetUp()
        {
            SvgPlatform.Init();
        }

        [Test]
        public void CanInheritAttribute()
        {
            // Arrange
            var expectedFill = new SvgColourServer(Color.Create(255, 0, 0));
            var rect = new SvgRectangle()
            {
                X = 100,
                Y = 150,
                Width = 300,
                Height = 50,
                StrokeDashArray = SvgUnitCollection.Inherit,
                Fill = SvgColourServer.Inherit,
                Stroke = SvgColourServer.Inherit
            };
            var group = new SvgGroup()
            {
                Fill = expectedFill,
                StrokeDashArray = new SvgUnitCollection { new SvgUnit(3), new SvgUnit(3) },
                Stroke = new SvgColourServer(Color.Create(0, 255, 0)),

                Children =
                {
                    rect
                }
            };
            var doc = new SvgDocument()
            {
                Children =
                {
                    group
                }
            };

            // Act

            // Assert
            rect.Fill.ShouldBe(expectedFill);
        }

        [Test]
        public void WhenInheritedAttributeChanges_ResetsCache()
        {
            // Arrange
            var rect = new SvgRectangle()
            {
                X = 100,
                Y = 150,
                Width = 300,
                Height = 50,
                StrokeDashArray = SvgUnitCollection.Inherit,
                Fill = SvgColourServer.Inherit,
                Stroke = SvgColourServer.Inherit
            };
            var group = new SvgGroup()
            {
                Fill = new SvgColourServer(Color.Create(255, 0, 0)),
                StrokeDashArray = new SvgUnitCollection { new SvgUnit(3), new SvgUnit(3) },
                Stroke = new SvgColourServer(Color.Create(0, 255, 0)),

                Children =
                {
                    rect
                }
            };
            var doc = new SvgDocument()
            {
                Children =
                {
                    group
                }
            };

            var prev = rect.Fill;

            // Act
            group.Fill = new SvgColourServer(Color.Create(255, 255, 0));


            // Assert
            rect.Fill.ShouldBe(group.Fill, "must change when parent fill changes");
        }

        [Test]
        public void WhenAttributeChanges_ResetsCache()
        {
            // Arrange
            var rect = new SvgRectangle()
            {
                X = 100,
                Y = 150,
                Width = 300,
                Height = 50,
                StrokeDashArray = SvgUnitCollection.Inherit,
                Fill = SvgColourServer.Inherit,
                Stroke = SvgColourServer.Inherit
            };

            var isVisible = rect.Visible;

            // Act
            rect.Attributes["visibility"] = false;

            // Assert
            rect.Visible.ShouldBe(false, "cached attribute must reset when attribute changes");
        }
    }
}
