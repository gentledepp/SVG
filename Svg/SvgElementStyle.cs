using System;
using System.Collections.Generic;
using System.Linq;
using Svg.Interfaces;

namespace Svg
{
    public partial class SvgElement
    {
        private bool _dirty;
        
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

        private SvgPaintServer _fill;
        /// <summary>
        /// Gets or sets the fill <see cref="SvgPaintServer"/> of this element.
        /// </summary>
        [SvgAttribute("fill", true)]
        public virtual SvgPaintServer Fill
        {
            get
            {
                if(_fill == null)
                    _fill = (this.Attributes["fill"] == null) ? SvgColourServer.NotSet : (SvgPaintServer)this.Attributes["fill"];
                return _fill;
            }
            set
            {
                this.Attributes["fill"] = value;
                _fill = value;
            }
        }

        private SvgPaintServer _stroke;
        /// <summary>
        /// Gets or sets the <see cref="SvgPaintServer"/> to be used when rendering a stroke around this element.
        /// </summary>
        [SvgAttribute("stroke", true)]
        public virtual SvgPaintServer Stroke
        {
            get
            {
                if(_stroke == null)
                    _stroke = Attributes.GetInheritedAttribute<SvgPaintServer>("stroke");
                
                return _stroke;
            }
            set
            {
                Attributes["stroke"] = value;
                _stroke = value;
            }
        }

        private SvgFillRule? _fillRule;
        [SvgAttribute("fill-rule", true)]
        public virtual SvgFillRule FillRule
        {
            get
            {
                if(_fillRule == null)
                    _fillRule = (this.Attributes["fill-rule"] == null) ? SvgFillRule.NonZero : (SvgFillRule)this.Attributes["fill-rule"];
                return _fillRule.Value;
            }
            set
            {
                this.Attributes["fill-rule"] = value;
                _fillRule = value;
            }
        }

        private float? _fillOpacity;
        /// <summary>
        /// Gets or sets the opacity of this element's <see cref="Fill"/>.
        /// </summary>
        [SvgAttribute("fill-opacity", true)]
        public virtual float FillOpacity
        {
            get
            {
                if (_fillOpacity == null)
                {
                    _fillOpacity = (this.Attributes["fill-opacity"] == null)
                        ? this.Opacity
                        : (float)this.Attributes["fill-opacity"];
                }
                
                return _fillOpacity.Value;
            }
            set
            {
                var v = FixOpacityValue(value);
                this.Attributes["fill-opacity"] = v;
                _fillOpacity = v;
            }
        }

        private SvgUnit? _strokeWidth;
        /// <summary>
        /// Gets or sets the width of the stroke (if the <see cref="Stroke"/> property has a valid value specified.
        /// </summary>
        [SvgAttribute("stroke-width", true)]
        public virtual SvgUnit StrokeWidth
        {
            get
            {
                if (_strokeWidth == null)
                    _strokeWidth = (this.Attributes["stroke-width"] == null) ? new SvgUnit(1.0f) : (SvgUnit)this.Attributes["stroke-width"];

                return _strokeWidth.Value;

            }
            set
            {
                this.Attributes["stroke-width"] = value;
                _strokeWidth = value;
            }
        }

        private SvgStrokeLineCap? _strokeLineCap;
        [SvgAttribute("stroke-linecap", true)]
        public virtual SvgStrokeLineCap StrokeLineCap
        {
            get
            {
                if(_strokeLineCap == null)
                    _strokeLineCap = (this.Attributes["stroke-linecap"] == null) ? SvgStrokeLineCap.Butt : (SvgStrokeLineCap)this.Attributes["stroke-linecap"];
                
                return _strokeLineCap.Value;}
            set
            {
                this.Attributes["stroke-linecap"] = value;
                _strokeLineCap = value;
            }
        }

