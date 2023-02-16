using System;
using System.Collections.Generic;
using System.Linq;
using Svg.Interfaces;

namespace Svg
{
    public partial class SvgElement
    {
        private bool _dirty;
        private SvgAttributeCollection.InheritedAttribute<SvgPaintServer> _stroke;
        private SvgAttributeCollection.InheritedAttribute<SvgUnitCollection> _strokeDashArray;
        protected SvgAttributeCollection.InheritedAttribute<object> _fill;
        private SvgAttributeCollection.InheritedAttribute<object> _fillRule;
        private SvgAttributeCollection.InheritedAttribute<object> _fillOpacity;
        private SvgAttributeCollection.InheritedAttribute<object> _strokeWidth;
        private SvgAttributeCollection.InheritedAttribute<object> _strokeLinecap;
        private SvgAttributeCollection.InheritedAttribute<object> _strokeLinejoin;
        private SvgAttributeCollection.InheritedAttribute<object> _strokeMiterlimit;
        private SvgAttributeCollection.InheritedAttribute<object> _strokeDashoffset;
        private SvgAttributeCollection.InheritedAttribute<object> _strokeOpacity;
        private SvgAttributeCollection.InheritedAttribute<object> _stopColor;
        private SvgAttributeCollection.InheritedAttribute<object> _opacity;
        protected SvgAttributeCollection.InheritedAttribute<object> _fontFamily;
        protected SvgAttributeCollection.InheritedAttribute<object> _fontSize;
        protected SvgAttributeCollection.InheritedAttribute<object> _fontStyle;
        protected SvgAttributeCollection.InheritedAttribute<object> _fontVariant;
        private SvgAttributeCollection.InheritedAttribute<object> _textDecoration;
        protected SvgAttributeCollection.InheritedAttribute<object> _fontWeight;
        private SvgAttributeCollection.InheritedAttribute<object> _font;

        /// <summary>
        /// Gets or sets a value indicating whether this element's <see cref="System.IO.Path"/> is dirty.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the path is dirty; otherwise, <c>false</c>.
        /// </value>
        protected virtual bool IsPathDirty
        {
            get { return this._dirty; }
            set { this._dirty = value; }
        }

        private static float FixOpacityValue(float value)
        {
            const float max = 1.0f;
            const float min = 0.0f;
            return Math.Min(Math.Max(value, min), max);
        }

        /// <summary>
        /// Gets or sets the fill <see cref="SvgPaintServer"/> of this element.
        /// </summary>
        [SvgAttribute("fill", true)]
        public virtual SvgPaintServer Fill
        {
            get { return (_fill??=this.Attributes.GetInheritedAttribute<object>("fill")).GetValue() is {} val ? (SvgPaintServer)val: SvgColourServer.NotSet; }
            set { this.Attributes["fill"] = value; }
        }

        /// <summary>
        /// Gets or sets the <see cref="SvgPaintServer"/> to be used when rendering a stroke around this element.
        /// </summary>
        [SvgAttribute("stroke", true)]
        public virtual SvgPaintServer Stroke
        {
            get { return (_stroke ??= Attributes.GetInheritedAttribute<SvgPaintServer>("stroke")).GetValue(); }
            set { Attributes["stroke"] = value; }
        }

        [SvgAttribute("fill-rule", true)]
        public virtual SvgFillRule FillRule
        {
            get { return (_fillRule ??= this.Attributes.GetInheritedAttribute<object>("fill-rule")).GetValue() is {} val ? (SvgFillRule)val: SvgFillRule.NonZero; }
            set { this.Attributes["fill-rule"] = value; }
        }

        /// <summary>
        /// Gets or sets the opacity of this element's <see cref="Fill"/>.
        /// </summary>
        [SvgAttribute("fill-opacity", true)]
        public virtual float FillOpacity
        {
            get { return (_fillOpacity ??= this.Attributes.GetInheritedAttribute<object>("fill-opacity")).GetValue() is {} val ? (float)val: this.Opacity ; }
            set { this.Attributes["fill-opacity"] = FixOpacityValue(value); }
        }

        /// <summary>
        /// Gets or sets the width of the stroke (if the <see cref="Stroke"/> property has a valid value specified.
        /// </summary>
        [SvgAttribute("stroke-width", true)]
        public virtual SvgUnit StrokeWidth
        {
            get
            {
                return (_strokeWidth ??= this.Attributes.GetInheritedAttribute<object>("stroke-width")).GetValue() is
                    { } val
                        ? (SvgUnit)val
                        : new SvgUnit(1.0f); }
            set { this.Attributes["stroke-width"] = value; }
        }

