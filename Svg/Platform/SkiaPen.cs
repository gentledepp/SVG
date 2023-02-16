using System;
using SkiaSharp;

namespace Svg.Platform
{
    public class SkiaPen : Pen
    {
        private readonly SkiaBrushBase _brush;
        private SKPathEffect _dashes;
        private float[] _dashPattern;
        private LineJoin _lineJoin;
        private float _miterLimit;
        private LineCap _cap;
        private float _dashOffset;

        private event EventHandler DashesChanged;

        public SkiaPen(Brush brush, float strokeWidth)
        {
            _brush = (SkiaBrushBase)brush;

            _brush.Paint.StrokeWidth = strokeWidth;
            _brush.Paint.IsStroke = true;
            _brush.Paint.TextSize = 14;

            DashesChanged += OnDashesChanged;
        }

        public override void Dispose()
        {
            _brush?.Dispose();
            _dashes?.Dispose();
        }

        public override float[] DashPattern
        {
            get { return _dashPattern; }
            set
            {
                _dashPattern = value;
                if (_dashPattern == null && _dashes != null)
                {
                    _dashes.Dispose();
                    _dashes = null;
                }

                DashesChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public override float DashOffset
        {
            get { return _dashOffset; }
            set
            {
                _dashOffset = value;

                DashesChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private void OnDashesChanged(object sender, EventArgs eventArgs)
        {
            if (_dashPattern == null) return;

            _dashes?.Dispose();

            _dashes = SKPathEffect.CreateDash(_dashPattern, _dashOffset);
            _brush.Paint.PathEffect = _dashes;
        }

        public override LineJoin LineJoin
        {
            get { return _lineJoin; }
            set
            {
                _lineJoin = value;

                switch (value)
                {
                    case LineJoin.Bevel:
                        _brush.Paint.StrokeJoin = SKStrokeJoin.Bevel;
                        break;
                    case LineJoin.Miter:
                        _brush.Paint.StrokeJoin = SKStrokeJoin.Miter;
                        break;
                    case LineJoin.MiterClipped:
                        _brush.Paint.StrokeJoin = SKStrokeJoin.Miter;
                        break;
                    case LineJoin.Round:
                        _brush.Paint.StrokeJoin = SKStrokeJoin.Round;
                        break;
                }
            }
        }

        public override float MiterLimit
        {
            get { return _miterLimit; }
            set
            {
                _miterLimit = value;
                _brush.Paint.StrokeMiter = MiterLimit;
            }
        }

        public override LineCap StartCap
        {
            get { return _cap; }
            set
            {
                _cap = value;
                
                SKStrokeCap cap = SKStrokeCap.Butt;

                switch (value)
                {
                    case LineCap.AnchorMask:
                    case LineCap.ArrowAnchor:
                    case LineCap.Custom:
                    case LineCap.NoAnchor:
                    case LineCap.DiamondAnchor:
                    case LineCap.Triangle:
                    case LineCap.Flat:
                        cap = SKStrokeCap.Butt;
                        break;
                    case LineCap.Round:
                    case LineCap.RoundAnchor:
                        cap = SKStrokeCap.Round;
                        break;
                    case LineCap.Square:
                    case LineCap.SquareAnchor:
                        cap = SKStrokeCap.Square;
                        break;
                        
                }
                _brush.Paint.StrokeCap = cap;
            }
        }

        public override LineCap EndCap
        {
            get { return StartCap; }
            set { StartCap = value; }
        }

        public SKPaint Paint
        {
            get { return _brush.Paint; }
        }

        public float TextSize
        {
            get { return _brush.Paint.TextSize; }
            set { _brush.Paint.TextSize = value; }
        }

        public override float StrokeWidth
        {
            get { return _brush.Paint.StrokeWidth; }
            set { _brush.Paint.StrokeWidth = value; }
        }

        public SKTextAlign TextAlign
        {
            get { return _brush.Paint.TextAlign; }
            set { _brush.Paint.TextAlign = value; }
        }
        
    }
}
