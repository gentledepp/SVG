using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Svg
{
    public abstract class SvgKern : SvgElement
    {
        private SvgAttributeCollection.InheritedAttribute<object> _g1;
        private SvgAttributeCollection.InheritedAttribute<object> _g2;
        private SvgAttributeCollection.InheritedAttribute<object> _u1;
        private SvgAttributeCollection.InheritedAttribute<object> _u2;
        private SvgAttributeCollection.InheritedAttribute<object> _k;

        [SvgAttribute("g1")]
        public string Glyph1
        {
            get { return (_g1 ??= this.Attributes.GetInheritedAttribute<object>("g1")).GetValue() as string; }
            set { this.Attributes["g1"] = value; }
        }
        [SvgAttribute("g2")]
        public string Glyph2
        {
            get { return (_g2 ??= this.Attributes.GetInheritedAttribute<object>("g2")).GetValue() as string; }
            set { this.Attributes["g2"] = value; }
        }
        [SvgAttribute("u1")]
        public string Unicode1
        {
            get { return (_u1 ??= this.Attributes.GetInheritedAttribute<object>("u1")).GetValue() as string; }
            set { this.Attributes["u1"] = value; }
        }
        [SvgAttribute("u2")]
        public string Unicode2
        {
            get { return (_u2 ??= this.Attributes.GetInheritedAttribute<object>("u2")).GetValue() as string; }
            set { this.Attributes["u2"] = value; }
        }
        [SvgAttribute("k")]
        public float Kerning
        {
            get { return (_k ??= this.Attributes.GetInheritedAttribute<object>("k")).GetValue() is { } val ? (float)val: 0; }
            set { this.Attributes["k"] = value; }
        }
    }

    [SvgElement("vkern")]
    public class SvgVerticalKern : SvgKern
    {
        public override SvgElement DeepCopy()
        {
            return base.DeepCopy<SvgVerticalKern>();
        }
    }
    [SvgElement("hkern")]
    public class SvgHorizontalKern : SvgKern
    {
        public override SvgElement DeepCopy()
        {
            return base.DeepCopy<SvgHorizontalKern>();
        }
    }
}
