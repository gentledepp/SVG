using System.Linq;
using NUnit.Framework;
using Shouldly;
using Svg.Editor.Tests;

namespace Svg.Tests.Win;

/// <summary>
/// Tests for SvgVisualElementsExtensions functionality, specifically testing bounds calculation
/// with different clip path configurations.
/// </summary>
[TestFixture]
public class SvgVisualElementsExtensionsTests
{
    /// <summary>
    /// Initializes the SVG platform before each test execution.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        SvgPlatform.Init();
    }

    /// <summary>
    /// Verifies that when a clip path is smaller than the element, the bounds returned
    /// are those of the clip path rather than the element itself.
    /// </summary>
    [Test]
    public void WhenGettingBoundsFromVisualElement_WithClipPathSmallerThanElement_GetClipPathBound()
    {
        // Arrange
        var svgPath = "ClipPathBounds.svg";
        var svgDoc = SvgDocument.Open<SvgDocument>("Assets\\" + svgPath);
        var rectangle = svgDoc.Children[1].Children.OfType<SvgRectangle>().FirstOrDefault();

        // Act & Assert
        rectangle.ShouldNotBeNull();
        ((int)rectangle.Width).ShouldBe(20000);
        ((int)rectangle.Height).ShouldBe(100000);
        rectangle.GetBounds().Width.ShouldBe(200);
        rectangle.GetBounds().Height.ShouldBe(50);
    }

    /// <summary>
    /// Verifies that when a clip path is larger than the element, the bounds returned
    /// are those of the element rather than the clip path.
    /// </summary>
    [Test]
    public void WhenGettingBoundsFromVisualElement_WithClipPathBiggerThanElement_GetElementBound()
    {
        // Arrange
        var svgPath = "ClipPathBounds.svg";
        var svgDoc = SvgDocument.Open<SvgDocument>("Assets\\" + svgPath);
        var rectangle = svgDoc.Children[2].Children.OfType<SvgRectangle>().FirstOrDefault();

        // Act & Assert
        rectangle.ShouldNotBeNull();
        ((int)rectangle.Width).ShouldBe(20);
        ((int)rectangle.Height).ShouldBe(10);
        rectangle.GetBounds().Width.ShouldBe(20);
        rectangle.GetBounds().Height.ShouldBe(10);
    }
}