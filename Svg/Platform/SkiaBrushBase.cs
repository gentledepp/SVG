using System;
using SkiaSharp;

namespace Svg.Platform
{
    public abstract class SkiaBrushBase : IDisposable
    {
        private SKPaint _paint;
        private SKFont _font;

        public SKPaint Paint
        {
            get
            {
                if (_paint == null)
                {
                    _paint = CreatePaint();
                    _paint.IsStroke = false;
                }
                return _paint;
            }
        }

        public SKFont Font
        {
            get
            {
                if (_font == null)
                    _font = new SKFont();
                return _font;
            }
        }

        protected abstract SKPaint CreatePaint();

        public void SetFontFamily(FontFamily fontFamily)
        {
            if (fontFamily is SkiaFontFamily ffm && ffm.Typeface is { } typeface)
            {
                if (_font == null)
                    _font = new SKFont();
                _font.Typeface = typeface;
            }
        }

        protected void Reset()
        {
            _paint?.Dispose();
            _paint = null;
            _font?.Dispose();
            _font = null;
        }

        public virtual void Dispose()
        {
            Reset();
        }
    }
}
