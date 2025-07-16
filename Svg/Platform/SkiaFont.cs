using SkiaSharp;

namespace Svg.Platform
{
    public class SkiaFont : Font
    {
        private FontStyle _style;
        private SkiaFontFamily _fontFamily;
        private SKFont _font;

        public SkiaFont(SkiaFontFamily fontFamily)
        {
            _fontFamily = fontFamily;
            _font = new SKFont();
            _font.Typeface = _fontFamily.Typeface;
        }

        public void Dispose()
        {
            if (_font != null)
            {
                _font.Dispose();
                _font = null;
            }
        }

        public float Size
        {
            get { return _font.Size; }
            set { _font.Size = value; }
        }

        public float SizeInPoints
        {
            get { return _font.Size; }
            set { _font.Size = value; }
        }

        public FontStyle Style
        {
            get { return _style; }
            set
            {
                _style = value;
                _font.Typeface = SKTypeface.FromFamilyName(_fontFamily.Name, value.ToSKFontStyle());
            }
        }

        public FontFamily FontFamily { get { return _fontFamily; } }

        public SKFont Font { get { return _font; } }
    }
}
