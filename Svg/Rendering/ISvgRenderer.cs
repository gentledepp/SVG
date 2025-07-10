using System;
using System.Collections.Generic;
using Svg.Interfaces;

namespace Svg
{
    public interface ISvgRenderer : IDisposable
    { 
        float DpiY { get; }
        void DrawImage(Image image, RectangleF destRect, RectangleF srcRect, GraphicsUnit graphicsUnit);
        void DrawImageUnscaled(Image image, PointF location);
        void DrawPath(Pen pen, GraphicsPath path);
        void FillPath(Brush brush, GraphicsPath path);
        ISvgBoundable GetBoundable();
        Region GetClip();
        ISvgBoundable PopBoundable();
        void RotateTransform(float fAngle, MatrixOrder order = MatrixOrder.Append);
        void ScaleTransform(float sx, float sy, MatrixOrder order = MatrixOrder.Append);
        void SetBoundable(ISvgBoundable boundable);
        void SetClip(Region region, CombineMode combineMode = CombineMode.Replace);
        void SetClip(GraphicsPath region, CombineMode combineMode = CombineMode.Replace);
        SmoothingMode SmoothingMode { get; set; }
        Matrix Transform { get; set; }
        void TranslateTransform(float dx, float dy, MatrixOrder order = MatrixOrder.Append);
        void DrawText(string text, float x, float y, Pen pen);
        Graphics Graphics { get; }
        void FillBackground(Color color);
        IDictionary<string, object> Context { get; }
        IDictionary<object, IDisposable> DrawingCache { get; }
        IDisposable UsingContextVariable(string key, object variable);
        ISvgRenderer UseGraphics(Graphics graphics);

        FontFamily GetFontFamily(SvgTextBase text);

    }
}
