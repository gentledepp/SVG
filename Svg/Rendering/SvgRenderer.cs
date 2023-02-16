using System;
using System.Collections.Generic;
using Svg.Interfaces;

namespace Svg
{
    /// <summary>
    /// Convenience wrapper around a graphics object
    ///
    /// The renderer can be re-used when rendering the same document multiple times.
    /// For performance reasons, graphics objects (brush, pen, fontfamily) are cached per svg-object in the renderer.
    /// This is because the SvgDocument can be loaded and shared by multiple threads in read-only fashion.
    /// </summary>
    public sealed class SvgRenderer : IDisposable, IGraphicsProvider, ISvgRenderer
    {
        private Graphics _innerGraphics;
        private Stack<ISvgBoundable> _boundables = new Stack<ISvgBoundable>();
        private readonly IDictionary<string, object> _context = new Dictionary<string, object>();
        private readonly IDictionary<object, IDisposable> _drawingCache = new Dictionary<object, IDisposable>();

        public void SetBoundable(ISvgBoundable boundable)
        {
            _boundables.Push(boundable);
        }
        public ISvgBoundable GetBoundable()
        {
            return _boundables.Peek();
        }
        public ISvgBoundable PopBoundable()
        {
            return _boundables.Pop();
        }

        public float DpiY
        {
            get { return _innerGraphics.DpiY; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ISvgRenderer"/> class.
        /// </summary>
        private SvgRenderer(Graphics graphics)
        {
            this._innerGraphics = graphics;
        }

        public void DrawImage(Image image, RectangleF destRect, RectangleF srcRect, GraphicsUnit graphicsUnit)
        {
            _innerGraphics.DrawImage(image, destRect, srcRect, graphicsUnit);
        }
        public void DrawImageUnscaled(Image image, PointF location)
        {
            this._innerGraphics.DrawImageUnscaled(image, location);
        }
        public void DrawPath(Pen pen, GraphicsPath path)
        {
            this._innerGraphics.DrawPath(pen, path);
        }
        public void FillPath(Brush brush, GraphicsPath path)
        {
            this._innerGraphics.FillPath(brush, path);
        }
        public Region GetClip()
        {
            return this._innerGraphics.Clip;
        }
        public void RotateTransform(float fAngle, MatrixOrder order = MatrixOrder.Prepend)
        {
            this._innerGraphics.RotateTransform(fAngle, MatrixOrder.Prepend);
        }
        public void ScaleTransform(float sx, float sy, MatrixOrder order = MatrixOrder.Prepend)
        {
            this._innerGraphics.ScaleTransform(sx, sy, MatrixOrder.Prepend);
        }
        public void TranslateTransform(float dx, float dy, MatrixOrder order = MatrixOrder.Prepend)
        {
            this._innerGraphics.TranslateTransform(dx, dy, MatrixOrder.Prepend);
        }
        public void SetClip(Region region, CombineMode combineMode = CombineMode.Replace)
        {
            this._innerGraphics.SetClip(region, combineMode);
        }

        public void FillBackground(Color color)
        {
            if (color == null) throw new ArgumentNullException(nameof(color));
            this._innerGraphics.FillBackground(color);
        }

        public IDictionary<string, object> Context => _context;
        public IDictionary<object, IDisposable> DrawingCache => _drawingCache;
        public IDisposable UsingContextVariable(string key, object variable)
        {
            return new TemporaryContextVariable(key, variable, _context);
        }
        
        public void DrawText(string text, float x, float y, Pen pen)
        {
            this._innerGraphics.DrawText(text, x, y, pen);
        }

        public Graphics Graphics => _innerGraphics;

        public SmoothingMode SmoothingMode
        {
            get { return this._innerGraphics.SmoothingMode; }
            set { this._innerGraphics.SmoothingMode = value; }
        }

        public Matrix Transform
        {
            get { return this._innerGraphics.Transform; }
            set { this._innerGraphics.Transform = value; }
        }

        public void Dispose()
        {
            this._innerGraphics.Dispose();

            foreach(var d in _drawingCache.Values)
                d.Dispose();
            _drawingCache.Clear();
        }
        
        Graphics IGraphicsProvider.GetGraphics()
        {
            return _innerGraphics;
        }

        public ISvgRenderer UseGraphics(Graphics graphics)
        {
            // ensure we do not dispose the existing graphics
            if(_innerGraphics!=graphics)
                _innerGraphics?.Dispose();

            _innerGraphics = graphics;
            return this;
        }

        /// <summary>
        /// Creates a new <see cref="ISvgRenderer"/> from the specified <see cref="Image"/>.
        /// </summary>
        /// <param name="image"><see cref="Image"/> from which to create the new <see cref="ISvgRenderer"/>.</param>
        public static ISvgRenderer FromImage(Image image)
        {
            var g = SvgEngine.Factory.CreateGraphicsFromImage(image);
            g.TextRenderingHint = TextRenderingHint.AntiAlias;
            // TODO LX take over?
            //g.PixelOffsetMode = PixelOffsetMode.Half;
            //g.CompositingQuality = CompositingQuality.HighQuality;
            //g.TextContrast = 1;
            return new SvgRenderer(g);
        }

        /// <summary>
        /// Creates a new <see cref="ISvgRenderer"/> from the specified <see cref="Graphics"/>.
        /// </summary>
        /// <param name="graphics">The <see cref="Graphics"/> to create the renderer from.</param>
        public static ISvgRenderer FromGraphics(Graphics graphics)
        {
            return new SvgRenderer(graphics);
        }

        public static ISvgRenderer FromNull()
        {
            var img = Bitmap.Create(1, 1);
            return SvgRenderer.FromImage(img);
        }

        private class TemporaryContextVariable : IDisposable
        {
            private readonly string _key;
            private readonly object _variable;
            private IDictionary<string, object> _context;

            public TemporaryContextVariable(string key, object variable, IDictionary<string, object> context)
            {
                this._key = key;
                this._variable = variable;
                this._context = context;

                context[key] = variable;
            }


            public void Dispose()
            {
                _context.Remove(_key);
                (this._variable as IDisposable)?.Dispose();
            }
        }
    }
}