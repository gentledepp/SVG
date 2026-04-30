using System;
using System.IO;
using System.Linq;
using Shouldly;
using NUnit.Framework;
using SkiaSharp;
using Svg.Platform;

namespace Svg.Tests.Win;

public class SvgClipPathTest
{

    [SetUp]
    public void SetUp()
    {
        SvgPlatform.Init();
    }


    [Test]
    public void WhenClipPathRendered_GroupElementShouldBeCLipped()
    {
        // Arrange
        var pngPath = "test_matrix.png";
        var svgPath = "testBosPlan.svg";
        var path = Path.Combine(TestContext.CurrentContext.WorkDirectory, "Assets", svgPath);
        var svgDoc = SvgDocument.Open<SvgDocument>(path);


        //var bitMap = svgDoc.Draw();
        //using var file = new FileSystem().OpenWrite("Assets\\zeljkoTest.png");

        //bitMap.SavePng(file, 100);
        //file.Close();
        // Act
        using var pngBitmap = TestHelper.GetBitmap(pngPath);

        using var svgBitmap = TestHelper.RenderSvg(svgPath, pngBitmap.Width, pngBitmap.Height);

        // Assert
        using var c = TestHelper.ImageCompare(svgBitmap, pngBitmap);


        c.AssertAreSimilar(97f, svgPath);

    }

    [Test]
    public void WhenGroupWithClipAndChildWithMatrixRenderer_CanvasIsSavedCorrectly()
    {
        // Arrange
        var pngPath = "test_clip_path.png";
        var svgPath = "clip_path.svg";
        //var svgPath = "test_CliPath_with_matrix_Child.svg";


        // Act
        using var pngBitmap = TestHelper.GetBitmap(pngPath);

        using var svgBitmap = TestHelper.RenderSvg(svgPath, pngBitmap.Width, pngBitmap.Height);

        // Assert
        using var c = TestHelper.ImageCompare(svgBitmap, pngBitmap);


        c.AssertAreSimilar(97f, svgPath);

    }

    [Test]
    public void WhenGettingCliPathBounds_BasedOnClipPathsChildren_GetBounds()
    {
        // Arrange
        var svgPath = "ClipPathBounds.svg";

        var path = Path.Combine(TestContext.CurrentContext.WorkDirectory, "Assets", svgPath);
        var svgDoc = SvgDocument.Open<SvgDocument>(path);

        var clip = svgDoc.Children.First().Children.OfType<SvgClipPath>().First();

        Assert.AreEqual(clip.Bounds.Width, 200);
        Assert.AreEqual(clip.Bounds.Height, 50);
    }

    [Test]
    public void WhenGroupClipPathContainsTextTransform_AndTextIsOutsideClip_TextShouldNotBeRendered()
    {
        // Arrange
        const string svg =
            """
            <svg width="200" height="200" viewBox="0 0 200 200" xmlns="http://www.w3.org/2000/svg">
              <defs>
                <clipPath id="c"><rect x="0" y="0" width="200" height="40" /></clipPath>
              </defs>
              <g clip-path="url(#c)"><text x="0" y="0" font-size="48" fill="black" transform="matrix(1 0 0 1 0 150)">text</text></g>
            </svg>
            """;

        // Act
        using var rendered = RenderSvgFromString(svg, 200, 200);

        // Assert
        CountNonTransparentPixels(rendered).ShouldBe(0);
    }

    [Test]
    public void WhenGroupClipPathContainsTextTransform_AndTextIsInsideClip_TextShouldBeRendered()
    {
        // Arrange
        const string svg =
            """
            <svg width="200" height="200" viewBox="0 0 200 200" xmlns="http://www.w3.org/2000/svg">
              <defs>
                <clipPath id="c"><rect x="0" y="120" width="200" height="80" /></clipPath>
              </defs>
              <g clip-path="url(#c)">
                <text x="0" y="0" font-size="48" fill="black" transform="matrix(1 0 0 1 0 150)">text</text>
              </g>
            </svg>
            """;

        // Act
        using var rendered = RenderSvgFromString(svg, 200, 200);

        // Assert
        CountNonTransparentPixels(rendered).ShouldBeGreaterThan(0);
    }

    [Test]
    public void WhenClipPathIsOnGroup_ChildTextTransformShouldNotMoveClipRegion()
    {
        // Arrange
        const string svg =
            """
            <svg width="220" height="220" viewBox="0 0 220 220" xmlns="http://www.w3.org/2000/svg">
              <defs>
                <clipPath id="c"><rect x="0" y="0" width="220" height="60" /></clipPath>
              </defs>
              <g clip-path="url(#c)">
                <text x="5" y="45" font-size="40" fill="black">top</text>
                <text x="5" y="0" font-size="40" fill="black" transform="matrix(1 0 0 1 0 170)">bottom</text>
              </g>
            </svg>
            """;

        // Act
        using var rendered = RenderSvgFromString(svg, 220, 220);

        // Assert
        CountNonTransparentPixelsInRegion(rendered, 0, 0, 220, 70).ShouldBeGreaterThan(0);
        CountNonTransparentPixelsInRegion(rendered, 0, 150, 220, 70).ShouldBe(0);
    }

    private static SKBitmap RenderSvgFromString(string svg, int width, int height)
    {
        using SvgDocument doc = SvgDocument.FromSvg<SvgDocument>(svg);
        using var surface = SKSurface.Create(new SKImageInfo(width, height));
        using var renderer = SvgRenderer.FromGraphics(new SkiaGraphics(surface));
        doc.Draw(renderer);
        using var image = surface.Snapshot();
        return SKBitmap.FromImage(image);
    }

    private static int CountNonTransparentPixels(SKBitmap bitmap)
    {
        return CountNonTransparentPixelsInRegion(bitmap, 0, 0, bitmap.Width, bitmap.Height);
    }

    private static int CountNonTransparentPixelsInRegion(SKBitmap bitmap, int x, int y, int width, int height)
    {
        var xStart = x < 0 ? 0 : x;
        var yStart = y < 0 ? 0 : y;
        var xEnd = x + width > bitmap.Width ? bitmap.Width : x + width;
        var yEnd = y + height > bitmap.Height ? bitmap.Height : y + height;

        var count = 0;
        for (var yy = yStart; yy < yEnd; yy++)
        {
            for (var xx = xStart; xx < xEnd; xx++)
            {
                if (bitmap.GetPixel(xx, yy).Alpha > 0)
                {
                    count++;
                }
            }
        }

        return count;
    }
}