using NUnit.Framework;
using SkiaSharp;
using Svg.Interfaces;
using Svg.Platform;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using Shouldly;

namespace Svg.Tests.Win
{
    /// <summary>
    /// Helper class providing utilities for SVG rendering and image comparison in tests.
    /// </summary>
    public static class TestHelper
    {
        /// <summary>
        /// Renders an SVG file to a bitmap with the specified dimensions.
        /// </summary>
        /// <param name="svgPath">The path to the SVG file (relative to Assets folder or absolute).</param>
        /// <param name="width">The width of the output bitmap in pixels.</param>
        /// <param name="height">The height of the output bitmap in pixels.</param>
        /// <returns>An SKBitmap containing the rendered SVG content.</returns>
        public static SKBitmap RenderSvg(string svgPath, int width, int height)
        {
            return RenderSvg(svgPath, width, height, null);
        }

        /// <summary>
        /// Renders an SVG file to a bitmap with the specified dimensions and background color.
        /// </summary>
        /// <param name="svgPath">The path to the SVG file (relative to Assets folder or absolute).</param>
        /// <param name="width">The width of the output bitmap in pixels.</param>
        /// <param name="height">The height of the output bitmap in pixels.</param>
        /// <param name="backgroundColor">The background color to fill before rendering (null for transparent).</param>
        /// <returns>An SKBitmap containing the rendered SVG content.</returns>
        public static SKBitmap RenderSvg(string svgPath, int width, int height, Color backgroundColor)
        {
            if (!Path.IsPathRooted(svgPath))
                svgPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Assets", svgPath);

            using var src = File.OpenRead(svgPath);
            using SvgDocument doc = SvgDocument.Open<SvgDocument>(src);

            // Create image info with explicit color type for SkiaSharp 3.x compatibility
            var imageInfo = new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
            using var surface = SKSurface.Create(imageInfo);

            if (surface == null)
                throw new InvalidOperationException($"Failed to create SKSurface with dimensions {width}x{height}");

            using var renderer = SvgRenderer.FromGraphics(new SkiaGraphics(surface));
            if (backgroundColor != null)
                renderer.FillBackground(backgroundColor);

            doc.Draw(renderer);

            using var img = surface.Snapshot();
            using var encodedData = img.Encode(SKEncodedImageFormat.Png, 100);
            using var stream = encodedData.AsStream();
            using var managedStream = new SKManagedStream(stream);

            return SKBitmap.Decode(managedStream);
        }

        /// <summary>
        /// Compares two bitmaps pixel by pixel and generates a similarity percentage and heat map.
        /// </summary>
        /// <param name="actual">The actual bitmap result.</param>
        /// <param name="expected">The expected bitmap result.</param>
        /// <returns>An ImageCompareResult containing similarity percentage and visualization bitmaps.</returns>
        public static ImageCompareResult ImageCompare(SKBitmap actual, SKBitmap expected)
        {
            float correctPixel = 0;
            float pixelAmount = Math.Max(actual.Height, expected.Height) * Math.Max(actual.Width, expected.Width);

            var bitmap = new SKBitmap(
                Math.Max(actual.Width, expected.Width),
                Math.Max(actual.Height, expected.Height),
                SKColorType.Rgb565,
                SKAlphaType.Opaque);

            var red = SKColor.Parse("#FF0000");
            var white = SKColor.Parse("#FFFFFF");
            var yellow = SKColor.Parse("#FFFF00");

            for (var y = 0; y < bitmap.Height; ++y)
            {
                for (var x = 0; x < bitmap.Width; ++x)
                {
                    // Color heat map yellow if the image sizes differ
                    if (x >= actual.Width || y >= actual.Height)
                    {
                        bitmap.SetPixel(x, y, yellow);
                        continue;
                    }
                    if (x >= expected.Width || y >= expected.Height)
                    {
                        bitmap.SetPixel(x, y, yellow);
                        continue;
                    }

                    var c1 = actual.GetPixel(x, y);
                    var c2 = expected.GetPixel(x, y);

                    if (object.Equals(c1.Alpha, c2.Alpha) &&
                        object.Equals(c1.Green, c2.Green) &&
                        object.Equals(c1.Blue, c2.Blue) &&
                        object.Equals(c1.Red, c2.Red))
                    {
                        correctPixel++;
                        bitmap.SetPixel(x, y, white);
                    }
                    else
                    {
                        // Set red pixel for differences
                        bitmap.SetPixel(x, y, red);
                    }
                }
            }

            return new ImageCompareResult((correctPixel / pixelAmount) * 100, bitmap, actual);
        }

