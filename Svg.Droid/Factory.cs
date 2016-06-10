using System;
using System.Drawing.Drawing2D;
using System.IO;
using Android.Graphics;
using Svg.Droid;
using Svg.Platform;
using Color = Svg.Interfaces.Color;
using PointF = Svg.Interfaces.PointF;
using RectangleF = Svg.Interfaces.RectangleF;

namespace Svg
{
    public class Factory : IFactory
    {
        public static IFactory Instance = new Factory();

        public GraphicsPath CreateGraphicsPath()
        {
            return new AndroidGraphicsPath();
        }

        public GraphicsPath CreateGraphicsPath(FillMode fillmode)
        {
            return new AndroidGraphicsPath(fillmode);
        }

        public Region CreateRegion(RectangleF rect)
        {
            return new Region(rect);
        }

        public Pen CreatePen(Brush brush, float strokeWidth)
        {
            return new AndroidPen(brush, strokeWidth);
        }

        public Matrix CreateMatrix()
        {
            return new AndroidMatrix();
        }

        public Matrix CreateMatrix(float i, float i1, float i2, float i3, float i4, float i5)
        {
            return new AndroidMatrix(i, i1, i2, i3, i4, i5);
        }

        public Bitmap CreateBitmap(Image inputImage)
        {
            return new AndroidBitmap(inputImage);
        }

        public Bitmap CreateBitmap(int width, int height)
        {
            return new AndroidBitmap(width, height);
        }

        public Graphics CreateGraphicsFromImage(Bitmap input)
        {
            var bitmap = (AndroidBitmap)input;
            return new AndroidGraphics(bitmap);
        }

        public Graphics CreateGraphicsFromImage(Image image)
        {
            var bitmap = (AndroidBitmap) image;
            return new AndroidGraphics(bitmap);
        }

        public ColorMatrix CreateColorMatrix(float[][] colorMatrixElements)
        {
            return new AndroidColorMatrix(colorMatrixElements);
        }

        public ImageAttributes CreateImageAttributes()
        {
            throw new System.NotImplementedException();
        }

        public SolidBrush CreateSolidBrush(Color color)
        {
            return new AndroidSolidBrush((AndroidColor)color);
        }

        public ColorBlend CreateColorBlend(int colourBlends)
        {
            return new ColorBlend(colourBlends);
        }

        public TextureBrush CreateTextureBrush(Bitmap image)
        {
            return new AndroidTextureBrush((AndroidBitmap) image);
        }

        public LinearGradientBrush CreateLinearGradientBrush(PointF start, PointF end, Color startColor, Color endColor)
        {
            return new AndroidLinearGradientBrush((AndroidPointF)start, (AndroidPointF)end, (AndroidColor)startColor, (AndroidColor)endColor);
        }

        public PathGradientBrush CreatePathGradientBrush(GraphicsPath path)
        {
            throw new System.NotImplementedException();
        }

        public StringFormat CreateStringFormatGenericTypographic()
        {
            throw new NotImplementedException();
        }

        public Font CreateFont(FontFamily fontFamily, float fontSize, FontStyle fontStyle, GraphicsUnit graphicsUnit)
        {
            var font = new AndroidFont((AndroidFontFamily) fontFamily);
            font.Size = fontSize;
            font.Style = fontStyle;
            // TODO LX: what to use graphicsUnit for?

            return font;
        }

        public FontFamilyProvider GetFontFamilyProvider()
        {
            return new AndroidFontFamilyProvider();
        }

        public Image CreateImageFromStream(Stream stream)
        {
            var bitmap = BitmapFactory.DecodeStream(stream);
            return new AndroidBitmap(bitmap);
        }

        public Bitmap CreateBitmapFromStream(Stream stream)
        {
            var bitmap = BitmapFactory.DecodeStream(stream);
            return new AndroidBitmap(bitmap);
        }

        public RectangleF CreateRectangleF(float left, float top, float width, float height)
        {
            return new AndroidRectangleF(left, top, width, height);
        }

        public RectangleF CreateRectangleF()
        {
            return new AndroidRectangleF();
        }

        public Color CreateColorFromArgb(int alpha, Color color)
        {
            return new AndroidColor(alpha, color);
        }

        public PointF CreatePointF(float x, float y)
        {
            return new AndroidPointF(x, y);
        }
    }
}