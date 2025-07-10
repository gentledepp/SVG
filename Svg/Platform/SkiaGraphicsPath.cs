using System;
using System.Collections.Generic;
using SkiaSharp;
using Svg.Interfaces;
using PointF = Svg.Interfaces.PointF;

namespace Svg.Platform
{
    public class SkiaGraphicsPath : GraphicsPath
    {
        private FillMode _fillmode;
        private readonly List<PointF> _points = new List<PointF>();
        private readonly List<byte> _pathTypes = new List<byte>();
        private readonly List<TextInfo> _texts = new List<TextInfo>();
        private SKPath _path;
        private RectangleF _bounds;

        public SkiaGraphicsPath()
        {
            _path = new SKPath();
        }


        public SkiaGraphicsPath(SKPath path)
        {
            _path = path;
        }

        public SkiaGraphicsPath(FillMode fillmode)
        {
            FillMode = fillmode;
        }


        public void Dispose()
        {
            if (_path != null)
            {
                _path.Dispose();
                _path = null;
            }
        }

        public RectangleF GetBounds()
        {
            if (_bounds == null)
            {
                var r = new SKRect();
                _path.GetBounds(out r);
                return RectangleF.Create(r.Left, r.Top, r.Right - r.Left, r.Bottom - r.Top);
            }

            return _bounds;
        }

        public void StartFigure()
        {
            _bounds = null;
        }
        public void CloseFigure()
        {
            _bounds = null;
            Path.Close();
        }

        public decimal PointCount { get { return _points.Count; } }
        public PointF[] PathPoints { get { return _points.ToArray(); } }
        public FillMode FillMode
        {
            get { return _fillmode; }
            set
            {
                _fillmode = value;

                switch (_fillmode)
                {
                    case FillMode.Alternate:
                        Path.FillType = SKPathFillType.EvenOdd;
                        break;
                    case FillMode.Winding:
                        Path.FillType = SKPathFillType.Winding;
                        break;
                }
            }
        }

        /// <summary>
        /// see: https://msdn.microsoft.com/en-us/library/system.drawing.drawing2d.graphicspath.pathtypes%28v=vs.110%29.aspx
        /// </summary>
        public byte[] PathTypes
        {
            get
            {
                return _pathTypes.ToArray();
            }
            set
            {
                _pathTypes.Clear();
                _pathTypes.AddRange(value);
            }
        }

        public PathData PathData
        {
            get
            {
                return new PathData(PathPoints, PathTypes);
            }
        }

        public SKPath Path
        {
            get { return _path; }
        }

        internal List<TextInfo> Texts
        {
            get { return _texts; }
        }

        public void AddEllipse(float x, float y, float width, float height)
        {
            _bounds = null;
            // TODO LX: Which direction is correct?
            Path.AddOval(new SKRect(x, y, x + width, y + height), SKPathDirection.Clockwise);

            _points.Add(PointF.Create(x, y));
            _points.Add(PointF.Create(x + width, y + height));
            _pathTypes.Add(0); // start of a figure
            _pathTypes.Add(0x80); // last point in closed sublath
        }

        public void MoveTo(PointF start)
        {
            var lp = GetLastPoint();
            if (lp == null || lp != start)
            {
                _bounds = null;
                Path.MoveTo(start.X, start.Y);
                _points.Add(start);
                _pathTypes.Add(0); // start of a figure ??
            }
        }

        public void AddLine(PointF start, PointF end)
        {
            _bounds = null;
            MoveTo(start);

            Path.LineTo(end.X, end.Y);
            _points.Add(end);
            _pathTypes.Add(1); // end point of line
        }

        public PointF GetLastPoint()
        {
            return _points.Count == 0 ? null : _points[_points.Count - 1];
        }

        public void AddRectangle(RectangleF rectangle)
        {
            _bounds = null;
            Path.AddRect((SkiaRectangleF)rectangle, SKPathDirection.Clockwise);
            _points.Add(PointF.Create(rectangle.Location.X, rectangle.Location.Y));
            _pathTypes.Add(0); // start of a figure
            _points.Add(PointF.Create(rectangle.Location.X + rectangle.Width, rectangle.Location.Y));
            _pathTypes.Add(0x7); // TODO LX: ???
            _points.Add(PointF.Create(rectangle.Location.X, rectangle.Location.Y + rectangle.Height));
            _pathTypes.Add(0x7); // TODO LX: ???
            _points.Add(PointF.Create(rectangle.Location.X + rectangle.Width, rectangle.Location.Y + rectangle.Height));
            _pathTypes.Add(0x80); // TODO LX: ???
        }

