using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Svg.Interfaces;
using Svg.Transforms;

namespace Svg
{
    /// <summary>
    /// The class that all SVG elements should derive from when they are to be rendered.
    /// </summary>
    public abstract partial class SvgVisualElement : SvgElement, ISvgBoundable, ISvgStylable, ISvgClipable
    {
        private bool _requiresSmoothRendering;
        private Region _previousClip;
        public const string CONTEXT_STROKE = "context-stroke";
        public const string CONTEXT_FILL = "context-fill";

        /// <summary>
        /// Gets the <see cref="GraphicsPath"/> for this element.
        /// </summary>
        public abstract GraphicsPath Path(ISvgRenderer renderer);

        PointF ISvgBoundable.Location
        {
            get
            {
                return Bounds.Location;
            }
        }

        SizeF ISvgBoundable.Size
        {
            get
            {
                return Bounds.Size;
            }
        }

        /// <summary>
        /// Gets the bounds of the element.
        /// </summary>
        /// <value>The bounds.</value>
        public virtual RectangleF Bounds
        {
            get
            {
                if (Renderable)
                {
                    return this.Path(null).GetBounds();
                }
                else
                {
                    var r = RectangleF.Create();
                    foreach (var c in this.Children.OfType<SvgVisualElement>())
                    {
                        // First it should check if rectangle is empty or it will return the wrong Bounds.
                        // This is because when the Rectangle is Empty, the Union method adds as if the first values where X=0, Y=0
                        if (r.IsEmpty)
                        {
                            r = ((SvgVisualElement)c).Bounds;
                        }
                        else
                        {
                            var childBounds = ((SvgVisualElement)c).Bounds;
                            if (!childBounds.IsEmpty)
                            {
                                r = r.UnionAndCopy(childBounds);
                            }
                        }
                    }

                    return r;
                }
            }
        }
        
        public PointF[] GetTransformedPoints(Matrix transform = null)
        {
            if (transform == null)
                transform = Matrix.Create();
            else
                transform = transform.Clone();
            
            if (Renderable)
            {
                return this.GetTransformedElementPoints(transform);
            }
            else
            {
                return this.GetTransformedChildPoints(transform);
            }
        }

        public RectangleF GetBoundingBox(Matrix transform = null)
        {
            var pts = GetTransformedPoints(transform);
            
            return RectangleF.FromPoints(pts);
        }
        
        [Obsolete("Use the overload with selectionMode")]
        public IEnumerable<TElement> HitTest<TElement>(RectangleF rectangle,
            SelectionType selectionType = SelectionType.Intersect,
            Matrix transform = null, 
            int maxRecursion = int.MaxValue) where TElement : SvgVisualElement
        {
            return this.HitTest<TElement>(rectangle, selectionType, HitTestResultMode.ReturnAllMatchingDescendants, transform ?? Matrix.Create(), maxRecursion);
        }
        
        /// <summary>
        /// This method gives a specific shape the chance to add additional hit testing logic.
        /// In case the elements bounding box is found during hit-testing, this method will be called, so that the element can still refuse to be hit
        /// This is useful, if e.g. a polygon without fill color should only be considered hit, if its border/line was hit
        /// </summary>
        /// <param name="rectangle"></param>
        /// <param name="transform"></param>
        /// <param name="maxRecursion"></param>
        /// <returns></returns>
        protected internal virtual bool IntersectsWith(RectangleF rectangle, Matrix transform, int maxRecursion)
        {
            return true;
        }
        
        /// <summary>
        /// Gets the associated <see cref="SvgClipPath"/> if one has been specified.
        /// </summary>
        [SvgAttribute("clip")]
        public virtual string Clip
        {
            get { return this.Attributes.GetInheritedAttribute<string>("clip"); }
            set { this.Attributes["clip"] = value; }
        }

        /// <summary>
        /// Gets the associated <see cref="SvgClipPath"/> if one has been specified.
        /// </summary>
        [SvgAttribute("clip-path")]
        public virtual Uri ClipPath
        {
            get { return this.Attributes.GetInheritedAttribute<Uri>("clip-path"); }
            set { this.Attributes["clip-path"] = value; }
        }