        private SvgStrokeLineJoin? _strokeLineJoin;
        [SvgAttribute("stroke-linejoin", true)]
        public virtual SvgStrokeLineJoin StrokeLineJoin
        {
            get
            {
                if(_strokeLineJoin == null)
                    _strokeLineJoin = (this.Attributes["stroke-linejoin"] == null) ? SvgStrokeLineJoin.Miter : (SvgStrokeLineJoin)this.Attributes["stroke-linejoin"];
                
                return _strokeLineJoin.Value;
            }
            set
            {
                this.Attributes["stroke-linejoin"] = value;
                _strokeLineJoin = value;
            }
        }

        private float? _strokeMiterLimit;
        [SvgAttribute("stroke-miterlimit", true)]
        public virtual float StrokeMiterLimit
        {
            get
            {
                if(_strokeMiterLimit == null)
                    _strokeMiterLimit = (this.Attributes["stroke-miterlimit"] == null) ? 4f : (float)this.Attributes["stroke-miterlimit"];

                return _strokeMiterLimit.Value;
            }
            set
            {
                this.Attributes["stroke-miterlimit"] = value;
                _strokeMiterLimit = value;
            }
        }

        private SvgUnitCollection _strokeDashArray;
        [SvgAttribute("stroke-dasharray", true)]
        public virtual SvgUnitCollection StrokeDashArray
        {
            get
            {
                if (_strokeDashArray == null)
                    _strokeDashArray = Attributes.GetInheritedAttribute<SvgUnitCollection>("stroke-dasharray") ?? SvgUnitCollection.Inherit;
                return _strokeDashArray;
            }
            set
            {
                Attributes["stroke-dasharray"] = value;
                _strokeDashArray = value;
            }
        }

        private SvgUnit? _strokeDashOffset;
        [SvgAttribute("stroke-dashoffset", true)]
        public virtual SvgUnit StrokeDashOffset
        {
            get
            {
                if(_strokeDashOffset == null)
                    _strokeDashOffset= (this.Attributes["stroke-dashoffset"] == null) ? SvgUnit.Empty : (SvgUnit)this.Attributes["stroke-dashoffset"];

                return _strokeDashOffset.Value;
            }
            set
            {
                this.Attributes["stroke-dashoffset"] = value;
                _strokeDashOffset = value;
            }
        }

        private float? _strokeOpacity;
        /// <summary>
        /// Gets or sets the opacity of the stroke, if the <see cref="Stroke"/> property has been specified. 1.0 is fully opaque; 0.0 is transparent.
        /// </summary>
        [SvgAttribute("stroke-opacity", true)]
        public virtual float StrokeOpacity
        {
            get
            {
                if(_strokeOpacity == null)
                    _strokeOpacity = (this.Attributes["stroke-opacity"] == null) ? this.Opacity : (float)this.Attributes["stroke-opacity"];
                
                return _strokeOpacity.Value;

            }
            set
            {
                var v = FixOpacityValue(value);
                this.Attributes["stroke-opacity"] = v;
                _strokeOpacity = v;
            }
        }

        private SvgPaintServer _stopColor;
        /// <summary>
        /// Gets or sets the colour of the gradient stop.
        /// </summary>
        /// <remarks>Apparently this can be set on non-sensical elements.  Don't ask; just check the tests.</remarks>
        [SvgAttribute("stop-color", true)]
        //[TypeConverter(typeof(SvgPaintServerFactory))]
        public SvgPaintServer StopColor
        {
            get
            {
                if (_stopColor == null)
                    _stopColor = this.Attributes["stop-color"] as SvgPaintServer;
                return _stopColor;
            }
            set
            {
                this.Attributes["stop-color"] = value;
                _stopColor = value;
            }
        }

        private float? _opacity;
        /// <summary>
        /// Gets or sets the opacity of the element. 1.0 is fully opaque; 0.0 is transparent.
        /// </summary>
        [SvgAttribute("opacity", true)]
        public virtual float Opacity
        {
            get
            {
                if(_opacity == null)
                    _opacity = (this.Attributes["opacity"] == null) ? 1.0f : (float)this.Attributes["opacity"];
                return _opacity.Value;
            }
            set
            {
                var v = FixOpacityValue(value);
                this.Attributes["opacity"] = v;
                _opacity = v;
            }
        }

