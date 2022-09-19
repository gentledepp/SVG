using System.Collections.Generic;
using System.Linq;
using Svg.Interfaces;
using Svg.Pathing;

namespace Svg
{
    public static class SvgPathSegmentListExtension
    {
        public static List<(PointF start, PointF end)> GetLines(this SvgPathSegmentList pathSegmentList)
        {

            var newPathData = new SvgPathSegmentList();
            for (int i = 0; i < pathSegmentList.Count; i++)
            {
                if (pathSegmentList[i].ToString().Contains("M"))
                    continue;
                if (pathSegmentList[i].ToString() == "z")
                {
                    newPathData.Add(new SvgLineSegment(pathSegmentList[i - 1].End, pathSegmentList[0].Start));
                    continue;
                }

                newPathData.Add(pathSegmentList[i]);
            }

            return newPathData.Select(seg => (seg.Start.Clone(), seg.End.Clone())).ToList();

        }
    }
}