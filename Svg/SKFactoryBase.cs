using SkiaSharp;
using Svg.Interfaces;
using Svg.Interfaces.Xml;
using Svg.Platform;
using Svg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Svg
{
    public abstract class SKFactoryBase : IFactory
    {
        private static readonly Lazy<Colors> _colors = new Lazy<Colors>(() => new SkiaColors());
        public static readonly Regex Base64UrlRegex = new Regex(
            "^(url\\(['\"]?data:[a-zA-Z/-]+;base64,)(?<source>(?:[A-Za-z0-9+/]{4})*(?:[A-Za-z0-9+/]{2}==|[A-Za-z0-9+/]{3}=)?)['\"]?\\)",
            RegexOptions.Compiled);
        private const string SvgDefaultFontFamily = "Segoe UI";

        public virtual GraphicsPath CreateGraphicsPath()
        {
            return new SkiaGraphicsPath();
        }

        public virtual GraphicsPath CreateGraphicsPath(FillMode winding)
        {
            return new SkiaGraphicsPath(winding);
        }

        public virtual Region CreateRegion(RectangleF rect)
        {
            return new Region(rect);
        }

        public virtual Pen CreatePen(Brush brush, float strokeWidth)
        {
            return new SkiaPen(brush, strokeWidth);
        }

        public virtual Matrix CreateMatrix()
        {
            return new SkiaMatrix();
        }

        public virtual Matrix CreateMatrix(float scaleX, float rotateX, float rotateY, float scaleY, float transX, float transY)
        {
            return new SkiaMatrix(scaleX, rotateX, rotateY, scaleY, transX, transY);
        }

        public virtual Bitmap CreateBitmap(Image inputImage)
        {
            if (inputImage == null) throw new ArgumentNullException(nameof(inputImage));
            return new SkiaBitmap(inputImage);
        }

        public virtual Bitmap CreateBitmap(int width, int height)
        {
            return new SkiaBitmap(width, height);
        }

        public virtual Graphics CreateGraphicsFromImage(Bitmap input)
        {
            return new SkiaGraphics((SkiaBitmap)input);
        }

        public virtual Graphics CreateGraphicsFromImage(Image image)
        {
            return new SkiaGraphics((SkiaBitmap)image);
        }

        public virtual ColorMatrix CreateColorMatrix(float[][] colorMatrixElements)
        {
            return new SkiaColorMatrix(colorMatrixElements);
        }

        public virtual ImageAttributes CreateImageAttributes()
        {
            throw new NotImplementedException();
        }
        
        public virtual SolidBrush CreateSolidBrush(Color color)
        {
            return new SkiaSolidBrush(color);
        }

        public virtual ColorBlend CreateColorBlend(int colourBlends)
        {
            return new ColorBlend(colourBlends);
        }

        public virtual TextureBrush CreateTextureBrush(Bitmap image)
        {
            return new SkiaTextureBrush((SkiaBitmap)image);
        }

        public virtual LinearGradientBrush CreateLinearGradientBrush(PointF start, PointF end, ColorBlend colorblend, WrapMode tileMode = WrapMode.Tile)
        {
            return new SkiaLinearGradientBrush(start, end, colorblend, tileMode);
        }

        public virtual RadialGradientBrush CreateRadialGradientBrush(PointF center, float radius, ColorBlend colorblend, WrapMode wrapMode = WrapMode.Tile)
        {
            return new SkiaRadialGradientBrush(center, radius, colorblend, wrapMode);
        }

        public virtual PathGradientBrush CreatePathGradientBrush(GraphicsPath path)
        {
            throw new NotImplementedException("pathgradientbrushs are not supported");
        }

        public virtual StringFormat CreateStringFormatGenericTypographic()
        {
            return new SkiaStringFormat();
        }

        public virtual Font CreateFont(FontFamily fontFamily, float fontSize, FontStyle fontStyle, GraphicsUnit graphicsUnit)
        {
            var font = new SkiaFont((SkiaFontFamily)fontFamily);
            font.Size = fontSize;
            font.Style = fontStyle;
            // TODO LX: what to use graphicsUnit for?

            return font;
        }

        public FontFamily GetFontFamily(string fontFamilyName, SvgFontWeight fontWeight, SvgFontStyle fontStyle,
            SvgDocument textOwnerDocument)
        {
            var typeFace = SKTypeface.FromFamilyName(fontFamilyName, fontWeight.ToSkFontStyleWeight(), SKFontStyleWidth.Normal, fontStyle.ToSkFontStyleSlant());

            if (typeFace is null)
                return null;
            
            return new SkiaFontFamily(typeFace, fontFamilyName??typeFace.FamilyName);
        }

        public FontFamily LoadCustomFontFamily(string fontFamilyName, SvgFontWeight fontWeight, SvgFontStyle fontStyle, SvgDocument doc)
        {
            if (string.IsNullOrEmpty(fontFamilyName))
                throw new ArgumentNullException(nameof(fontFamilyName));
            
            var customFont = doc.StyleSheets.SelectMany(s => s.FontFaceDirectives).FirstOrDefault(ff => ff.FontFamily == fontFamilyName);
            if (customFont?.Src is { } src)
            {
                var m = Base64UrlRegex.Match(src);
                if (m.Success)
                {
                    var base64 = m.Groups["source"].Value;
                    using var ms = new MemoryStream(Convert.FromBase64String(base64));
                    ms.Seek(0, SeekOrigin.Begin);
                    var tf = SKTypeface.FromStream(ms);
                    if (tf is null)
                        return null;
                    return new SkiaFontFamily(tf, fontFamilyName);
                }
            }

            var typeFace = SKTypeface.FromFamilyName(fontFamilyName, fontWeight.ToSkFontStyleWeight(), SKFontStyleWidth.Normal, fontStyle.ToSkFontStyleSlant());
            if (typeFace is null)
                return null;
            
            return new SkiaFontFamily(typeFace, fontFamilyName);
        }
        

        public virtual FontFamilyProvider GetFontFamilyProvider()
        {
            return new SkiaFontFamilyProvider();
        }

        public virtual RectangleF CreateRectangleF()
        {
            return new SkiaRectangleF();
        }

        public virtual RectangleF CreateRectangleF(PointF location, SizeF size)
        {
            return new SkiaRectangleF(location, size);
        }

        public virtual RectangleF CreateRectangleF(float left, float top, float width, float height)
        {
            return new SkiaRectangleF(left, top, width, height);
        }

        public virtual Colors Colors => _colors.Value;
        public virtual Color CreateColorFromArgb(int alpha, Color colour)
        {
            return new SkiaColor((byte)alpha, colour);
        }

        public virtual Color CreateColorFromArgb(int alpha, int r, int g, int b)
        {
            return new SkiaColor((byte)alpha, (byte)r, (byte)g, (byte)b);
        }

        public virtual Color CreateColorFromHexString(string hex)
        {
            if (hex == null) throw new ArgumentException("Hex string cannot be null.", nameof(hex));

            if (Regex.IsMatch(hex.ToLowerInvariant(), @"^#[a-f0-9]{8}$"))
            {
                //RGBA - so the last two digits are the alpha
                var r = int.Parse(hex.Substring(1, 2), NumberStyles.HexNumber);
                var g = int.Parse(hex.Substring(3, 2), NumberStyles.HexNumber);
                var b = int.Parse(hex.Substring(5, 2), NumberStyles.HexNumber);
                var a = int.Parse(hex.Substring(7, 2), NumberStyles.HexNumber);

                return CreateColorFromArgb(a, r, g, b);
            }

            if (Regex.IsMatch(hex.ToLowerInvariant(), @"^#[a-f0-9]{6}$"))
            {
                var r = int.Parse(hex.Substring(1, 2), NumberStyles.HexNumber);
                var g = int.Parse(hex.Substring(3, 2), NumberStyles.HexNumber);
                var b = int.Parse(hex.Substring(5, 2), NumberStyles.HexNumber);

                return CreateColorFromArgb(255, r, g, b);
            }

            if (Regex.IsMatch(hex.ToLowerInvariant(), @"^#[a-f0-9]{4}$"))
            {
                //RGBA - so the last digit is the alpha
                var c = string.Format("#{0}{0}{1}{1}{2}{2}{3}{3}",hex[1], hex[2], hex[3], hex[4]);
                var r= int.Parse(c.Substring(1, 2), NumberStyles.HexNumber);
                var g = int.Parse(c.Substring(3, 2), NumberStyles.HexNumber);
                var b = int.Parse(c.Substring(5, 2), NumberStyles.HexNumber);
                var a = int.Parse(c.Substring(7, 2), NumberStyles.HexNumber);

                return CreateColorFromArgb(a, r, g, b);
            }


            if (Regex.IsMatch(hex.ToLowerInvariant(), @"^#[a-f0-9]{3}$"))
            {
                var c = string.Format("#{0}{0}{1}{1}{2}{2}", hex[1], hex[2], hex[3]);
                var r = int.Parse(c.Substring(1, 2), NumberStyles.HexNumber);
                var g = int.Parse(c.Substring(3, 2), NumberStyles.HexNumber);
                var b = int.Parse(c.Substring(5, 2), NumberStyles.HexNumber);

                return CreateColorFromArgb(255, r, g, b);
            }

            throw new ArgumentException("Not a valid hex string.", nameof(hex));

        }

        public virtual PointF CreatePointF(float x, float y)
        {
            return new SkiaPointF(x, y);
        }

        public virtual SizeF CreateSizeF(float width, float height)
        {
            return new SkiaSizeF(width, height);
        }

        public virtual IDictionary<TKey, TValue> CreateSortedDictionary<TKey, TValue>()
        {
            return new SortedDictionary<TKey, TValue>();
        }

        public virtual Bitmap CreateBitmapFromStream(Stream stream)
        {
            using (var s = new SKManagedStream(stream))
            {
                var bm = SKBitmap.Decode(s);
                return new SkiaBitmap(bm);
            }
        }

        public virtual Image CreateImageFromStream(Stream stream)
        {
            using (var s = new SKManagedStream(stream))
            {
                var bm = SKBitmap.Decode(s);
                return new SkiaBitmap(bm);
            }
        }

        public abstract ISortedList<TKey, TValue> CreateSortedList<TKey, TValue>();

        public abstract IXmlTextWriter CreateXmlTextWriter(StringWriter writer);

        public abstract IXmlTextWriter CreateXmlTextWriter(Stream stream, Encoding utf8);
        public abstract IXmlReader CreateSvgTextReader(Stream stream, Dictionary<string, string> entities);

        public abstract IXmlReader CreateSvgTextReader(StringReader r, Dictionary<string, string> entities);

        public void Dispose()
        {
           
        }
    }
}
