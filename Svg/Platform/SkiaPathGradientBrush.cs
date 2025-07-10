using System;
using System.Linq;
using SkiaSharp;
using Svg.Interfaces;

namespace Svg.Platform
{
    public class SkiaPathGradientBrush : SkiaBrushBase, PathGradientBrush, IDisposable
    {
        private PointF _centerPoint;
        private ColorBlend _interpolationColors;
        private SKShader _shader;
        private readonly GraphicsPath _path;
        private float _radius;

        public SkiaPathGradientBrush(GraphicsPath path)
        {
            _path = path ?? throw new ArgumentNullException(nameof(path));
            
            // Calculate center point from path bounds if not set
            var bounds = path.GetBounds();
            _centerPoint = PointF.Create(
                bounds.Left + bounds.Width / 2,
                bounds.Top + bounds.Height / 2
            );
            
            // Calculate radius as half the diagonal of the bounding box
            _radius = (float)Math.Sqrt(bounds.Width * bounds.Width + bounds.Height * bounds.Height) / 2;
        }

        public PointF CenterPoint
        {
            get { return _centerPoint; }
            set
            {
                _centerPoint = value;
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

        protected override SKPaint CreatePaint()
        {
            var paint = new SKPaint();
            
            if (InterpolationColors != null && InterpolationColors.Colors.Length > 0)
            {
                if(_shader != null) _shader.Dispose();

                var colors = InterpolationColors.Colors.Select(c => new SKColor(c.R, c.G, c.B, c.A)).ToArray();
                var positions = (InterpolationColors.Positions?.Length > 0) ? InterpolationColors.Positions : null;
                
                // Create a radial gradient from center point to the calculated radius
                _shader = SKShader.CreateRadialGradient(
                    new SKPoint(CenterPoint.X, CenterPoint.Y), 
                    _radius, 
                    colors, 
                    positions, 
                    SKShaderTileMode.Clamp
                );

                paint.Shader = _shader;
            }
            
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