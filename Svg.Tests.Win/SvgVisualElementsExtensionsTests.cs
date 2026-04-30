using NUnit.Framework;
using Svg.Editor.Tests;
using System.IO;
using System.Linq;

namespace Svg.Tests.Win;

[TestFixture]
public class SvgVisualElementsExtensionsTests
{
    [SetUp]
    public void SetUp()
    {
        SvgPlatform.Init();
    }

    [Test]
    public void WhenGettingBoundsFromVisualElement_WithClipPathSmallerThanElement_GetClipPathBound()
    {
        // Arrange
        var svgPath = "ClipPathBounds.svg";

        var path = Path.Combine(TestContext.CurrentContext.WorkDirectory, "Assets", svgPath);
        var svgDoc = SvgDocument.Open<SvgDocument>(path);
        var rectangle = svgDoc.Children[1].Children.OfType<SvgRectangle>().FirstOrDefault();


        Assert.IsNotNull(rectangle);
        Assert.AreEqual(20000, (int)rectangle.Width);
        Assert.AreEqual(100000, (int)rectangle.Height);
        Assert.AreEqual(200, rectangle.GetBounds().Width);
        Assert.AreEqual(50, rectangle.GetBounds().Height);
        
    }

    [Test]
    public void WhenGettingBoundsFromVisualElement_WithClipPathBiggerThanElement_GetElementBound()
    {
        // Arrange
        var svgPath = "ClipPathBounds.svg";

        var path = Path.Combine(TestContext.CurrentContext.WorkDirectory, "Assets", svgPath);
        var svgDoc = SvgDocument.Open<SvgDocument>(path);
        var rectangle = svgDoc.Children[2].Children.OfType<SvgRectangle>().FirstOrDefault();


        Assert.IsNotNull(rectangle);
        Assert.AreEqual(20, (int)rectangle.Width);
        Assert.AreEqual(10, (int)rectangle.Height);
        Assert.AreEqual(20, rectangle.GetBounds().Width);
        Assert.AreEqual(10, rectangle.GetBounds().Height);
        
    }
}