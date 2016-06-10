using System;
using System.Drawing;
using System.Diagnostics;

namespace Svg
{
    /// <summary>
    /// SvgPolyline defines a set of connected straight line segments. Typically, <see cref="SvgPolyline"/> defines open shapes.
    /// </summary>
    [SvgElement("polyline")]
    public class SvgPolyline : SvgPolygon
    {
        /// <summary>
        /// Gets or sets the marker (end cap) of the path.
        /// </summary>
        [SvgAttribute("marker-end")]
        public Uri MarkerEnd
        {
            get { return this.Attributes.GetAttribute<Uri>("marker-end"); }
            set { this.Attributes["marker-end"] = value; }
        }


        /// <summary>
        /// Gets or sets the marker (start cap) of the path.
        /// </summary>
        [SvgAttribute("marker-mid")]
        public Uri MarkerMid
        {
            get { return this.Attributes.GetAttribute<Uri>("marker-mid"); }
            set { this.Attributes["marker-mid"] = value; }
        }


        /// <summary>
        /// Gets or sets the marker (start cap) of the path.
        /// </summary>
        [SvgAttribute("marker-start")]
        public Uri MarkerStart
        {
            get { return this.Attributes.GetAttribute<Uri>("marker-start"); }
            set { this.Attributes["marker-start"] = value; }
        }

        private GraphicsPath _Path;
        public override GraphicsPath Path(ISvgRenderer renderer)
        {
            if (_Path == null || this.IsPathDirty)
            {
                _Path = SvgSetup.Factory.CreateGraphicsPath();

                try
                {
                    for (int i = 0; (i + 1) < Points.Count; i += 2)
                    {
                        PointF endPoint = new PointF(Points[i].ToDeviceValue(renderer, UnitRenderingType.Horizontal, this), 
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
                catch (Exception exc)
                {
                    Trace.TraceError("Error rendering points: " + exc.Message);
                }
                this.IsPathDirty = false;
            }
            return _Path;
        }

        /// <summary>
        /// Renders the stroke of the <see cref="SvgVisualElement"/> to the specified <see cref="ISvgRenderer"/>
        /// </summary>
        /// <param name="renderer">The <see cref="ISvgRenderer"/> object to render to.</param>
        protected internal override bool RenderStroke(ISvgRenderer renderer)
        {
            var result = base.RenderStroke(renderer);
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
    }
}