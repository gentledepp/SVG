using System.Linq;

namespace Svg
{
    [SvgElement("font")]
    public class SvgFont : SvgElement
    {
        private SvgAttributeCollection.InheritedAttribute<object> _horizAdvX;
        private SvgAttributeCollection.InheritedAttribute<object> _horizOriginX;
        private SvgAttributeCollection.InheritedAttribute<object> _horizOriginY;
        private SvgAttributeCollection.InheritedAttribute<object> _vertAdvY;
        private SvgAttributeCollection.InheritedAttribute<object> _vertOriginX;
        private SvgAttributeCollection.InheritedAttribute<object> _vertOriginY;

        [SvgAttribute("horiz-adv-x")]
        public float HorizAdvX
        {
            get { return (_horizAdvX ??= this.Attributes.GetInheritedAttribute<object>("horiz-adv-x")).GetValue() is { } val ? (float)val: 0; }
            set { this.Attributes["horiz-adv-x"] = value; }
        }
        [SvgAttribute("horiz-origin-x")]
        public float HorizOriginX
        {
            get { return (_horizOriginX ??= this.Attributes.GetInheritedAttribute<object>("horiz-origin-x")).GetValue() is { } val ? (float)val: 0; }
            set { this.Attributes["horiz-origin-x"] = value; }
        }
        [SvgAttribute("horiz-origin-y")]
        public float HorizOriginY
        {
            get { return (_horizOriginY ??= this.Attributes.GetInheritedAttribute<object>("horiz-origin-y")).GetValue() is { } val ? (float)val: 0; }
            set { this.Attributes["horiz-origin-y"] = value; }
        }
        [SvgAttribute("vert-adv-y")]
        public float VertAdvY
        {
            get { return (_vertAdvY ??= this.Attributes.GetInheritedAttribute<object>("vert-adv-y")).GetValue()is { } val ? (float)val: this.Children.OfType<SvgFontFace>().First().UnitsPerEm; }
            set { this.Attributes["vert-adv-y"] = value; }
        }
        [SvgAttribute("vert-origin-x")]
        public float VertOriginX
        {
            get { return (_vertOriginX ??= this.Attributes.GetInheritedAttribute<object>("vert-origin-x")).GetValue() is { } val ? (float)val: this.HorizAdvX / 2; }
            set { this.Attributes["vert-origin-x"] = value; }
        }
        [SvgAttribute("vert-origin-y")]
        public float VertOriginY
        {
            get { return (_vertOriginY ??= this.Attributes.GetInheritedAttribute<object>("vert-origin-y")).GetValue() is { } val 
                ? (float)val 
                : (this.Children.OfType<SvgFontFace>().First().Attributes["ascent"] == null ? 0 : this.Children.OfType<SvgFontFace>().First().Ascent); }
            set { this.Attributes["vert-origin-y"] = value; }
        }

        public override SvgElement DeepCopy()
        {
            return base.DeepCopy<SvgFont>();
        }
    }
}