        private string _fontFamily;
        /// <summary>
        /// Indicates which font family is to be used to render the text.
        /// </summary>
        [SvgAttribute("font-family", true)]
        public virtual string FontFamily
        {
            get
            {
                if(_fontFamily == null)
                    _fontFamily = this.Attributes["font-family"] as string;

                return _fontFamily;
            }
            set
            {
                this.Attributes["font-family"] = value; 
                this.IsPathDirty = true;
                _fontFamily = value;
            }
        }

        private SvgUnit? _fontSize;
        /// <summary>
        /// Refers to the size of the font from baseline to baseline when multiple lines of text are set solid in a multiline layout environment.
        /// </summary>
        [SvgAttribute("font-size", true)]
        public virtual SvgUnit FontSize
        {
            get
            {
                if (_fontSize == null)
                    _fontSize = (this.Attributes["font-size"] == null) ? SvgUnit.Empty : (SvgUnit)this.Attributes["font-size"];
                
                return _fontSize.Value;
            }
            set
            {
                this.Attributes["font-size"] = value; this.IsPathDirty = true;
                _fontSize = value;
            }
        }

        // TODO: add font-stretch attribute
        private SvgFontStyle? _fontStyle;
        /// <summary>
        /// Refers to the style of the font.
        /// </summary>
        [SvgAttribute("font-style", true)]
        public virtual SvgFontStyle FontStyle
        {
            get
            {
                if (_fontStyle == null)
                    _fontStyle = (this.Attributes["font-style"] == null) ? SvgFontStyle.All : (SvgFontStyle)this.Attributes["font-style"];
                return _fontStyle.Value;
            }
            set
            {
                this.Attributes["font-style"] = value; this.IsPathDirty = true;
                _fontStyle = value;
            }
        }

        private SvgFontVariant? _fontVariant;
        /// <summary>
        /// Refers to the varient of the font.
        /// </summary>
        [SvgAttribute("font-variant", true)]
        public virtual SvgFontVariant FontVariant
        {
            get
            {
                if(_fontVariant == null)
                    _fontVariant = (this.Attributes["font-variant"] == null) ? SvgFontVariant.Inherit : (SvgFontVariant)this.Attributes["font-variant"];
                return _fontVariant.Value;
            }
            set
            {
                this.Attributes["font-variant"] = value; this.IsPathDirty = true;
                _fontVariant = value;
            }
        }

        private SvgTextDecoration? _textDecoration;
        /// <summary>
        /// Refers to the boldness of the font.
        /// </summary>
        [SvgAttribute("text-decoration", true)]
        public virtual SvgTextDecoration TextDecoration
        {
            get
            {
                if(_textDecoration == null)
                    _textDecoration = (this.Attributes["text-decoration"] == null) ? SvgTextDecoration.Inherit : (SvgTextDecoration)this.Attributes["text-decoration"];
                return _textDecoration.Value;

            }
            set
            {
                this.Attributes["text-decoration"] = value; this.IsPathDirty = true;
                _textDecoration = value;
            }
        }

        private SvgFontWeight? _fontWeight;
        /// <summary>
        /// Refers to the boldness of the font.
        /// </summary>
        [SvgAttribute("font-weight", true)]
        public virtual SvgFontWeight FontWeight
        {
            get
            {
                if (_fontWeight == null)
                {
                    _fontWeight = (this.Attributes["font-weight"] == null) ? SvgFontWeight.Inherit : (SvgFontWeight)this.Attributes["font-weight"];
                }
                return _fontWeight.Value;

            }
            set
            {
                this.Attributes["font-weight"] = value; this.IsPathDirty = true;
                _fontWeight = value;
            }
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

        private string _font;
        /// <summary>
        /// Set all font information.
        /// </summary>
        [SvgAttribute("font", true)]
        public virtual string Font
        {
            get
            {
                if (_font == null)
                    _font = (this.Attributes["font"] == null ? "" : this.Attributes["font"] as string);
                return _font;
            }
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
                _font = value;
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
