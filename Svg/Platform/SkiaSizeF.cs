using SkiaSharp;
using Svg.Interfaces;

namespace Svg.Platform
{
    public class SkiaSizeF : SizeF
    {
        public SkiaSizeF(PointF pt) : base(pt)
        {
        }

        public SkiaSizeF(SizeF size) : base(size)
        {
        }

        public SkiaSizeF(float width, float height) : base(width, height)
        {
        }

        public static implicit operator SkiaSizeF(SKSize other)
        {
            return new SkiaSizeF(other.Width, other.Height);
        }
        public static implicit operator SKSize(SkiaSizeF other)
        {
            return new SKSize(other.Width, other.Height);
        }

    }
}