        [SvgAttribute("stroke-linecap", true)]
        public virtual SvgStrokeLineCap StrokeLineCap
        {
            get
            {
                return (_strokeLinecap ??= this.Attributes.GetInheritedAttribute<object>("stroke-linecap"))
                    .GetValue() is { } val
                        ? (SvgStrokeLineCap)val
                        : SvgStrokeLineCap.Butt; }
            set { this.Attributes["stroke-linecap"] = value; }
        }

        [SvgAttribute("stroke-linejoin", true)]
        public virtual SvgStrokeLineJoin StrokeLineJoin
        {
            get { return (_strokeLinejoin ??= this.Attributes.GetInheritedAttribute<object>("stroke-linejoin")).GetValue() is { } val ? (SvgStrokeLineJoin)val: SvgStrokeLineJoin.Miter; }
            set { this.Attributes["stroke-linejoin"] = value; }
        }

        [SvgAttribute("stroke-miterlimit", true)]
        public virtual float StrokeMiterLimit
        {
            get { return (_strokeMiterlimit ??= this.Attributes.GetInheritedAttribute<object>("stroke-miterlimit")).GetValue() is { } val ? (float)val: 4f; }
            set { this.Attributes["stroke-miterlimit"] = value; }
        }

        [SvgAttribute("stroke-dasharray", true)]
        public virtual SvgUnitCollection StrokeDashArray
        {
            get
            {
                var val = (_strokeDashArray ??= Attributes.GetInheritedAttribute<SvgUnitCollection>("stroke-dasharray"))
                    .GetValue();
                return val ?? SvgUnitCollection.Inherit;
            }
            set { Attributes["stroke-dasharray"] = value; }
        }

        [SvgAttribute("stroke-dashoffset", true)]
        public virtual SvgUnit StrokeDashOffset
        {
            get { return (_strokeDashoffset ??= this.Attributes.GetInheritedAttribute<object>("stroke-dashoffset")).GetValue() is { } val ? (SvgUnit)val: SvgUnit.Empty; }
            set { this.Attributes["stroke-dashoffset"] = value; }
        }

        /// <summary>
        /// Gets or sets the opacity of the stroke, if the <see cref="Stroke"/> property has been specified. 1.0 is fully opaque; 0.0 is transparent.
        /// </summary>
        [SvgAttribute("stroke-opacity", true)]
        public virtual float StrokeOpacity
        {
            get { return (_strokeOpacity ??= this.Attributes.GetInheritedAttribute<object>("stroke-opacity")).GetValue() is { } val ? (float)val: this.Opacity; }
            set { this.Attributes["stroke-opacity"] = FixOpacityValue(value); }
        }

        /// <summary>
        /// Gets or sets the colour of the gradient stop.
        /// </summary>
        /// <remarks>Apparently this can be set on non-sensical elements.  Don't ask; just check the tests.</remarks>
        [SvgAttribute("stop-color", true)]
        //[TypeConverter(typeof(SvgPaintServerFactory))]
        public SvgPaintServer StopColor
        {
            get { return (_stopColor ??= this.Attributes.GetInheritedAttribute<object>("stop-color")).GetValue() as SvgPaintServer; }
            set { this.Attributes["stop-color"] = value; }
        }

        /// <summary>
        /// Gets or sets the opacity of the element. 1.0 is fully opaque; 0.0 is transparent.
        /// </summary>
        [SvgAttribute("opacity", true)]
        public virtual float Opacity
        {
            get { return (_opacity ??= this.Attributes.GetInheritedAttribute<object>("opacity")).GetValue() is { } val ? (float)val: 1.0f; }
            set { this.Attributes["opacity"] = FixOpacityValue(value); }
        }

        /// <summary>
        /// Indicates which font family is to be used to render the text.
        /// </summary>
        [SvgAttribute("font-family", true)]
        public virtual string FontFamily
        {
            get { return (_fontFamily ??= this.Attributes.GetInheritedAttribute<object>("font-family")).GetValue() as string; }
            set { this.Attributes["font-family"] = value; this.IsPathDirty = true; }
        }

