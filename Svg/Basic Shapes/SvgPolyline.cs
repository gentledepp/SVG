using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Svg.Interfaces;

namespace Svg
{
    /// <summary>
    /// SvgPolyline defines a set of connected straight line segments. Typically, <see cref="SvgPolyline"/> defines open shapes.
    /// </summary>
    [SvgElement("polyline")]
    public class SvgPolyline : SvgPolygon
    {
       
        private GraphicsPath _Path;
        public override GraphicsPath Path(ISvgRenderer renderer)
        {
            if (_Path == null || this.IsPathDirty)
            {
                _Path?.Dispose();
                _Path = SvgEngine.Factory.CreateGraphicsPath();

                try
                {
                    for (int i = 0; (i + 1) < Points.Count; i += 2)
                    {
                        PointF endPoint = PointF.Create(Points[i].ToDeviceValue(renderer, UnitRenderingType.Horizontal, this), 
                                                     Points[i + 1].ToDeviceValue(renderer, UnitRenderingType.Vertical, this));

                        // TODO: Remove unrequired first line
                        if (_Path.PointCount == 0)
                        {
                            _Path.AddLine(endPoint, endPoint);
                        }
                        else
                        {
                            _Path.AddLine(_Path.GetLastPoint(), endPoint);
                        }
                    }
                }
                catch (Exception)
                {
                    //Trace.TraceError("Error rendering points: " + exc.Message);
                }
                this.IsPathDirty = false;
            }
            return _Path;
        }

        /// <summary>
        /// Renders the stroke of the <see cref="SvgVisualElement"/> to the specified <see cref="ISvgRenderer"/>
        /// </summary>
        /// <param name="renderer">The <see cref="ISvgRenderer"/> object to render to.</param>
        protected override bool RenderStroke(ISvgRenderer renderer, RenderCacheEntry cacheEntry)
        {
            var result = base.RenderStroke(renderer, cacheEntry);
            var path = this.Path(renderer);

            if (this.MarkerStart != null)
            {
                SvgMarker marker = this.OwnerDocument.GetElementById<SvgMarker>(this.MarkerStart.ToString());
                marker.RenderMarker(renderer, this, path.PathPoints[0], path.PathPoints[0], path.PathPoints[1]);
            }

            if (this.MarkerMid != null)
            {
                SvgMarker marker = this.OwnerDocument.GetElementById<SvgMarker>(this.MarkerMid.ToString());
                for (int i = 1; i <= path.PathPoints.Length - 2; i++)
                    marker.RenderMarker(renderer, this, path.PathPoints[i], path.PathPoints[i - 1], path.PathPoints[i], path.PathPoints[i + 1]);
            }

            if (this.MarkerEnd != null)
            {
                SvgMarker marker = this.OwnerDocument.GetElementById<SvgMarker>(this.MarkerEnd.ToString());
                marker.RenderMarker(renderer, this, path.PathPoints[path.PathPoints.Length - 1], path.PathPoints[path.PathPoints.Length - 2], path.PathPoints[path.PathPoints.Length - 1]);
            }

            return result;
        }

        protected internal override bool IntersectsWith(RectangleF rectangle, Matrix transform, int maxRecursion)
        {
            if (this.HasFill())
                return true;

            var units = Points.ToList();

            var lineSegments = new List<(PointF from, PointF to)>();
            for (var i = 0; i < units.Count - 3; i += 2)
            {
                lineSegments.Add((
                    PointF.Create(units[i].Value, units[i + 1].Value),
                    PointF.Create(units[i + 2].Value, units[i + 3].Value)
                ));
            }
            // does not add last line which connects last point to first point as this would be a polygon
            // see: https://www.w3schools.com/graphics/svg_polyline.asp

            return lineSegments.IsIntersectingWithLine(transform, rectangle);
        }
    }
}