using Svg.Interfaces;
using Svg.Pathing;
using System.Linq;

namespace Svg
{
    [SvgElement("glyph")]
    public class SvgGlyph : SvgVisualElement
    {
        private GraphicsPath _path;
        protected SvgAttributeCollection.InheritedAttribute<object> _glyphName;
        private SvgAttributeCollection.InheritedAttribute<object> _horizAdvX;
        private SvgAttributeCollection.InheritedAttribute<object> _unicode;
        private SvgAttributeCollection.InheritedAttribute<object> _vertAdvY;
        private SvgAttributeCollection.InheritedAttribute<object> _vertOriginX;
        private SvgAttributeCollection.InheritedAttribute<object> _vertOriginY;
        private SvgAttributeCollection.Attribute<SvgPathSegmentList> _d;

        /// <summary>
        /// Gets or sets a <see cref="SvgPathSegmentList"/> of path data.
        /// </summary>
        [SvgAttribute("d", true)]
        public SvgPathSegmentList PathData
        {
            get { return (_d ??= this.Attributes.GetAttribute<SvgPathSegmentList>("d")).GetValue(); }
            set { this.Attributes["d"] = value; }
        }

        [SvgAttribute("glyph-name", true)]
        public virtual string GlyphName
        {
            get { return (_glyphName ??= this.Attributes.GetInheritedAttribute<object>("glyph-name")).GetValue() as string; }
            set { this.Attributes["glyph-name"] = value; }
        }
        [SvgAttribute("horiz-adv-x", true)]
        public float HorizAdvX
        {
            get { return (_horizAdvX ??= this.Attributes.GetInheritedAttribute<object>("horiz-adv-x")).GetValue() is {} val ? (float)val: this.Parents.OfType<SvgFont>().First().HorizAdvX; }
            set { this.Attributes["horiz-adv-x"] = value; }
        }
        [SvgAttribute("unicode", true)]
        public string Unicode
        {
            get { return (_unicode ??= this.Attributes.GetInheritedAttribute<object>("unicode")).GetValue() as string; }
            set { this.Attributes["unicode"] = value; }
        }
        [SvgAttribute("vert-adv-y", true)]
        public float VertAdvY
        {
            get { return (_vertAdvY ??= this.Attributes.GetInheritedAttribute<object>("vert-adv-y")).GetValue() is { } val ? (float)val: this.Parents.OfType<SvgFont>().First().VertAdvY; }
            set { this.Attributes["vert-adv-y"] = value; }
        }
        [SvgAttribute("vert-origin-x", true)]
        public float VertOriginX
        {
            get { return (_vertOriginX ??= this.Attributes.GetInheritedAttribute<object>("vert-origin-x")).GetValue() is { } val ? (float)val: this.Parents.OfType<SvgFont>().First().VertOriginX; }
            set { this.Attributes["vert-origin-x"] = value; }
        }
        [SvgAttribute("vert-origin-y", true)]
        public float VertOriginY
        {
            get { return (_vertOriginY ??= this.Attributes.GetInheritedAttribute<object>("vert-origin-y")).GetValue() is { } val ? (float)val: this.Parents.OfType<SvgFont>().First().VertOriginY; }
            set { this.Attributes["vert-origin-y"] = value; }
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

        
        /// <summary>
        /// Gets or sets a value to determine if anti-aliasing should occur when the element is being rendered.
        /// </summary>
        protected override bool RequiresSmoothRendering
        {
            get { return true; }
        }


        public override RectangleF GetBounds()
        {
            return this.Path(null).GetBounds();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SvgGlyph"/> class.
        /// </summary>
        public SvgGlyph()
        {
            var pathData = new SvgPathSegmentList();
            this.Attributes["d"] = pathData;
        }

        public override SvgElement DeepCopy()
        {
            return DeepCopy<SvgGlyph>();
        }

        public override SvgElement DeepCopy<T>()
        {
            var newObj = base.DeepCopy<T>() as SvgGlyph;
            foreach (var pathData in this.PathData)
                newObj.PathData.Add(pathData.Clone());
            return newObj;

        }
    }
}