        /// <summary>
        /// Refers to the size of the font from baseline to baseline when multiple lines of text are set solid in a multiline layout environment.
        /// </summary>
        [SvgAttribute("font-size", true)]
        public virtual SvgUnit FontSize
        {
            get { return (_fontSize ??= this.Attributes.GetInheritedAttribute<object>("font-size")).GetValue() is { } val ? (SvgUnit)val: SvgUnit.Empty; }
            set { this.Attributes["font-size"] = value; this.IsPathDirty = true; }
        }

        // TODO: add font-stretch attribute

        /// <summary>
        /// Refers to the style of the font.
        /// </summary>
        [SvgAttribute("font-style", true)]
        public virtual SvgFontStyle FontStyle
        {
            get { return (_fontStyle ??= this.Attributes.GetInheritedAttribute<object>("font-style")).GetValue() is { } val ? (SvgFontStyle)val: SvgFontStyle.All; }
            set { this.Attributes["font-style"] = value; this.IsPathDirty = true; }
        }

        /// <summary>
        /// Refers to the varient of the font.
        /// </summary>
        [SvgAttribute("font-variant", true)]
        public virtual SvgFontVariant FontVariant
        {
            get { return (_fontVariant ??= this.Attributes.GetInheritedAttribute<object>("font-variant")).GetValue() is { } val ? (SvgFontVariant)val: SvgFontVariant.Inherit; }
            set { this.Attributes["font-variant"] = value; this.IsPathDirty = true; }
        }

        /// <summary>
        /// Refers to the boldness of the font.
        /// </summary>
        [SvgAttribute("text-decoration", true)]
        public virtual SvgTextDecoration TextDecoration
        {
            get { return (_textDecoration ??= this.Attributes.GetInheritedAttribute<object>("text-decoration")).GetValue() is { } val ? (SvgTextDecoration)val: SvgTextDecoration.Inherit; }
            set { this.Attributes["text-decoration"] = value; this.IsPathDirty = true; }
        }

        /// <summary>
        /// Refers to the boldness of the font.
        /// </summary>
        [SvgAttribute("font-weight", true)]
        public virtual SvgFontWeight FontWeight
        {
            get { return (_fontWeight ??= this.Attributes.GetInheritedAttribute<object>("font-weight")).GetValue() is { } val ? (SvgFontWeight)val: SvgFontWeight.Inherit; }
            set { this.Attributes["font-weight"] = value; this.IsPathDirty = true; }
        }

        private enum FontParseState
        {
            fontStyle,
            fontVariant,
            fontWeight,
            fontSize,
            fontFamilyNext,
            fontFamilyCurr
        }

        /// <summary>
        /// Set all font information.
        /// </summary>
        [SvgAttribute("font", true)]
        public virtual string Font
        {
            get { return (_font ??= this.Attributes.GetInheritedAttribute<object>("font")).GetValue() is { } val? val as string: string.Empty; }
            set
            {
                var state = FontParseState.fontStyle;
                var parts = value.Split(' ');

                SvgFontStyle fontStyle;
                SvgFontVariant fontVariant;
                SvgFontWeight fontWeight;
                SvgUnit fontSize;

                bool success;
                string[] sizes;
                string part;

                for (int i = 0; i < parts.Length; i++)
                {
                    part = parts[i];
                    success = false;
                    while (!success)
                    {
                        switch (state)
                        {
                            case FontParseState.fontStyle:
                                success = Enum.TryParse<SvgFontStyle>(part, out fontStyle);
                                if (success) this.FontStyle = fontStyle;
                                state++;
                                break;
                            case FontParseState.fontVariant:
                                success = Enum.TryParse<SvgFontVariant>(part, out fontVariant);
                                if (success) this.FontVariant = fontVariant;
                                state++;
                                break;
                            case FontParseState.fontWeight:
                                success = Enum.TryParse<SvgFontWeight>(part, out fontWeight);
                                if (success) this.FontWeight = fontWeight;
                                state++;
                                break;
                            case FontParseState.fontSize:
                                sizes = part.Split('/');
                                try
                                {
                                    fontSize = (SvgUnit)(SvgEngine.Resolve<ISvgUnitConverter>().ConvertFromInvariantString(sizes[0]));
                                    success = true;
                                    this.FontSize = fontSize;
                                }
                                catch { }
                                state++;
                                break;
                            case FontParseState.fontFamilyNext:
                                state++;
                                success = true;
                                break;
                        }
                    }

                    switch (state)
                    {
                        case FontParseState.fontFamilyNext:
                            this.FontFamily = string.Join(" ", parts, i + 1, parts.Length - (i + 1));
                            i = int.MaxValue - 2;
                            break;
                        case FontParseState.fontFamilyCurr:
                            this.FontFamily = string.Join(" ", parts, i, parts.Length - (i));
                            i = int.MaxValue - 2;
                            break;
                    }

                }

                this.Attributes["font"] = value;
                this.IsPathDirty = true;
            }
        }

