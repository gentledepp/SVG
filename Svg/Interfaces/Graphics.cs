
using System;
using Svg.Interfaces;

namespace Svg
{
    public interface Graphics : IDisposable
    {
        void CanvasRestore();
        void DrawImage(Bitmap bitmap, RectangleF rectangle, int x, int y, int width, int height, GraphicsUnit pixel);
        void DrawImage(Bitmap bitmap, RectangleF rectangle, int x, int y, int width, int height, GraphicsUnit pixel, ImageAttributes attributes);
        void Flush();
        void Save();
        void Restore();
        TextRenderingHint TextRenderingHint { get; set; }
        float DpiY { get; }
        Region Clip { get; }
        SmoothingMode SmoothingMode { get; set; }
        Matrix Transform { get; set; }
        void DrawImage(Image bitmap, RectangleF destRect, RectangleF srcRect, GraphicsUnit graphicsUnit);
        void DrawImageUnscaled(Image image, PointF location);
        void DrawPath(Pen pen, GraphicsPath path);
        void FillPath(Brush brush, GraphicsPath path);
        void RotateTransform(float fAngle, MatrixOrder order);
        void ScaleTransform(float sx, float sy, MatrixOrder order);
        void DrawImage(Image image, PointF location);
        void SetClip(Region region, CombineMode combineMode);
        void SetClip(GraphicsPath path, CombineMode combineMode);
        void TranslateTransform(float dx, float dy, MatrixOrder order);
        Region[] MeasureCharacterRanges(string text, Font font, RectangleF rectangle, StringFormat format);
        void DrawText(string text, float x, float y, Pen pen);
        void Concat(Matrix matrix);
        void FillBackground(Color color);
    }
}