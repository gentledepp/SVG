using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using PointF = Svg.Interfaces.PointF;
using RectangleF = Svg.Interfaces.RectangleF;
using SizeF = Svg.Interfaces.SizeF;

namespace Svg
{
    public class GdiFontDefn : IFontDefn
    {
        private Font _font;

        public float Size
        {
            get { return _font.Size; }
        }
        public float SizeInPoints
        {
            get { return _font.SizeInPoints; }
        }

        public GdiFontDefn(Font font)
        {
            _font = font;
        }

        public void AddStringToPath(ISvgRenderer renderer, GraphicsPath path, string text, PointF location)
        {
            path.AddString(text, _font.FontFamily, (int)_font.Style, _font.Size, location, SvgEngine.Factory.CreateStringFormatGenericTypographic());
        }

        //Baseline calculation to match http://bobpowell.net/formattingtext.aspx
        public float Ascent(ISvgRenderer renderer)
        {
            var ff = _font.FontFamily;
            float ascent = ff.GetCellAscent(_font.Style);
            float baselineOffset = _font.SizeInPoints / ff.GetEmHeight(_font.Style) * ascent;
            return renderer.DpiY / 72f * baselineOffset;
        }

        public IList<RectangleF> MeasureCharacters(ISvgRenderer renderer, string text)
        {
            var g = GetGraphics(renderer);
            var regions = new List<RectangleF>();
            StringFormat format;
            for (int s = 0; s <= (text.Length - 1) / 32; s++)
            {
                format = SvgEngine.Factory.CreateStringFormatGenericTypographic();
                format.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces;
                format.SetMeasurableCharacterRanges((from r in Enumerable.Range(32 * s, Math.Min(32, text.Length - 32 * s))
                                                     select new CharacterRange(r, 1)).ToArray());
                regions.AddRange(from r in g.MeasureCharacterRanges(text, _font, RectangleF.Create(0, 0, 1000, 1000), format)
                                 select r.GetBounds(g));
            }
            return regions;
        }

        public SizeF MeasureString(ISvgRenderer renderer, string text)
        {
            var g = GetGraphics(renderer);
            var provider = SvgEngine.Factory.GetFontFamilyProvider();
            StringFormat format = provider.GenericTypographic;
            format.SetMeasurableCharacterRanges(new CharacterRange[] { new CharacterRange(0, text.Length) });
            format.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces;
            Region[] r = g.MeasureCharacterRanges(text, _font, RectangleF.Create(0, 0, 1000, 1000), format);
            RectangleF rect = r[0].GetBounds(g);

            return SizeF.Create(rect.Width, Ascent(renderer));
        }

        private static Graphics _graphics;
        private static Graphics GetGraphics(object renderer)
        {
            var provider = renderer as IGraphicsProvider;
            if (provider == null)
            {
                if (_graphics == null)
                {
                    var bmp = Bitmap.Create(1, 1);
                    _graphics = SvgEngine.Factory.CreateGraphicsFromImage(bmp);
                }
                return _graphics;
            }
            else
            {
                return provider.GetGraphics();
            }
        }

        public void Dispose()
        {
            _font.Dispose();
        }
    }
}