        private const string DefaultFontFamily = "Times New Roman";

        /// <summary>
        /// Get the font information based on data stored with the text object or inherited from the parent.
        /// </summary>
        /// <returns></returns>
        internal IFontDefn GetFont(ISvgRenderer renderer)
        {
            // Get the font-size
            float fontSize;
            var fontSizeUnit = this.FontSize;
            if (fontSizeUnit == SvgUnit.None || fontSizeUnit == SvgUnit.Empty)
            {
                fontSize = 1.0f;
            }
            else
            {
                fontSize = fontSizeUnit.ToDeviceValue(renderer, UnitRenderingType.Vertical, this);
            }

            var family = ValidateFontFamily(this.FontFamily, this.OwnerDocument);
            var sFaces = family as IEnumerable<SvgFontFace>;

            if (sFaces == null)
            {
                var fontStyle = Svg.FontStyle.Regular;

                // Get the font-weight
                switch (this.FontWeight)
                {
                    //Note: Bold is not listed because it is = W700.
                    case SvgFontWeight.Bolder:
                    case SvgFontWeight.W600:
                    case SvgFontWeight.W700:
                    case SvgFontWeight.W800:
                    case SvgFontWeight.W900:
                        fontStyle |= Svg.FontStyle.Bold;
                        break;
                }

                // Get the font-style
                switch (this.FontStyle)
                {
                    case SvgFontStyle.Italic:
                    case SvgFontStyle.Oblique:
                        fontStyle |= Svg.FontStyle.Italic;
                        break;
                }

                // Get the text-decoration
                switch (this.TextDecoration)
                {
                    case SvgTextDecoration.LineThrough:
                        fontStyle |= Svg.FontStyle.Strikeout;
                        break;
                    case SvgTextDecoration.Underline:
                        fontStyle |= Svg.FontStyle.Underline;
                        break;
                }

                var ff = family as FontFamily;
                if (!ff.IsStyleAvailable(fontStyle))
                {
                    // Do Something
                }

                // Get the font-family
                return new GdiFontDefn(SvgEngine.Factory.CreateFont(ff, fontSize, fontStyle, GraphicsUnit.Pixel));
            }
            else
            {
                var font = sFaces.First().Parent as SvgFont;
                if (font == null)
                {
                    var uri = sFaces.First().Descendants().OfType<SvgFontFaceUri>().First().ReferencedElement;
                    font = OwnerDocument.IdManager.GetElementById(uri) as SvgFont;
                }
                return new SvgFontDefn(font, fontSize, OwnerDocument.Ppi);
            }
        }

        public static object ValidateFontFamily(string fontFamilyList, SvgDocument doc)
        {
            // Split font family list on "," and then trim start and end spaces and quotes.
            var fontParts = (fontFamilyList ?? "").Split(new[] { ',' }).Select(fontName => fontName.Trim(new[] { '"', ' ', '\'' }));

            var ffProvider = SvgEngine.Factory.GetFontFamilyProvider();

            var families = ffProvider.Families;
            FontFamily family;
            IEnumerable<SvgFontFace> sFaces;

            // Find a the first font that exists in the list of installed font families.
            //styles from IE get sent through as lowercase.
            foreach (var f in fontParts)
            {
                if (doc.FontDefns().TryGetValue(f, out sFaces)) return sFaces;

                family = families.FirstOrDefault(ff => ff.Name.ToLower() == f.ToLower());
                if (family != null) return family;

                switch (f)
                {
                    case "serif":
                        return ffProvider.GenericSerif;
                    case "sans-serif":
                        return ffProvider.GenericSansSerif;
                    case "monospace":
                        return ffProvider.GenericMonospace;
                }
            }

            // No valid font family found from the list requested.
            return ffProvider.GenericSansSerif;
        }
    }
}
