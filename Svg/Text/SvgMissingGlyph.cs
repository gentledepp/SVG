using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Svg
{
    [SvgElement("missing-glyph")]
    public class SvgMissingGlyph : SvgGlyph
    {
        [SvgAttribute("glyph-name")]
        public override string GlyphName
        {
            get { return (_glyphName ??= this.Attributes.GetInheritedAttribute<object>("glyph-name")).GetValue() as string ?? "__MISSING_GLYPH__"; }
            set { this.Attributes["glyph-name"] = value; }
        }
    }
}
