using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Svg.Interfaces;
using Svg.Interfaces.Xml;
using Svg.Shared.Interfaces;

namespace Svg
{
    public interface IFactory : IDisposable
    {
        GraphicsPath CreateGraphicsPath();
        GraphicsPath CreateGraphicsPath(FillMode winding);
        Region CreateRegion(RectangleF rect);
        Pen CreatePen(Brush brush, float strokeWidth);
        Matrix CreateMatrix();
        Matrix CreateMatrix(float scaleX, float rotateX, float rotateY, float scaleY, float transX, float transY);
        Bitmap CreateBitmap(Image inputImage);
        Bitmap CreateBitmap(int width, int height);
        Graphics CreateGraphicsFromImage(Bitmap input);
        Graphics CreateGraphicsFromImage(Image image);
        ColorMatrix CreateColorMatrix(float[][] colorMatrixElements);
        ImageAttributes CreateImageAttributes();
        SolidBrush CreateSolidBrush(Color color);
        ColorBlend CreateColorBlend(int colourBlends);
        TextureBrush CreateTextureBrush(Bitmap image);
        LinearGradientBrush CreateLinearGradientBrush(PointF start, PointF end, ColorBlend colorblend, WrapMode tileMode = WrapMode.Tile);
        RadialGradientBrush CreateRadialGradientBrush(PointF center, float radius, ColorBlend colorblend, WrapMode wrapMode = WrapMode.Tile);
        PathGradientBrush CreatePathGradientBrush(GraphicsPath path);
        StringFormat CreateStringFormatGenericTypographic();
        Font CreateFont(FontFamily fontFamily, float fontSize, FontStyle fontStyle, GraphicsUnit graphicsUnit);
        FontFamilyProvider GetFontFamilyProvider();
        Image CreateImageFromStream(Stream stream);
        Bitmap CreateBitmapFromStream(Stream stream);
        RectangleF CreateRectangleF();
        RectangleF CreateRectangleF(PointF location, SizeF size);
        RectangleF CreateRectangleF(float left, float top, float width, float height);

        Colors Colors { get; }
        Color CreateColorFromArgb(int alpha, Color colour);
        Color CreateColorFromArgb(int alpha, int r, int g, int b);
        Color CreateColorFromHexString(string hex);
        PointF CreatePointF(float x, float y);
        SizeF CreateSizeF(float width, float height);

        IXmlTextWriter CreateXmlTextWriter(StringWriter writer);
        IXmlTextWriter CreateXmlTextWriter(Stream stream, Encoding utf8);
        IXmlReader CreateSvgTextReader(Stream stream, Dictionary<string, string> entities);
        IXmlReader CreateSvgTextReader(StringReader reader, Dictionary<string, string> entities);
        ISortedList<TKey, TValue> CreateSortedList<TKey, TValue>();
        IDictionary<TKey, TValue> CreateSortedDictionary<TKey, TValue>();
    }
}