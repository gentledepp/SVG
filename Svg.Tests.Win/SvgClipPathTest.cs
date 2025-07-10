using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using Shouldly;
using SkiaSharp;
using Svg.Interfaces;
using Svg.Pathing;
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

        var svgDoc = SvgDocument.Open<SvgDocument>("Assets\\"+ svgPath);


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

        var svgDoc = SvgDocument.Open<SvgDocument>("Assets\\" + svgPath);

        var clip = svgDoc.Children.First().Children.OfType<SvgClipPath>().First();

       clip.Bounds.Width.ShouldBe(200);
       clip.Bounds.Height.ShouldBe(50);
    }
}