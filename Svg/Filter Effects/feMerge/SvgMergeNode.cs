using System;
using System.Collections.Generic;
using System.Text;


namespace Svg.FilterEffects
{

	[SvgElement("feMergeNode")]
    public class SvgMergeNode : SvgElement
    {
        private SvgAttributeCollection.Attribute<string> _in;

        [SvgAttribute("in")]
        public string Input
        {
            get { return (_in ??= this.Attributes.GetAttribute<string>("in")).GetValue(); }
            set { this.Attributes["in"] = value; }
        }

		public override SvgElement DeepCopy()
		{
		    var e = (SvgMergeNode)base.DeepCopy<SvgMergeNode>();
		    e.Input = this.Input;
		    return e;
		}

    }
}