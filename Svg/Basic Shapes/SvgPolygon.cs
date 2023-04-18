using Svg.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Svg
{
    /// <summary>
    /// SvgPolygon defines a closed shape consisting of a set of connected straight line segments.
    /// </summary>
    [SvgElement("polygon")]
    public class SvgPolygon : SvgVisualElement
    {
        private GraphicsPath _path;
        private SvgAttributeCollection.InheritedAttribute<object> _points;
        private SvgAttributeCollection.Attribute<Uri> _markerEnd;
        private SvgAttributeCollection.Attribute<Uri> _markerMid;
        private SvgAttributeCollection.Attribute<Uri> _markerStart;

        /// <summary>
        /// The points that make up the SvgPolygon
        /// </summary>
        [SvgAttribute("points")]
        public SvgPointCollection Points
        {
            get { return (_points??=this.Attributes.GetInheritedAttribute<object>("points")).GetValue() as SvgPointCollection; }
            set
            {
                this.Attributes["points"] = value;
                this.IsPathDirty = true;
            }
        }

        /// <summary>
        /// Gets or sets the marker (end cap) of the path.
        /// </summary>
        [SvgAttribute("marker-end")]
        public Uri MarkerEnd
        {
            get { return (_markerEnd ??= this.Attributes.GetAttribute<Uri>("marker-end")).GetValue(); }
            set { this.Attributes["marker-end"] = value; }
        }


        /// <summary>
        /// Gets or sets the marker (start cap) of the path.
        /// </summary>
        [SvgAttribute("marker-mid")]
        public Uri MarkerMid
        {
            get { return (_markerMid ??= this.Attributes.GetAttribute<Uri>("marker-mid")).GetValue(); }
            set { this.Attributes["marker-mid"] = value; }
        }


        /// <summary>
        /// Gets or sets the marker (start cap) of the path.
        /// </summary>
        [SvgAttribute("marker-start")]
        public Uri MarkerStart
        {
            get { return (_markerStart ??= this.Attributes.GetAttribute<Uri>("marker-start")).GetValue(); }
            set { this.Attributes["marker-start"] = value; }
        }

        protected override bool RequiresSmoothRendering
        {
            get { return true; }
        }

        public override GraphicsPath Path(ISvgRenderer renderer)
        {
            if (this._path == null || this.IsPathDirty)
            {
                _path?.Dispose();
                this._path = SvgEngine.Factory.CreateGraphicsPath();
                this._path.StartFigure();

                try
                {
                    var points = this.Points;
                    for (int i = 2; (i + 1) < points.Count; i += 2)
                    {
                        var endPoint = SvgUnit.GetDevicePoint(points[i], points[i + 1], renderer, this);

                        //first line
                        if (_path.PointCount == 0)
                        {
                            _path.AddLine(SvgUnit.GetDevicePoint(points[i - 2], points[i - 1], renderer, this),
                                endPoint);
                        }
                        else
                        {
                            _path.AddLine(_path.GetLastPoint(), endPoint);
                        }
                    }
                }
                catch
                {
                    //Trace.TraceError("Error parsing points");
                }

                this._path.CloseFigure();
                this.IsPathDirty = false;
            }

            return this._path;
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
                    marker.RenderMarker(renderer, this, path.PathPoints[i], path.PathPoints[i - 1], path.PathPoints[i],
                        path.PathPoints[i + 1]);
            }

            if (this.MarkerEnd != null)
            {
                SvgMarker marker = this.OwnerDocument.GetElementById<SvgMarker>(this.MarkerEnd.ToString());
                marker.RenderMarker(renderer, this, path.PathPoints[path.PathPoints.Length - 1],
                    path.PathPoints[path.PathPoints.Length - 2], path.PathPoints[path.PathPoints.Length - 1]);
            }

            return result;
        }

        public override RectangleF GetBounds()
        {
            return this.Path(null).GetBounds();
        }
        
        public override SvgElement DeepCopy()
        {
            return DeepCopy<SvgPolygon>();
        }

        public override SvgElement DeepCopy<T>()
        {
            var newObj = base.DeepCopy<T>() as SvgPolygon;
            newObj.Points = new SvgPointCollection();
            foreach (var pt in this.Points)
                newObj.Points.Add(pt);
            return newObj;
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
            // finally, add last line segment that connects the last with the first point of the polygon...
            lineSegments.Add((
                PointF.Create(units[units.Count-2].Value, units[units.Count-1].Value),
                PointF.Create(units[0].Value, units[1].Value))
                );

            return lineSegments.IsIntersectingWithLine(transform, rectangle);
        }
    }
}