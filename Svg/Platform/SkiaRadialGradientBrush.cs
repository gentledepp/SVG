using System;
using System.Linq;
using SkiaSharp;
using Svg.Interfaces;
using Svg.Shared.Interfaces;

namespace Svg.Platform
{
    public class SkiaRadialGradientBrush : SkiaBrushBase, RadialGradientBrush, IDisposable
    {
        private PointF _center;
        private float _radius;
        private SKShader _shader;
        private WrapMode _wrapMode;
        private ColorBlend _interpolationColors;

        public SkiaRadialGradientBrush(PointF center, float radius, ColorBlend interpolationColors, WrapMode wrapMode = WrapMode.Tile)
        {
            if (center == null) throw new ArgumentNullException(nameof(center));
            if (interpolationColors == null) throw new ArgumentNullException(nameof(interpolationColors));

            _center = center;
            _radius = radius;
            InterpolationColors = interpolationColors;
            WrapMode = wrapMode;
        }
        
        public PointF Center
        {
            get { return _center; }
            set
            {
                _center = value;
                Reset();
            }
        }

        public float Radius
        {
            get { return _radius; }
            set
            {
                _radius = value;
                Reset();
            }
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
            var positions = (InterpolationColors.Positions?.Length >= 0) ? InterpolationColors.Positions : null; // see: https://developer.xamarin.com/api/member/SkiaSharp.SKShader.CreateRadialGradient/p/SkiaSharp.SKPoint/System.Single/SkiaSharp.SKColor[]/System.Single[]/SkiaSharp.SKShaderTileMode/
            _shader = SKShader.CreateRadialGradient(new SKPoint(Center.X, Center.Y), Radius, colors, positions, tileMode);

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
