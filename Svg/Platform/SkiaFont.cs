using SkiaSharp;

namespace Svg.Platform
{
    public class SkiaFont : Font
    {
        private FontStyle _style;
        private SkiaFontFamily _fontFamily;
        private SKPaint _paint;

        public SkiaFont(SkiaFontFamily fontFamily)
        {
            _fontFamily = fontFamily;
            _paint = new SKPaint();
            _paint.Typeface =  _fontFamily.Typeface;
        }

        public void Dispose()
        {
            if (_paint != null)
            {
                _paint.Dispose();
                _paint = null;
            }
        }

        public float Size
        {
            get { return _paint.TextSize; }
            set { _paint.TextSize = value; }
        }

        public float SizeInPoints
        {
            get { return _paint.TextSize; }
            set { _paint.TextSize = value; }
        }

        public FontStyle Style
        {
            get { return _style; }
            set
            {
                _style = value;
                _paint.Typeface = SKTypeface.FromFamilyName(_fontFamily.Name, value.ToSKFontStyle());
            }
        }

        public FontFamily FontFamily { get { return _fontFamily; } }
    }
}
