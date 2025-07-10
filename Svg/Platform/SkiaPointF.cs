using SkiaSharp;
using Svg.Interfaces;

namespace Svg.Platform
{
    public class SkiaPointF : PointF
    {
        public SkiaPointF(float x, float y) : base(x, y)
        {
        }

        public static implicit operator SkiaPointF(SKPoint other)
        {
            return new SkiaPointF(other.X, other.Y);
        }
        public static implicit operator SKPoint(SkiaPointF other)
        {
            return new SKPoint(other.X, other.Y);
        }

    }
}
