using System;
using System.Linq;
using System.Xml;
using Svg.Interfaces;
using Svg.Interfaces.Xml;

namespace Svg
{
    /// <summary>
    /// An <see cref="SvgFragment"/> represents an SVG fragment that can be the root element or an embedded fragment of an SVG document.
    /// </summary>
    [SvgElement("svg")]
    public class SvgFragment : SvgElement, ISvgViewPort, ISvgBoundable
    {
        /// <summary>
        /// Gets the SVG namespace string.
        /// </summary>
        public static readonly Uri Namespace = new Uri("http://www.w3.org/2000/svg");

        PointF ISvgBoundable.Location
        {
            get
            {
                return PointF.Create(X, Y);
            }
        }

        SizeF ISvgBoundable.Size
        {
            get
            {
                return GetDimensions();
            }
        }

        RectangleF ISvgBoundable.Bounds
        {
            get
            {
                var loc = ((ISvgBoundable) this).Location;
                var siz = ((ISvgBoundable) this).Size;
                return RectangleF.Create(loc.X, loc.Y, siz.Width, siz.Height);
            }
        }

        private SvgUnit _x;
        private SvgUnit _y;
        private SvgAttributeCollection.InheritedAttribute<float?> _fontSize;
        private SvgAttributeCollection.InheritedAttribute<string> _fontFamily;
        private SvgAttributeCollection.Attribute<SvgUnit> _width;
        private SvgAttributeCollection.Attribute<SvgOverflow> _overflow;
        private SvgAttributeCollection.Attribute<SvgUnit> _height;
        private SvgAttributeCollection.Attribute<SvgViewBox> _viewBox;
        private SvgAttributeCollection.Attribute<SvgAspectRatio> _preserveAspectRatio;

        /// <summary>
        /// Gets or sets the position where the left point of the svg should start.
        /// </summary>
        [SvgAttribute("x")]
        public SvgUnit X
        {
            get { return _x; }
            set
            {
                if (_x != value)
                {
                    var oldValue = _x;
                    _x = value;
                    this.Attributes["x"] = value;
                    OnAttributeChanged(new AttributeEventArgs("x", value, oldValue));
                }
            }
        }

        /// <summary>
        /// Gets or sets the position where the top point of the svg should start.
        /// </summary>
        [SvgAttribute("y")]
        public SvgUnit Y
        {
            get { return _y; }
            set
            {
                if (_y != value)
                {
                    var oldValue = _y;
                    _y = value;
                    this.Attributes["y"] = value;
                    OnAttributeChanged(new AttributeEventArgs("y", value, oldValue));
                }
            }
        }

        /// <summary>
        /// Gets or sets the width of the fragment.
        /// </summary>
        /// <value>The width.</value>
        [SvgAttribute("width")]
        public SvgUnit Width
        {
            get { return (_width ??= this.Attributes.GetAttribute<SvgUnit>("width")).GetValue(); }
            set { this.Attributes["width"] = value; }
        }

        /// <summary>
        /// Gets or sets the height of the fragment.
        /// </summary>
        /// <value>The height.</value>
        [SvgAttribute("height")]
        public SvgUnit Height
        {
            get { return (_height ??= this.Attributes.GetAttribute<SvgUnit>("height")).GetValue(); }
            set { this.Attributes["height"] = value; }
        }

        [SvgAttribute("overflow")]
        public virtual SvgOverflow Overflow
        {
            get { return (_overflow ??= this.Attributes.GetAttribute<SvgOverflow>("overflow")).GetValue(); }
            set { this.Attributes["overflow"] = value; }
        }

        /// <summary>
        /// Gets or sets the viewport of the element.
        /// </summary>
        /// <value></value>
        [SvgAttribute("viewBox")]
        public SvgViewBox ViewBox
        {
            get { return (_viewBox ??= this.Attributes.GetAttribute<SvgViewBox>("viewBox")).GetValue(); }
            set { this.Attributes["viewBox"] = value; }
        }

        /// <summary>
        /// Gets or sets the aspect of the viewport.
        /// </summary>
        /// <value></value>
        [SvgAttribute("preserveAspectRatio")]
        public SvgAspectRatio AspectRatio
        {
            get { return (_preserveAspectRatio ??= this.Attributes.GetAttribute<SvgAspectRatio>("preserveAspectRatio")).GetValue(); }
            set { this.Attributes["preserveAspectRatio"] = value; }
        }

        /// <summary>
        /// Refers to the size of the font from baseline to baseline when multiple lines of text are set solid in a multiline layout environment.
        /// </summary>
        [SvgAttribute("font-size")]
        public override SvgUnit FontSize
        {
            get
            {
                _fontSize ??= this.Attributes.GetInheritedAttribute<float?>("font-size");
                var v = _fontSize.GetValue();
                return (v == null) ? SvgUnit.Empty : (SvgUnit)v;
            }
            set { this.Attributes["font-size"] = value; }
        }

        /// <summary>
        /// Indicates which font family is to be used to render the text.
        /// </summary>
        [SvgAttribute("font-family")]
        public override string FontFamily
        {
            get { return (_fontFamily ??=this.Attributes.GetInheritedAttribute<string>("font-family")).GetValue(); }
            set { this.Attributes["font-family"] = value; }
        }

