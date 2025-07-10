using System.Globalization;
using Svg.Interfaces;

namespace Svg
{
    public static class PointFExtensions
    {
        public static string ToSvgString(this PointF p)
        {
            return $"{p.X.ToString(CultureInfo.InvariantCulture)} {p.Y.ToString(CultureInfo.InvariantCulture)}";
        }
    }
}