        /// <summary>
        /// Loads a bitmap from a PNG file path.
        /// </summary>
        /// <param name="pngPath">The path to the PNG file (relative to Assets folder or absolute).</param>
        /// <returns>An SKBitmap loaded from the specified file.</returns>
        public static SKBitmap GetBitmap(string pngPath)
        {
            if (!Path.IsPathRooted(pngPath))
                pngPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Assets", pngPath);

            using var fileStream = File.OpenRead(pngPath);
            using var managedStream = new SKManagedStream(fileStream);

            var bitmap = SKBitmap.Decode(managedStream);
            if (bitmap == null)
                throw new InvalidOperationException($"Failed to decode bitmap from file: {pngPath}");

            return bitmap;
        }
    }

    /// <summary>
    /// Represents the result of an image comparison operation, including similarity metrics and visualization data.
    /// </summary>
    public class ImageCompareResult : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the ImageCompareResult class.
        /// </summary>
        /// <param name="similarity">The similarity percentage between 0 and 100.</param>
        /// <param name="heatmap">A bitmap showing differences as a heat map.</param>
        /// <param name="actualResult">The actual result bitmap.</param>
        public ImageCompareResult(float similarity, SKBitmap heatmap, SKBitmap actualResult)
        {
            Similarity = similarity;
            Heatmap = heatmap;
            ActualResult = actualResult;
        }

        /// <summary>
        /// Gets the similarity percentage between the compared images (0-100).
        /// </summary>
        public float Similarity { get; private set; }

        /// <summary>
        /// Gets the heat map bitmap showing pixel differences.
        /// White pixels indicate matches, red pixels indicate differences, yellow indicates size mismatches.
        /// </summary>
        public SKBitmap Heatmap { get; private set; }

        /// <summary>
        /// Gets the actual result bitmap that was compared.
        /// </summary>
        public SKBitmap ActualResult { get; private set; }

        /// <summary>
        /// Releases all resources used by the ImageCompareResult.
        /// </summary>
        public void Dispose()
        {
            Heatmap?.Dispose();
            ActualResult?.Dispose();
        }
    }

    /// <summary>
    /// Extension methods for ImageCompareResult to provide convenient assertion methods.
    /// </summary>
    public static class ImageCompareResultExtensions
    {
        /// <summary>
        /// Asserts that the image comparison result meets the specified similarity threshold.
        /// If the assertion fails, saves debug images to disk for analysis.
        /// </summary>
        /// <param name="res">The image comparison result.</param>
        /// <param name="similarity">The minimum required similarity percentage (0-100).</param>
        /// <param name="svgPath">The path to the original SVG file (for error reporting).</param>
        /// <param name="postFix">Optional postfix for generated debug file names.</param>
        /// <param name="testMethodName">The name of the test method (automatically captured).</param>
        public static void AssertAreSimilar(this ImageCompareResult res,
            float similarity,
            string svgPath,
            string postFix = null,
            [CallerMemberName] string testMethodName = null)
        {
            if (res.Similarity < similarity)
            {
                // Save debug images when assertion fails
                var differenceFileName = $"{testMethodName}{postFix}_difference.png";
                var actualFileName = $"{testMethodName}{postFix}_actual.png";

                SaveBitmapToPng(res.Heatmap, differenceFileName);
                SaveBitmapToPng(res.ActualResult, actualFileName);

                Console.WriteLine($"Saved heatmap in {Path.Combine(Environment.CurrentDirectory, differenceFileName)}");
                Console.WriteLine($"Saved actual result in {Path.Combine(Environment.CurrentDirectory, actualFileName)}");
            }

            res.Similarity.ShouldBeGreaterThanOrEqualTo(similarity);
        }

        /// <summary>
        /// Saves an SKBitmap to a PNG file using SkiaSharp 3.x API.
        /// </summary>
        /// <param name="bitmap">The bitmap to save.</param>
        /// <param name="fileName">The output file name.</param>
        private static void SaveBitmapToPng(SKBitmap bitmap, string fileName)
        {
            using var image = SKImage.FromBitmap(bitmap);
            using var encodedData = image.Encode(SKEncodedImageFormat.Png, 100);
            using var fileStream = File.Create(fileName);
            encodedData.SaveTo(fileStream);
        }
    }
}