using SkiaSharp;
using Svg.Editor.Interfaces;
using Svg.Interfaces;
using Svg.Platform;
using PointF = Svg.Interfaces.PointF;

namespace Svg.Editor.Services
{
    public class SKCanvasRenderer : IRenderer
    {
        private readonly SKSurface _surface;
        private readonly int _width;
        private readonly int _height;
        private readonly SKCanvas _canvas;

        public SKCanvasRenderer(SKSurface surface, int width, int height)
        {
            _surface = surface;
            _width = width;
            _height = height;
            _canvas = surface.Canvas;
            Graphics = new SkiaGraphics(_surface);
        }

        public int Width => _width;

        public int Height => _height;

        public void DrawLine(float startX, float startY, float stopX, float stopY, Pen pen)
        {
            _canvas.DrawLine(startX, startY, stopX, stopY, ((SkiaPen) pen).Paint);
        }

        public void Scale(float zoomFactor, float focusX, float focusY)
        {
            // see: https://github.com/android/platform_frameworks_base/blob/master/graphics/java/android/graphics/Canvas.java#L589
            //translate(px, py);
            //scale(sx, sy);
            //translate(-px, -py);
            _canvas.Translate(focusX, focusY);
            _canvas.Scale(zoomFactor, zoomFactor);
            _canvas.Translate(-focusX, -focusY);
        }

        public void Translate(float deltaX, float deltaY)
        {
            _canvas.Translate(deltaX, deltaY);
        }

        public void DrawCircle(float x, float y, float radius, Pen pen)
        {
            //_canvas.DrawCircle(x, y, radius, ((SkiaPen)pen).Paint);
            
            var r = new SKRect(x - radius, y - radius, x + 2*radius, y + 2*radius);

            _canvas.DrawOval(r, ((SkiaPen)pen).Paint);
        }

        public void FillCircle(float x, float y, float radius, Brush brush)
        {
            var r = new SKRect(x - radius, y - radius, x + 2 * radius, y + 2 * radius);

            var skBrush = (SkiaBrushBase) brush;
            var wasStroke = skBrush.Paint.IsStroke;
            try
            {
                skBrush.Paint.IsStroke = false;
                _canvas.DrawOval(r, skBrush.Paint);
            }
            finally
            {
                skBrush.Paint.IsStroke = wasStroke;
            }
        }

        public void DrawRectangle(RectangleF rectangleF, Pen pen)
        {
            _canvas.DrawRect((SkiaRectangleF) rectangleF, ((SkiaPen) pen).Paint);
        }

        public void DrawPath(GraphicsPath path, Pen pen)
        {
            _canvas.DrawPath(((SkiaGraphicsPath)path).Path, ((SkiaPen)pen).Paint);
        }

        public void FillEntireCanvasWithColor(Svg.Interfaces.Color color)
        {
            var c = (SkiaColor) color;
            _canvas.DrawColor(c);
        }

        public void DrawPolygon(PointF[] points, Pen pen)
        {
            for (int i = 1; i < points.Length; i++)
            {
                DrawLine(points[i - 1].X, points[i - 1].Y, points[i].X, points[i].Y, pen);
            }
            DrawLine(points[points.Length - 1].X, points[points.Length - 1].Y, points[0].X, points[0].Y, pen);
        }

        public Graphics Graphics { get; }
    }
}