        /// <summary>
        /// Applies the required transforms to <see cref="ISvgRenderer"/>.
        /// </summary>
        /// <param name="renderer">The <see cref="ISvgRenderer"/> to be transformed.</param>
        protected internal override bool PushTransforms(ISvgRenderer renderer)
        {
            if (!base.PushTransforms(renderer)) return false;
            renderer.Graphics.Save();
            this.ViewBox.AddViewBoxTransform(this.AspectRatio, renderer, this);
            return true;
        }

        protected internal override void PopTransforms(ISvgRenderer renderer)
        {
            renderer.Graphics.Restore();
            base.PopTransforms(renderer);
        }

        protected override void Render(ISvgRenderer renderer)
        {
            switch (this.Overflow)
            {
                case SvgOverflow.Auto:
                case SvgOverflow.Visible:
                case SvgOverflow.Scroll:
                    base.Render(renderer);
                    break;
                default:
                    var prevClip = renderer.GetClip();
                    try
                    {
                        var size = (this.Parent == null ? renderer.GetBoundable().Bounds.Size : GetDimensions());
                        var clip = RectangleF.Create(this.X.ToDeviceValue(renderer, UnitRenderingType.Horizontal, this),
                                                  this.Y.ToDeviceValue(renderer, UnitRenderingType.Horizontal, this),
                                                  size.Width, size.Height);
                        renderer.SetClip(new Region(clip), CombineMode.Intersect);
                        base.Render(renderer);
                    }
                    finally
                    {
                        renderer.SetClip(prevClip, CombineMode.Replace);
                    }
                    break;
            }
        }

        /// <summary>
        /// Gets the <see cref="GraphicsPath"/> for this element.
        /// </summary>
        /// <value></value>
        public GraphicsPath Path
        {
            get
            {
                var path = SvgEngine.Factory.CreateGraphicsPath();

                AddPaths(this, path);

                return path;
            }
        }

        ///// <summary>
        ///// Gets the bounds of the svg element.
        ///// </summary>
        ///// <value>The bounds.</value>
        //public RectangleF Bounds
        //{
        //    get
        //    {
        //        return this.Path.GetBounds();
        //    }
        //}
        public RectangleF Bounds
        {
            get
            {
                var r = RectangleF.Create();
                foreach (var c in this.Children)
                {
                    if (c is SvgVisualElement)
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
                }

                return r;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SvgFragment"/> class.
        /// </summary>
        public SvgFragment()
        {
            _x = 0.0f;
            _y = 0.0f;
            this.Height = new SvgUnit(SvgUnitType.Percentage, 100.0f);
            this.Width = new SvgUnit(SvgUnitType.Percentage, 100.0f);
            this.ViewBox = SvgViewBox.Empty;
            this.AspectRatio = new SvgAspectRatio(SvgPreserveAspectRatio.xMidYMid);
        }

        public SizeF GetDimensions()
        {
            float w, h;
            var isWidthperc = Width.Type == SvgUnitType.Percentage;
            var isHeightperc = Height.Type == SvgUnitType.Percentage;

            RectangleF bounds = RectangleF.Create();
            if (isWidthperc || isHeightperc)
            {
                if (ViewBox.Width > 0 && ViewBox.Height > 0)
                {
                    bounds = RectangleF.Create(ViewBox.MinX, ViewBox.MinY, ViewBox.Width, ViewBox.Height);
                }
                else
                {
                    bounds = this.Bounds; //do just one call to the recursive bounds property
                }
            }

            if (isWidthperc)
            {
                w = (bounds.Width + bounds.X) * (Width.Value * 0.01f);
            }
            else
            {
                w = Width.ToDeviceValue(null, UnitRenderingType.Horizontal, this);
            }
            if (isHeightperc)
            {
                h = (bounds.Height + bounds.Y) * (Height.Value * 0.01f);
            }
            else
            {
                h = Height.ToDeviceValue(null, UnitRenderingType.Vertical, this);
            }

            return SizeF.Create(w, h);
        }

        public override SvgElement DeepCopy()
        {
            return DeepCopy<SvgFragment>();
        }

        public override SvgElement DeepCopy<T>()
        {
            var newObj = base.DeepCopy<T>() as SvgFragment;
            newObj.Height = this.Height;
            newObj.Width = this.Width;
            newObj.Overflow = this.Overflow;
            newObj.ViewBox = this.ViewBox.DeepCopy();
            newObj.AspectRatio = this.AspectRatio.DeepCopy();
            return newObj;
        }

        protected override void WriteStartElementInternal(IXmlTextWriter writer)
        {
            if (ElementName != string.Empty)
            {
                var baseNamespace = SvgAttributeAttribute.Namespaces.Single(x => string.IsNullOrEmpty(x.Key));

                writer.WriteStartElement(ElementName, baseNamespace.Value);
            }


            foreach (var ns in SvgAttributeAttribute.Namespaces)
            {
                if (!string.IsNullOrEmpty(ns.Key))
                    writer.WriteAttributeString(ns.Key, "xmlns", ns.Value);
            }

            writer.WriteAttributeString("version", "1.1");
        }
    }
}