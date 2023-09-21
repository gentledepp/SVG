namespace Svg
{
    [SvgElement("font-face")]
    public class SvgFontFace : SvgElement
    {
        private SvgAttributeCollection.InheritedAttribute<object> _alphabetic;
        private SvgAttributeCollection.InheritedAttribute<object> _ascent;
        private SvgAttributeCollection.InheritedAttribute<object> _ascentHeight;
        private SvgAttributeCollection.InheritedAttribute<object> _descent;
        private SvgAttributeCollection.InheritedAttribute<object> _panose1;
        private SvgAttributeCollection.InheritedAttribute<object> _unitsPerEm;
        private SvgAttributeCollection.InheritedAttribute<object> _xHeight;

        [SvgAttribute("alphabetic")]
        public float Alphabetic
        {
            get { return (_alphabetic ??= this.Attributes.GetInheritedAttribute<object>("alphabetic")).GetValue() is { } val ? (float)val: 0; }
            set { this.Attributes["alphabetic"] = value; }
        }

        [SvgAttribute("ascent")]
        public float Ascent
        {
            get
            {
                var val = (_ascent ??= this.Attributes.GetInheritedAttribute<object>("ascent")).GetValue();
                if (val == null) 
                {
                    var font = this.Parent as SvgFont;
                    return (font == null ? 0 : this.UnitsPerEm - font.VertOriginY);
                }
                else
                {
                    return (float)val;
                }
            }
            set { this.Attributes["ascent"] = value; }
        }

        [SvgAttribute("ascent-height")]
        public float AscentHeight
        {
            get { return (_ascentHeight ??= this.Attributes.GetInheritedAttribute<object>("ascent-height")).GetValue() is { } val ? (float)val: this.Ascent; }
            set { this.Attributes["ascent-height"] = value; }
        }

        [SvgAttribute("descent")]
        public float Descent
        {
            get
            {
                var val = (_descent ??= this.Attributes.GetInheritedAttribute<object>("descent")).GetValue();
                if (val == null) 
                {
                    var font = this.Parent as SvgFont;
                    return (font == null ? 0 : font.VertOriginY);
                }
                else 
                {
                    return (float)this.Attributes["descent"];
                }
            }
            set { this.Attributes["descent"] = value; }
        }

        /// <summary>
        /// Indicates which font family is to be used to render the text.
        /// </summary>
        [SvgAttribute("font-family")]
        public virtual string FontFamily
        {
            get { return (_fontFamily ??= this.Attributes.GetInheritedAttribute<object>("font-family")).GetValue() as string; }
            set { this.Attributes["font-family"] = value; }
        }

        /// <summary>
        /// Refers to the size of the font from baseline to baseline when multiple lines of text are set solid in a multiline layout environment.
        /// </summary>
        [SvgAttribute("font-size")]
        public virtual SvgUnit FontSize
        {
            get { return (_fontSize ??= this.Attributes.GetInheritedAttribute<object>("font-size")).GetValue() is { } val ? (SvgUnit)val: SvgUnit.Empty; }
            set { this.Attributes["font-size"] = value; }
        }

        /// <summary>
        /// Refers to the style of the font.
        /// </summary>
        [SvgAttribute("font-style")]
        public virtual SvgFontStyle FontStyle
        {
            get { return (_fontStyle ??= this.Attributes.GetInheritedAttribute<object>("font-style")).GetValue() is { } val ? (SvgFontStyle)val: SvgFontStyle.All; }
            set { this.Attributes["font-style"] = value; }
        }

        /// <summary>
        /// Refers to the varient of the font.
        /// </summary>
        [SvgAttribute("font-variant")]
        public virtual SvgFontVariant FontVariant
        {
            get { return (_fontVariant ??= this.Attributes.GetInheritedAttribute<object>("font-variant")).GetValue() is { } val ? (SvgFontVariant)val: SvgFontVariant.Inherit; }
            set { this.Attributes["font-variant"] = value; }
        }

        /// <summary>
        /// Refers to the boldness of the font.
        /// </summary>
        [SvgAttribute("font-weight")]
        public virtual SvgFontWeight FontWeight
        {
            get { return (_fontWeight ??= this.Attributes.GetInheritedAttribute<object>("font-weight")).GetValue() is { } val ? (SvgFontWeight)val: SvgFontWeight.Inherit; }
            set { this.Attributes["font-weight"] = value; }
        }

        [SvgAttribute("panose-1")]
        public string Panose1
        {
            get { return (_panose1 ??= this.Attributes.GetInheritedAttribute<object>("panose-1")).GetValue() as string; }
            set { this.Attributes["panose-1"] = value; }
        }

        [SvgAttribute("units-per-em")]
        public float UnitsPerEm
        {
            get { return (_unitsPerEm ??= this.Attributes.GetInheritedAttribute<object>("units-per-em")).GetValue() is { } val ? (float)val: 1000; }
            set { this.Attributes["units-per-em"] = value; }
        }

        [SvgAttribute("x-height")]
        public float XHeight
        {
            get { return (_xHeight ??= this.Attributes.GetInheritedAttribute<object>("x-height")).GetValue() is { } val ? (float)val: float.MinValue; }
            set { this.Attributes["x-height"] = value; }
        }


        public override SvgElement DeepCopy()
        {
            return base.DeepCopy<SvgFontFace>();
        }
    }
}