        public void AddArc(RectangleF rectangle, float startAngle, float sweepAngle)
        {
            _bounds = null;
            Path.AddArc((SkiaRectangleF)rectangle, startAngle, sweepAngle);

            _points.Add(PointF.Create(rectangle.Location.X, rectangle.Location.Y));
            _pathTypes.Add(1); // start point of line
            _points.Add(PointF.Create(rectangle.Location.X + rectangle.Width, rectangle.Location.Y));
            _pathTypes.Add(0x20); // TODO LX: ???
            _points.Add(PointF.Create(rectangle.Location.X, rectangle.Location.Y + rectangle.Height));
            _pathTypes.Add(0x20); // TODO LX: ???
            _points.Add(PointF.Create(rectangle.Location.X + rectangle.Width, rectangle.Location.Y + rectangle.Height));
            _pathTypes.Add(1); // end point of line
        }

        public GraphicsPath Clone()
        {
            var cl = new SkiaGraphicsPath(new SKPath(this.Path));
            cl._points.AddRange(this._points);
            cl._pathTypes.AddRange(this._pathTypes);
            return cl;
        }

        public void Transform(Matrix transform)
        {
            _bounds = null;
            var m = new SkiaMatrix(transform.Elements);
            Path.Transform(m);
        }

        public void AddPath(GraphicsPath childPath, bool connect)
        {
            _bounds = null;
            var ap = (SkiaGraphicsPath)childPath;
            
            var mode = connect ? SKPathAddMode.Extend : SKPathAddMode.Append;
            Path.AddPath(ap.Path, mode);

            _points.AddRange(ap._points);
            _pathTypes.AddRange(ap._pathTypes);
        }

        public void AddString(string text, FontFamily fontFamily, int style, float size, PointF location,
            StringFormat createStringFormatGenericTypographic)
        {
            _bounds = null;
            // little hack as android path does not support text!
            _texts.Add(new TextInfo(text, fontFamily, style, size, location, createStringFormatGenericTypographic));
        }

        public void AddBezier(PointF start, PointF point1, PointF point2, PointF point3)
        {
            _bounds = null;

            MoveTo(start);
            Path.CubicTo(point1.X, point1.Y, point2.X, point2.Y, point3.X, point3.Y);
            
            _points.AddRange(new[] { start, point1, point2, point3 });
            _pathTypes.Add(1); // start point of line
            _pathTypes.Add(3); // control point of cubic bezier spline
            _pathTypes.Add(3); // control point of cubic bezier spline
            _pathTypes.Add(3); // endpoint of cubic bezier spline
        }

        public void AddBezier(float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4)
        {
            _bounds = null;
            var start = PointF.Create(x1, y1);

            MoveTo(start);
            Path.CubicTo(x2, y2, x3, y3, x4, y4);

            _points.AddRange(new[] { PointF.Create(x1, y1), PointF.Create(x2, y2), PointF.Create(x3, y3), PointF.Create(x4, y4) });
            _pathTypes.Add(1); // start point of line
            _pathTypes.Add(3); // control point of cubic bezier spline
            _pathTypes.Add(3); // control point of cubic bezier spline
            _pathTypes.Add(3); // endpoint of cubic bezier spline
        }

        public void AddQuad(PointF start, PointF control, PointF end)
        {
            _bounds = null;

            MoveTo(start);

            Path.QuadTo(control.X, control.Y, end.X, end.Y);

            _points.Add(start.Clone());
            _pathTypes.Add(1);
            _points.Add(control.Clone());
            _pathTypes.Add(3);
            _points.Add(end.Clone());
            _pathTypes.Add(3);
        }

        public bool IsVisible(PointF pointF)
        {
            return GetBounds().Contains(pointF.X, pointF.Y);
        }

        public void Flatten()
        {
            // TODO LX not supported by Android.Graphics.Path
            throw new NotSupportedException();
        }

        public void AddPolygon(PointF[] polygon)
        {
            _bounds = null;
            for (int i = 0; i < polygon.Length; i++)
            {
                if (i == 0)
                {
                    Path.MoveTo(polygon[i].X, polygon[i].Y);
                    _points.Add(polygon[i]);
                    _pathTypes.Add(0); // start point of figure
                }
                else if (i == polygon.Length - 1)
                {
                    Path.Close();
                    _points.Add(polygon[i]);
                    _pathTypes.Add(0x80); // end point of figure
                }
                else
                {
                    Path.LineTo(polygon[i].X, polygon[i].Y);
                    _points.Add(polygon[i]);
                    _pathTypes.Add(1); // TODO LX: ???
                }
            }
        }

        public void Reset()
        {
            _bounds = null;
            _path = new SKPath();
        }
        
        internal class TextInfo
        {
            public string text;
            public FontFamily fontFamily;
            public int style;
            public float size;
            public PointF location;
            StringFormat createStringFormatGenericTypographic;

            public TextInfo(string text, FontFamily fontFamily, int style, float size, PointF location, StringFormat createStringFormatGenericTypographic)
            {
                this.text = text;
                this.fontFamily = fontFamily;
                this.style = style;
                this.size = size;
                this.location = location;
                this.createStringFormatGenericTypographic = createStringFormatGenericTypographic;
            }
        }
    }
}