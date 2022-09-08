using System;

using Svg.Interfaces;

namespace Svg
{
    /// <summary>
    /// Represents and SVG line element.
    /// </summary>
    [SvgElement("line")]
    public class SvgLine : SvgVisualElement
    {
        private SvgUnit _startX;
        private SvgUnit _startY;
        private SvgUnit _endX;
        private SvgUnit _endY;
        private GraphicsPath _path;

        [SvgAttribute("x1")]
        public SvgUnit StartX
        {
            get { return this._startX; }
            set 
            { 
            	if(_startX != value)
	            {
	                var oldValue = _startX;
            		this._startX = value;
                    this.Attributes["x1"] = value;
                    this.IsPathDirty = true;
            		OnAttributeChanged(new AttributeEventArgs("x1", value, oldValue));
            	}
            }
        }

        [SvgAttribute("y1")]
        public SvgUnit StartY
        {
            get { return this._startY; }
            set 
            { 
            	if(_startY != value)
                {
                    var oldValue = _startY;
                    this._startY = value;
                    this.Attributes["y1"] = value;
                    this.IsPathDirty = true;
            		OnAttributeChanged(new AttributeEventArgs("y1", value, oldValue));
            	}
            }
        }

        [SvgAttribute("x2")]
        public SvgUnit EndX
        {
            get { return this._endX; }
            set 
            { 
            	if(_endX != value)
                {
                    var oldValue = _endX;
                    this._endX = value;
                    this.Attributes["x2"] = value;
                    this.IsPathDirty = true;
            		OnAttributeChanged(new AttributeEventArgs("x2", value, oldValue));
            	}
            }
        }

        [SvgAttribute("y2")]
        public SvgUnit EndY
        {
            get { return this._endY; }
            set 
            { 
            	if(_endY != value)
                {
                    var oldValue = _endY;
                    this._endY = value;
                    this.Attributes["y2"] = value;
                    this.IsPathDirty = true;
            		OnAttributeChanged(new AttributeEventArgs("y2", value, oldValue));
            	}
            }
        }

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

        public override GraphicsPath Path(ISvgRenderer renderer)
        {
            if (this._path == null || this.IsPathDirty)
            {
                PointF start = PointF.Create(this.StartX.ToDeviceValue(renderer, UnitRenderingType.Horizontal, this), 
                                          this.StartY.ToDeviceValue(renderer, UnitRenderingType.Vertical, this));
                PointF end = PointF.Create(this.EndX.ToDeviceValue(renderer, UnitRenderingType.Horizontal, this), 
                                        this.EndY.ToDeviceValue(renderer, UnitRenderingType.Vertical, this));

                this._path = SvgEngine.Factory.CreateGraphicsPath();
                this._path.AddLine(start, end);
                this.IsPathDirty = false;
            }
            return this._path;
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

        protected internal override bool IntersectsWith(RectangleF rectangle, Matrix transform, int maxRecursion)
        {
            var start = PointF.Create(this.StartX, this.StartY);
            var end = PointF.Create(this.EndX, this.EndY);
            
            return (start,end).IsIntersectingWithLine(transform, rectangle);
        }

        public override RectangleF Bounds
        {
            get
            {
                return this.Path(null).GetBounds();
            }
        }

		public override SvgElement DeepCopy()
		{
			return DeepCopy<SvgLine>();
		}

		public override SvgElement DeepCopy<T>()
		{
			var newObj = base.DeepCopy<T>() as SvgLine;
			newObj.StartX = this.StartX;
			newObj.EndX = this.EndX;
			newObj.StartY = this.StartY;
			newObj.EndY = this.EndY;
			if (this.Fill != null)
				newObj.Fill = this.Fill.DeepCopy() as SvgPaintServer;

			return newObj;
		}

    }
}
