using System;
using SkiaSharp;

namespace Svg.Platform
{
    /// <summary>
    /// Represents a font family implementation using SkiaSharp for cross-platform font handling.
    /// Provides methods to retrieve font metrics and style information for SVG rendering.
    /// </summary>
    public class SkiaFontFamily : FontFamily
    {
        private readonly SKTypeface _typeface;
        private readonly string _name;

        /// <summary>
        /// Initializes a new instance of the SkiaFontFamily class.
        /// </summary>
        /// <param name="typeface">The SkiaSharp typeface to wrap.</param>
        /// <param name="name">The name of the font family.</param>
        /// <exception cref="ArgumentNullException">Thrown when typeface or name is null.</exception>
        public SkiaFontFamily(SKTypeface typeface, string name)
        {
            _typeface = typeface ?? throw new ArgumentNullException(nameof(typeface));
            _name = name ?? throw new ArgumentNullException(nameof(name));
        }

        /// <summary>
        /// Gets the cell ascent value for the specified font style.
        /// The ascent is the maximum distance above the baseline for any character in the font.
        /// </summary>
        /// <param name="style">The font style to get the ascent for.</param>
        /// <returns>The ascent value as a float.</returns>
        public float GetCellAscent(FontStyle style)
        {
            using var font = new SKFont();
            font.Typeface = GetTypefaceForStyle(style);
            return -font.Metrics.Ascent; // Negative because SkiaSharp ascent is negative
        }

        /// <summary>
        /// Gets the em height (line height) for the specified font style.
        /// This represents the total height of the font including ascenders and descenders.
        /// </summary>
        /// <param name="style">The font style to get the em height for.</param>
        /// <returns>The em height value as a float.</returns>
        public float GetEmHeight(FontStyle style)
        {
            using var font = new SKFont();
            font.Typeface = GetTypefaceForStyle(style);
            var metrics = font.Metrics;
            return metrics.Descent - metrics.Ascent; // Total height from top to bottom
        }

        /// <summary>
        /// Determines whether the specified font style is available for this font family.
        /// </summary>
        /// <param name="fontStyle">The font style to check availability for.</param>
        /// <returns>True if the style is available; otherwise, false.</returns>
        public bool IsStyleAvailable(FontStyle fontStyle)
        {
            try
            {
                var styledTypeface = GetTypefaceForStyle(fontStyle);
                return styledTypeface != null;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the name of the font family.
        /// </summary>
        public string Name => _name;

        /// <summary>
        /// Gets the underlying SkiaSharp typeface.
        /// </summary>
        public SKTypeface Typeface => _typeface;

        /// <summary>
        /// Gets or creates a typeface for the specified font style.
        /// In SkiaSharp 3.x, we create a new typeface from the family name with the desired style.
        /// </summary>
        /// <param name="style">The font style to apply.</param>
        /// <returns>An SKTypeface with the specified style applied.</returns>
        private SKTypeface GetTypefaceForStyle(FontStyle style)
        {
            var fontStyle = style.ToSKFontStyle();

            // In SkiaSharp 3.x, we create a new typeface from the family name with the desired style
            var styledTypeface = SKTypeface.FromFamilyName(_typeface.FamilyName, fontStyle);

            // If we can't create a styled version, fall back to the original typeface
            return styledTypeface ?? _typeface;
        }

        /// <summary>
        /// Releases all resources used by the SkiaFontFamily.
        /// </summary>
        public void Dispose()
        {
            _typeface?.Dispose();
        }
    }

    /// <summary>
    /// Provides extension methods for converting between FontStyle and SkiaSharp font style types.
    /// </summary>
    public static class TypeFaceExtensions
    {
        /// <summary>
        /// Converts a FontStyle enum value to the corresponding SKFontStyle.
        /// SkiaSharp 3.x uses SKFontStyle for font styling.
        /// </summary>
        /// <param name="value">The FontStyle value to convert.</param>
        /// <returns>The corresponding SKFontStyle.</returns>
        public static SKFontStyle ToSKFontStyle(this FontStyle value)
        {
            var weight = (value & FontStyle.Bold) == FontStyle.Bold
                ? SKFontStyleWeight.Bold
                : SKFontStyleWeight.Normal;

            var slant = (value & FontStyle.Italic) == FontStyle.Italic
                ? SKFontStyleSlant.Italic
                : SKFontStyleSlant.Upright;

            return new SKFontStyle(weight, SKFontStyleWidth.Normal, slant);
        }
    }
}