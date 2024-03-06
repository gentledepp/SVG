using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using SkiaSharp;
using Svg.Interfaces;
using Svg.Pathing;

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
        var pngPath = "test_clip_path.png";
        var svgPath = "clip_path.svg";

        // Act
        //var svgDoc = SvgDocument.Open<SvgDocument>("Assets\\clip_path.svg");

        //var bitMap = svgDoc.DrawDocument();
        //using var file = new FileSystem().OpenWrite("Assets\\test_clip_path.png");
        
        //bitMap.SavePng(file, 100);
        //file.Close();

        using var pngBitmap = TestHelper.GetBitmap(pngPath);

        using var svgBitmap = TestHelper.RenderSvg(svgPath, pngBitmap.Width, pngBitmap.Height);

        // Assert
        using var c = TestHelper.ImageCompare(svgBitmap, pngBitmap);


        c.AssertAreSimilar(97f, svgPath);

    }
}