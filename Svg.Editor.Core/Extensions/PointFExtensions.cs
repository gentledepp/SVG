using System.Collections.Generic;
using System.Linq;
using Svg.Interfaces;

namespace Svg.Editor
{
    public static class PointFExtensions
    {
        public static PointF GetFocus(this IEnumerable<PointF> points)
        {
            return PointF.Create(points.Average(p => p.X), points.Average(p => p.Y));
        }
    }
}
