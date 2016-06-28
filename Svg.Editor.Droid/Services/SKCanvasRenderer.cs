using Android.Graphics;
using SkiaSharp;
using Svg.Core.Interfaces;
using Svg.Interfaces;
using Svg.Platform;

namespace Svg.Droid.Editor.Services
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
            _canvas.Scale(zoomFactor, zoomFactor/*, focusX, focusY*/);
        }

        public void Translate(float deltaX, float deltaY)
        {
            _canvas.Translate(deltaX, deltaY);
        }

        public void DrawCircle(float x, float y, int radius, Pen pen)
        {
            //_canvas.DrawCircle(x, y, radius, ((SkiaPen)pen).Paint);
            
            var r = new SKRect(x - radius, y - radius, x + 2*radius, y + 2*radius);

            _canvas.DrawOval(r, ((SkiaPen)pen).Paint);
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

        public Graphics Graphics { get; }
    }
}