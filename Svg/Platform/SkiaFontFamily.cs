using System;
using SkiaSharp;

namespace Svg.Platform
{
    public class SkiaFontFamily : FontFamily
    {
        private readonly SKTypeface _typeface;
        private readonly string _name;

        public SkiaFontFamily(SKTypeface typeface, string name)
        {
            if (typeface == null) throw new ArgumentNullException("typeface");
            if (name == null) throw new ArgumentNullException("name");
            _typeface = typeface;
            _name = name;
        }

        public float GetCellAscent(FontStyle style)
        {
            using (var paint = new SKPaint())
            {
                paint.Typeface = SKTypeface.FromTypeface(_typeface, style.ToSKTypefaceStyle());
                return paint.FontMetrics.Ascent;
            }
        }

        public float GetEmHeight(FontStyle style)
        {
            using (var paint = new SKPaint())
            {
                paint.Typeface = SKTypeface.FromTypeface(_typeface, style.ToSKTypefaceStyle());
                return paint.FontMetrics.Top;
            }
        }

        public bool IsStyleAvailable(FontStyle fontStyle)
        {
            // TODO LX how to implement
            return true;
        }

        public string Name => _name;

        public SKTypeface Typeface => _typeface;

        public void Dispose()
        {
            _typeface?.Dispose();
        }
    }

    public static class TypeFaceExtensions
    {
        public static SKTypefaceStyle ToSKTypefaceStyle(this FontStyle value)
        {
            var tfs = SKTypefaceStyle.Normal;

            if ((value & FontStyle.Bold) == FontStyle.Bold &&
                (value & FontStyle.Italic) == FontStyle.Italic)
                tfs = SKTypefaceStyle.BoldItalic;
            else if ((value & FontStyle.Bold) == FontStyle.Bold)
                tfs = SKTypefaceStyle.Bold;
            else if ((value & FontStyle.Italic) == FontStyle.Italic)
                tfs = SKTypefaceStyle.Italic;
            else if ((value & FontStyle.Regular) == FontStyle.Regular)
                tfs = SKTypefaceStyle.Normal;

            return tfs;
        }
    }
}
