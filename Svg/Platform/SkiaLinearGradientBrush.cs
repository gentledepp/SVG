using System;
using System.Linq;
using SkiaSharp;
using Svg.Interfaces;

namespace Svg.Platform
{
    public class SkiaLinearGradientBrush : SkiaBrushBase, LinearGradientBrush, IDisposable
    {
        private readonly Color[] _colors;
        private readonly float[] _colorPositions;
        private PointF _start;
        private PointF _end;
        private Color _colorStart;
        private Color _colorEnd;
        private SKShader _shader;
        private ColorBlend _interpolationColors;
        private WrapMode _wrapMode;

        public SkiaLinearGradientBrush(PointF start, PointF end, ColorBlend interpolationColors, WrapMode wrapMode = WrapMode.Tile)
        {
            if (start == null) throw new ArgumentNullException(nameof(start));
            if (end == null) throw new ArgumentNullException(nameof(end));
            if (interpolationColors == null) throw new ArgumentNullException(nameof(interpolationColors));

            WrapMode = wrapMode;
            InterpolationColors = interpolationColors;
            Start = start;
            End = end;
        }

        public ColorBlend InterpolationColors
        {
            get { return _interpolationColors; }
            set
            {
                _interpolationColors = value;
                Reset();
            }
        }

        public WrapMode WrapMode
        {
            get { return _wrapMode; }
            set
            {
                _wrapMode = value;
                Reset();
            }
        }

        public PointF Start
        {
            get { return _start; }
            set
            {
                _start = value;
                Reset();
            }
        }

        public PointF End
        {
            get { return _end; }
            set
            {
                _end = value;
                Reset();
            }
        }

        protected override SKPaint CreatePaint()
        {
            var paint = new SKPaint();
            SKShaderTileMode tileMode = SKShaderTileMode.Clamp;
            switch (WrapMode)
            {
                case WrapMode.Clamp:
                    tileMode = SKShaderTileMode.Clamp;
                    break;
                case WrapMode.Tile:
                    tileMode = SKShaderTileMode.Repeat;
                    break;
                case WrapMode.TileFlipX:
                case WrapMode.TileFlipXY:
                case WrapMode.TileFlipY:
                    tileMode = SKShaderTileMode.Mirror;
                    break;
            }

            if(_shader != null)_shader.Dispose();

            var colors = InterpolationColors.Colors.Select(c => new SKColor(c.R, c.G, c.B, c.A)).ToArray();
            var positions = (InterpolationColors.Positions?.Length >= 0) ? InterpolationColors.Positions : null;  // see: https://developer.xamarin.com/api/member/SkiaSharp.SKShader.CreateLinearGradient/p/SkiaSharp.SKPoint/SkiaSharp.SKPoint/SkiaSharp.SKColor[]/System.Single[]/SkiaSharp.SKShaderTileMode/
            _shader = SKShader.CreateLinearGradient(new SKPoint(Start.X, Start.Y), new SKPoint(End.X, End.Y), colors, positions, tileMode);
            
            paint.Shader = _shader;
            paint.IsAntialias = true;
            return paint;
        }

        public override void Dispose()
        {
            base.Dispose();
            _shader?.Dispose();
            _shader = null;
        }
    }
}
