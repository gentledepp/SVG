using SkiaSharp;
using Svg.Interfaces;
using PointF = Svg.Interfaces.PointF;

namespace Svg.Platform
{
    public class SkiaRectangleF : RectangleF
    {
        public SkiaRectangleF() : base(0, 0, 0, 0)
        {
            
        }

        public SkiaRectangleF(PointF location, SizeF size) : base(location, size)
        {
        }

        public SkiaRectangleF(float x, float y, float width, float height) : base(x, y, width, height)
        {
        }

        public static implicit operator SKRect(SkiaRectangleF rect)
        {
            return new SKRect(rect.X, rect.Y, rect.X + rect.Width, rect.Y + rect.Height);
        }

        public override string ToString()
        {
            return $"<rect x=\"{X}\" y=\"{Y}\" width=\"{Width}\" height=\"{Height}\" style=\"fill:black\" />";
            //return $"x:{X} y:{Y} width:{Width} height:{Height}";
        }
    }
}
