using System;
using System.Collections.Generic;
using System.Linq;
using Svg.Basic_Shapes;
using Svg.Interfaces;
using Svg.Pathing;
using Svg.Transforms;

namespace Svg
{
    /// <summary>
    /// Represents an SVG path element.
    /// </summary>
    [SvgElement("path")]
    public class SvgPath : SvgVisualElement
    {
        private GraphicsPath _path;
        private SvgAttributeCollection.Attribute<SvgPathSegmentList> _d;
        private SvgAttributeCollection.Attribute<float> _pathLength;
        private SvgAttributeCollection.Attribute<Uri> _markerEnd;
        private SvgAttributeCollection.Attribute<Uri> _markerMid;
        private SvgAttributeCollection.Attribute<Uri> _markerStart;

        /// <summary>
        /// Gets or sets a <see cref="SvgPathSegmentList"/> of path data.
        /// </summary>
        [SvgAttribute("d", true)]
        public SvgPathSegmentList PathData
        {
        	get { return (_d ??= this.Attributes.GetAttribute<SvgPathSegmentList>("d")).GetValue(); }
            set
            {
            	this.Attributes["d"] = value;
            	value._owner = this;
                this.IsPathDirty = true;
            }
        }

        /// <summary>
        /// Gets or sets the length of the path.
        /// </summary>
        [SvgAttribute("pathLength", true)]
        public float PathLength
        {
            get { return (_pathLength ??= this.Attributes.GetAttribute<float>("pathLength")).GetValue(); }
            set { this.Attributes["pathLength"] = value; }
        }

		
        /// <summary>
        /// Gets or sets the marker (end cap) of the path.
        /// </summary>
        [SvgAttribute("marker-end", true)]
		public Uri MarkerEnd
        {
			get { return (_markerEnd ??= this.Attributes.GetAttribute<Uri>("marker-end")).GetValue(); }
			set { this.Attributes["marker-end"] = value; }
		}


		/// <summary>
		/// Gets or sets the marker (start cap) of the path.
		/// </summary>
        [SvgAttribute("marker-mid", true)]
		public Uri MarkerMid
		{
			get { return (_markerMid ??= this.Attributes.GetAttribute<Uri>("marker-mid")).GetValue(); }
			set { this.Attributes["marker-mid"] = value; }
		}


		/// <summary>
		/// Gets or sets the marker (start cap) of the path.
		/// </summary>
        [SvgAttribute("marker-start", true)]
		public Uri MarkerStart
		{
			get { return (_markerStart ??= this.Attributes.GetAttribute<Uri>("marker-start")).GetValue(); }
			set { this.Attributes["marker-start"] = value; }
		}


        /// <summary>
        /// Gets the <see cref="GraphicsPath"/> for this element.
        /// </summary>
        public override GraphicsPath Path(ISvgRenderer renderer)
        {
            if (this._path == null || this.IsPathDirty)
            {
                _path?.Dispose();
                _path = SvgEngine.Factory.CreateGraphicsPath();

                foreach (SvgPathSegment segment in this.PathData)
                {
                    segment.AddToPath(_path);
                }

                this.IsPathDirty = false;
            }
            return _path;
        }

        internal void OnPathUpdated(SvgPathSegmentList oldValue)
        {
            this.IsPathDirty = true;
            OnAttributeChanged(new AttributeEventArgs("d", PathData, oldValue));
        }

        /// <summary>
        /// Marks the path as dirty so it will be rebuilt on the next render,
        /// without triggering attribute change events or undo/redo cloning.
        /// </summary>
        public void MarkPathDirty()
        {
            this.IsPathDirty = true;
        }

        /// <summary>
        /// Appends a single segment to the existing GraphicsPath without rebuilding.
        /// If the path hasn't been built yet, marks it dirty for a full rebuild on next render.
        /// </summary>
        public void AppendSegmentToPath(SvgPathSegment segment)
        {
            if (_path == null)
            {
                IsPathDirty = true;
                return;
            }
            segment.AddToPath(_path);
        }

        /// <summary>
        /// Gets or sets a value to determine if anti-aliasing should occur when the element is being rendered.
        /// </summary>
        protected override bool RequiresSmoothRendering
        {
            get { return true; }
        }

        public override RectangleF GetBounds()
        {
            return this.GetBoundsWithClipPaths();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SvgPath"/> class.
        /// </summary>
        public SvgPath()
        {
            var pathData = new SvgPathSegmentList();
            this.Attributes["d"] = pathData;
            pathData._owner = this;
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

            var lineSegments = PathData.GetLines();
            
            return lineSegments.IsIntersectingWithLine(transform, rectangle);
        }


        public override SvgElement DeepCopy()
		{
			return DeepCopy<SvgPath>();
		}

		public override SvgElement DeepCopy<T>()
		{
			var newObj = base.DeepCopy<T>() as SvgPath;
			foreach (var pathData in this.PathData)
				newObj.PathData.Add(pathData.Clone());
			newObj.PathLength = this.PathLength;
			newObj.MarkerStart = this.MarkerStart;
			newObj.MarkerEnd = this.MarkerEnd;
			return newObj;

		}
    }
}