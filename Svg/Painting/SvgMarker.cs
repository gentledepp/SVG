using System;
using System.Linq;
using Svg.DataTypes;
using Svg.Interfaces;
using Svg.Transforms;

namespace Svg
{
    [SvgElement("marker")]
    public class SvgMarker : SvgVisualElement, ISvgViewPort
    {
        private const string MARKER_POINT = "MARKER_POINT";
        private const string MARKER_ANGLE = "MARKER_ANGLE";
        private const string MARKER_OWNER = "MARKER_OWNER";

        private SvgOrient _svgOrient = new SvgOrient();

        [SvgAttribute("refX")]
        public virtual SvgUnit RefX
        {
            get { return this.Attributes.GetAttribute<SvgUnit>("refX"); }
            set { this.Attributes["refX"] = value; }
        }

        [SvgAttribute("refY")]
        public virtual SvgUnit RefY
        {
            get { return this.Attributes.GetAttribute<SvgUnit>("refY"); }
            set { this.Attributes["refY"] = value; }
        }


        [SvgAttribute("orient")]
        public virtual SvgOrient Orient
        {
            get { return _svgOrient; }
            set
            {
                _svgOrient = value;
                this.Attributes["orient"] = value;
            }
        }


        [SvgAttribute("overflow")]
        public virtual SvgOverflow Overflow
        {
            get { return this.Attributes.GetAttribute<SvgOverflow>("overflow"); }
            set { this.Attributes["overflow"] = value; }
        }


        [SvgAttribute("viewBox")]
        public virtual SvgViewBox ViewBox
        {
            get { return this.Attributes.GetAttribute<SvgViewBox>("viewBox"); }
            set { this.Attributes["viewBox"] = value; }
        }


        [SvgAttribute("preserveAspectRatio")]
        public virtual SvgAspectRatio AspectRatio
        {
            get { return this.Attributes.GetAttribute<SvgAspectRatio>("preserveAspectRatio"); }
            set { this.Attributes["preserveAspectRatio"] = value; }
        }


        [SvgAttribute("markerWidth")]
        public virtual SvgUnit MarkerWidth
        {
            get { return this.Attributes.GetAttribute<SvgUnit>("markerWidth"); }
            set { this.Attributes["markerWidth"] = value; }
        }

        [SvgAttribute("markerHeight")]
        public virtual SvgUnit MarkerHeight
        {
            get { return this.Attributes.GetAttribute<SvgUnit>("markerHeight"); }
            set { this.Attributes["markerHeight"] = value; }
        }

        [SvgAttribute("markerUnits")]
        public virtual SvgMarkerUnits MarkerUnits
        {
            get { return this.Attributes.GetAttribute<SvgMarkerUnits>("markerUnits"); }
            set { this.Attributes["markerUnits"] = value; }
        }

        /// <summary>
        /// Gets or sets the width of the stroke (if the <see cref="Stroke"/> property has a valid value specified.
        /// </summary>
        [SvgAttribute("stroke-width", true)]
        public override SvgUnit StrokeWidth
        {
            get { return (this.Attributes["stroke-width"] == null) ? new SvgUnit(0f) : (SvgUnit)this.Attributes["stroke-width"]; }
            set { this.Attributes["stroke-width"] = value; }
        }

        public SvgMarker()
        {
            MarkerUnits = SvgMarkerUnits.StrokeWidth;
            MarkerHeight = 3;
            MarkerWidth = 3;
            Overflow = SvgOverflow.Hidden;
        }

        public override GraphicsPath Path(ISvgRenderer renderer)
        {
            var path = this.Children.FirstOrDefault(x => x is SvgVisualElement);
            if (path != null)
                return (path as SvgVisualElement).Path(renderer);
            return null;
        }

        public override RectangleF GetBounds()
        {
            var path = this.Path(null);
            if (path != null)
            {
                return path.GetBounds();
            }
            return RectangleF.Create();
        }

        private RectangleF _bounds;
        public override RectangleF Bounds
        {
            get
            {

                return _bounds ??= GetBounds();
            }
        }

        public override SvgElement DeepCopy()
        {
            return DeepCopy<SvgMarker>();
        }

        public override SvgElement DeepCopy<T>()
        {
            var newObj = base.DeepCopy<T>() as SvgMarker;
            newObj.RefX = this.RefX;
            newObj.RefY = this.RefY;
            newObj.Orient = this.Orient;
            newObj.ViewBox = this.ViewBox;
            newObj.Overflow = this.Overflow;
            newObj.AspectRatio = this.AspectRatio;

            return newObj;
        }

        /// <summary>
        /// Render this marker using the slope of the given line segment
        /// </summary>
        /// <param name="pRenderer"></param>
        /// <param name="pOwner"></param>
        /// <param name="pMarkerPoint1"></param>
        /// <param name="pMarkerPoint2"></param>
        public void RenderMarker(ISvgRenderer pRenderer, SvgVisualElement pOwner, PointF pRefPoint, PointF pMarkerPoint1, PointF pMarkerPoint2)
        {
            float xDiff = pMarkerPoint2.X - pMarkerPoint1.X;
            float yDiff = pMarkerPoint2.Y - pMarkerPoint1.Y;
            float fAngle1 = (float)(Math.Atan2(yDiff, xDiff) * 180.0 / Math.PI);

            RenderPart2(fAngle1, pRenderer, pOwner, pRefPoint);
        }