        /// <summary>
        /// Gets or sets the algorithm which is to be used to determine the clipping region.
        /// </summary>
        [SvgAttribute("clip-rule")]
        public SvgClipRule ClipRule
        {
            get { return this.Attributes.GetAttribute<SvgClipRule>("clip-rule", SvgClipRule.NonZero); }
            set { this.Attributes["clip-rule"] = value; }
        }

        /// <summary>
        /// Gets the associated <see cref="SvgClipPath"/> if one has been specified.
        /// </summary>
        [SvgAttribute("filter")]
        public virtual Uri Filter
        {
            get { return this.Attributes.GetInheritedAttribute<Uri>("filter"); }
            set { this.Attributes["filter"] = value; }
        }
        
        /// <summary>
        /// Gets or sets a value to determine if anti-aliasing should occur when the element is being rendered.
        /// </summary>
        protected virtual bool RequiresSmoothRendering
        {
            get { return this._requiresSmoothRendering; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SvgGraphicsElement"/> class.
        /// </summary>
        public SvgVisualElement()
        {
            this.IsPathDirty = true;
            this._requiresSmoothRendering = false;
        }

        protected internal virtual bool Renderable { get { return true; } }

        /// <summary>
        /// Renders the <see cref="SvgElement"/> and contents to the specified <see cref="Graphics"/> object.
        /// </summary>
        /// <param name="renderer">The <see cref="ISvgRenderer"/> object to render to.</param>
        protected override void Render(ISvgRenderer renderer)
        {
            this.Render(renderer, true);
        }

        private void Render(ISvgRenderer renderer, bool renderFilter)
        {
            if (this.Visible && this.Displayable && this.PushTransforms(renderer) &&
                (!Renderable || this.Path(renderer) != null))
            {
                bool renderNormal = true;

                if (renderFilter && this.Filter != null)
                {
                    var filterPath = this.Filter;
                    if (filterPath.ToString().StartsWith("url("))
                    {
                        filterPath = new Uri(filterPath.ToString().Substring(4, filterPath.ToString().Length - 5), UriKind.RelativeOrAbsolute);
                    }
                    var filter = this.OwnerDocument.IdManager.GetElementById(filterPath) as FilterEffects.SvgFilter;
                    if (filter != null)
                    {
                        this.PopTransforms(renderer);
                        try
                        {
                            filter.ApplyFilter(this, renderer, (r) => this.Render(r, false));
                        }
                        catch (Exception ex)
                        {
                            SvgEngine.Logger.Info(ex.ToString());
                        }
                        renderNormal = false;
                    }
                }


                if (renderNormal)
                {
                    this.SetClip(renderer);

                    if (Renderable)
                    {
                        // If this element needs smoothing enabled turn anti-aliasing on
                        if (this.RequiresSmoothRendering)
                        {
                            renderer.SmoothingMode = SmoothingMode.AntiAlias;
                        }

                        this.RenderFill(renderer);
                        this.RenderStroke(renderer);

                        // Reset the smoothing mode
                        if (this.RequiresSmoothRendering && renderer.SmoothingMode == SmoothingMode.AntiAlias)
                        {
                            renderer.SmoothingMode = SmoothingMode.Default;
                        }
                    }
                    else
                    {
                        base.RenderChildren(renderer);
                    }

                    this.ResetClip(renderer);
                    this.PopTransforms(renderer);
                }

            }
        }

        /// <summary>
        /// Renders the fill of the <see cref="SvgVisualElement"/> to the specified <see cref="ISvgRenderer"/>
        /// </summary>
        /// <param name="renderer">The <see cref="ISvgRenderer"/> object to render to.</param>
        protected internal virtual void RenderFill(ISvgRenderer renderer)
        {
            object fillTemp;
            var fill = this.Fill;
            if (fill == SvgColourServer.ContextFill &&
                renderer.Context.TryGetValue(CONTEXT_FILL, out fillTemp))
            {
                fill = (SvgPaintServer)fillTemp;
            }

            if (this.HasFill())
            {
                /*using (*/
                //var brush = GetFillBrush(renderer);/*)*/
                using(var brush = fill.GetBrush(this, renderer, Math.Min(Math.Max(this.FillOpacity * this.Opacity, 0), 1)))
                {
                    if (brush != null)
                    {
                        this.Path(renderer).FillMode = this.FillRule == SvgFillRule.NonZero ? FillMode.Winding : FillMode.Alternate;
                        renderer.FillPath(brush, this.Path(renderer));
                    }
                }
            }
        }

        /// <summary>
        /// Renders the stroke of the <see cref="SvgVisualElement"/> to the specified <see cref="ISvgRenderer"/>
        /// </summary>
        /// <param name="renderer">The <see cref="ISvgRenderer"/> object to render to.</param>
        protected internal virtual bool RenderStroke(ISvgRenderer renderer)
        {
            // allow to override stroke using context variable (used by marker to have same stoke color as owning path)
            object strokeTemp;
            SvgPaintServer stroke = this.Stroke;
            if (this.Stroke == SvgColourServer.ContextStroke &&
                renderer.Context.TryGetValue(CONTEXT_STROKE, out strokeTemp))
            {
                stroke = (SvgPaintServer)strokeTemp;
            }

            if (this.HasStroke())
            {
                float strokeWidth = this.StrokeWidth.ToDeviceValue(renderer, UnitRenderingType.Other, this);
                //using (var brush = GetStrokeBrush(renderer))
                using(var brush = stroke.GetBrush(this, renderer, Math.Min(Math.Max(this.StrokeOpacity * this.Opacity, 0), 1), true))
                {
                    if (brush != null)
                    {
                        var path = this.Path(renderer);
                        var bounds = path.GetBounds();
                        if (path.PointCount < 1) return false;
                        if (bounds.Width <= 0 && bounds.Height <= 0)
                        {
                            switch (this.StrokeLineCap)
                            {
                                case SvgStrokeLineCap.Round:
                                    using (var capPath = SvgEngine.Factory.CreateGraphicsPath())
                                    {
                                        capPath.AddEllipse(path.PathPoints[0].X - strokeWidth / 2, path.PathPoints[0].Y - strokeWidth / 2, strokeWidth, strokeWidth);
                                        renderer.FillPath(brush, capPath);
                                    }
                                    break;
                                case SvgStrokeLineCap.Square:
                                    using (var capPath = SvgEngine.Factory.CreateGraphicsPath())
                                    {
                                        capPath.AddRectangle(RectangleF.Create(path.PathPoints[0].X - strokeWidth / 2, path.PathPoints[0].Y - strokeWidth / 2, strokeWidth, strokeWidth));
                                        renderer.FillPath(brush, capPath);
                                    }
                                    break;
                            }
                        }
                        else
                        {
                            using (var pen = SvgEngine.Factory.CreatePen(brush, strokeWidth))
                            {
                                var strokeDashArray = StrokeDashArray;
                                if (!SvgUnitCollection.IsNullOrEmpty(strokeDashArray))
                                {
                                    var dashPattern = strokeDashArray.ConvertAll(u => u.ToDeviceValue(renderer, UnitRenderingType.Other, this)).ToArray();
                                    pen.DashPattern = dashPattern.Length % 2 == 0 ? dashPattern : dashPattern.Concat(dashPattern).ToArray();
                                }

                                pen.DashOffset = StrokeDashOffset;

                                switch (StrokeLineJoin)
                                {
                                    case SvgStrokeLineJoin.Bevel:
                                        pen.LineJoin = LineJoin.Bevel;
                                        break;
                                    case SvgStrokeLineJoin.Round:
                                        pen.LineJoin = LineJoin.Round;
                                        break;
                                    default:
                                        pen.LineJoin = LineJoin.Miter;
                                        break;
                                }
                                pen.MiterLimit = StrokeMiterLimit;
                                switch (StrokeLineCap)
                                {
                                    case SvgStrokeLineCap.Round:
                                        pen.StartCap = LineCap.Round;
                                        pen.EndCap = LineCap.Round;
                                        break;
                                    case SvgStrokeLineCap.Square:
                                        pen.StartCap = LineCap.Square;
                                        pen.EndCap = LineCap.Square;
                                        break;
                                }

                                renderer.DrawPath(pen, path);

                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }
        
        /// <summary>
        /// Sets the clipping region of the specified <see cref="ISvgRenderer"/>.
        /// </summary>
        /// <param name="renderer">The <see cref="ISvgRenderer"/> to have its clipping region set.</param>
        protected internal virtual void SetClip(ISvgRenderer renderer)
        {
            if (this.ClipPath != null || !string.IsNullOrEmpty(this.Clip))
            {
                this._previousClip = renderer.GetClip();

                if (this.ClipPath != null)
                {
                    SvgClipPath clipPath = this.OwnerDocument.GetElementById<SvgClipPath>(this.ClipPath.ToString());
                    if (clipPath != null) renderer.SetClip(clipPath.GetClipRegion(this), CombineMode.Intersect);
                }

                var clip = this.Clip;
                if (!string.IsNullOrEmpty(clip) && clip.StartsWith("rect("))
                {
                    clip = clip.Trim();
                    var offsets = (from o in clip.Substring(5, clip.Length - 6).Split(',')
                                   select float.Parse(o.Trim())).ToList();
                    var bounds = this.Bounds;
                    var clipRect = RectangleF.Create(bounds.Left + offsets[3], bounds.Top + offsets[0],
                                                  bounds.Width - (offsets[3] + offsets[1]),
                                                  bounds.Height - (offsets[2] + offsets[0]));
                    renderer.SetClip(new Region(clipRect), CombineMode.Intersect);
                }
            }
        }

        /// <summary>
        /// Resets the clipping region of the specified <see cref="ISvgRenderer"/> back to where it was before the <see cref="SetClip"/> method was called.
        /// </summary>
        /// <param name="renderer">The <see cref="ISvgRenderer"/> to have its clipping region reset.</param>
        protected internal virtual void ResetClip(ISvgRenderer renderer)
        {
            if (this._previousClip != null)
            {
                renderer.SetClip(this._previousClip);
                this._previousClip = null;
            }
        }

        /// <summary>
        /// Sets the clipping region of the specified <see cref="ISvgRenderer"/>.
        /// </summary>
        /// <param name="renderer">The <see cref="ISvgRenderer"/> to have its clipping region set.</param>
        void ISvgClipable.SetClip(ISvgRenderer renderer)
        {
            this.SetClip(renderer);
        }

        /// <summary>
        /// Resets the clipping region of the specified <see cref="ISvgRenderer"/> back to where it was before the <see cref="SetClip"/> method was called.
        /// </summary>
        /// <param name="renderer">The <see cref="ISvgRenderer"/> to have its clipping region reset.</param>
        void ISvgClipable.ResetClip(ISvgRenderer renderer)
        {
            this.ResetClip(renderer);
        }

        public override SvgElement DeepCopy<T>()
        {
            var newObj = base.DeepCopy<T>() as SvgVisualElement;
            newObj.ClipPath = this.ClipPath;
            newObj.ClipRule = this.ClipRule;
            newObj.Filter = this.Filter;

            newObj.Visible = this.Visible;
            if (this.Fill != null)
                newObj.Fill = this.Fill;
            if (this.Stroke != null)
                newObj.Stroke = this.Stroke;
            newObj.FillRule = this.FillRule;
            newObj.FillOpacity = this.FillOpacity;
            newObj.StrokeWidth = this.StrokeWidth;
            newObj.StrokeLineCap = this.StrokeLineCap;
            newObj.StrokeLineJoin = this.StrokeLineJoin;
            newObj.StrokeMiterLimit = this.StrokeMiterLimit;
            newObj.StrokeDashArray = this.StrokeDashArray;
            newObj.StrokeDashOffset = this.StrokeDashOffset;
            newObj.StrokeOpacity = this.StrokeOpacity;
            newObj.Opacity = this.Opacity;

            return newObj;
        }
    }
}