        /// <summary>
        /// Render this marker using the average of the slopes of the two given line segments
        /// </summary>
        /// <param name="pRenderer"></param>
        /// <param name="pOwner"></param>
        /// <param name="pMarkerPoint1"></param>
        /// <param name="pMarkerPoint2"></param>
        /// <param name="pMarkerPoint3"></param>
        public void RenderMarker(ISvgRenderer pRenderer, SvgVisualElement pOwner, PointF pRefPoint, PointF pMarkerPoint1, PointF pMarkerPoint2, PointF pMarkerPoint3)
        {
            float xDiff = pMarkerPoint2.X - pMarkerPoint1.X;
            float yDiff = pMarkerPoint2.Y - pMarkerPoint1.Y;
            float fAngle1 = (float)(Math.Atan2(yDiff, xDiff) * 180.0 / Math.PI);

            xDiff = pMarkerPoint3.X - pMarkerPoint2.X;
            yDiff = pMarkerPoint3.Y - pMarkerPoint2.Y;
            float fAngle2 = (float)(Math.Atan2(yDiff, xDiff) * 180.0 / Math.PI);

            RenderPart2((fAngle1 + fAngle2) / 2, pRenderer, pOwner, pRefPoint);
        }

        /// <summary>
        /// Common code for rendering a marker once the orientation angle has been calculated
        /// </summary>
        /// <param name="fAngle"></param>
        /// <param name="pRenderer"></param>
        /// <param name="pOwner"></param>
        /// <param name="pMarkerPoint"></param>
        private void RenderPart2(float fAngle, ISvgRenderer pRenderer, SvgVisualElement pOwner, PointF pMarkerPoint)
        {
            using (pRenderer.UsingContextVariable(MARKER_POINT, pMarkerPoint))
            using (pRenderer.UsingContextVariable(MARKER_ANGLE, fAngle))
            using (pRenderer.UsingContextVariable(MARKER_OWNER, pOwner))
            using (pRenderer.UsingContextVariable(CONTEXT_STROKE, pOwner.Stroke))
            using (pRenderer.UsingContextVariable(CONTEXT_FILL, pOwner.Fill))
            {
                Render(pRenderer);
            }
        }

        protected internal override bool PushTransforms(ISvgRenderer renderer)
        {
            renderer.Graphics.Save();
            _graphicsMatrix = renderer.Transform;
            _graphicsClip = renderer.GetClip();

            foreach (SvgTransform transformation in this.Transforms)
            {
                transformation.ApplyTo(renderer);
            }
            
            object mp, fa, po;

            if (renderer.Context.TryGetValue(MARKER_POINT, out mp) &&
                renderer.Context.TryGetValue(MARKER_ANGLE, out fa) &&
                renderer.Context.TryGetValue(MARKER_OWNER, out po))
            {
                var pMarkerPoint = (PointF) mp;
                var fAngle = (float) fa;
                var pOwner = (SvgVisualElement) po;

                // marker point has to be 0,0 of markers coordinate system, so translate there
                renderer.TranslateTransform(pMarkerPoint.X, pMarkerPoint.Y);

                if (Orient.IsAuto)
                    renderer.RotateTransform(fAngle);
                else
                    renderer.RotateTransform(Orient.Angle);

                // get the scaled bounding box of the markers child elements
                var box = GetBoundingBox();

                // apply viewbox transform
                var vb = ViewBox;
                if (vb != null)
                {
                    vb.AddViewBoxTransform(new SvgAspectRatio(SvgPreserveAspectRatio.xMinYMin), renderer, box);
                }

                // apply marker transformations
                var transMatrix = Matrix.Create();

                switch (MarkerUnits)
                {
                    case SvgMarkerUnits.StrokeWidth:


                        var swDv = pOwner.StrokeWidth.ToDeviceValue(renderer, UnitRenderingType.Other, this);
                        
                        // scale by stroke width
                        var sx = swDv;
                        var sy = swDv;


                        var scale = Math.Min(sx, sy);
                        transMatrix.Scale(scale, scale);

                        if (vb != null)
                        {

                            box = GetBoundingBox(transMatrix);
                            // markerWidth and markerHeight are only considered given there is a viewBox set!
                            var mwDv = MarkerWidth.ToDeviceValue(renderer, UnitRenderingType.Other, this);
                            var mhDv = MarkerHeight.ToDeviceValue(renderer, UnitRenderingType.Other, this);
                            var markerWidthHeight = Math.Min(mwDv, mhDv);
                            var intendedSize = markerWidthHeight * swDv;
                            var markerScaleFactorX = intendedSize / box.Width;
                            var markerScaleFactorY = intendedSize / box.Height;

                            transMatrix.Scale(markerScaleFactorX, markerScaleFactorY);

                        }

                        var tx = -RefX.ToDeviceValue(renderer, UnitRenderingType.Horizontal, this);
                        var ty = -RefY.ToDeviceValue(renderer, UnitRenderingType.Horizontal, this);
                        transMatrix.Translate(tx, ty);


                        break;
                    case SvgMarkerUnits.UserSpaceOnUse:
                        transMatrix.Translate(-RefX.ToDeviceValue(renderer, UnitRenderingType.Horizontal, this),
                                                -RefY.ToDeviceValue(renderer, UnitRenderingType.Vertical, this));
                        break;

                }

                renderer.Graphics.Concat(transMatrix);
            }

            return true;
        }

        protected override bool Renderable => false;
    }
}