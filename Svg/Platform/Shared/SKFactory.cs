using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using Svg.Interfaces;
using Svg.Interfaces.Xml;

namespace Svg
{
    public class SKFactory : SKFactoryBase
    {
        public override ISortedList<TKey, TValue> CreateSortedList<TKey, TValue>()
        {
            return new SvgSortedList<TKey, TValue>();
        }

        public override IXmlTextWriter CreateXmlTextWriter(StringWriter writer)
        {
            return new SvgXmlTextWriter(writer);
        }

        public override IXmlTextWriter CreateXmlTextWriter(Stream stream, Encoding utf8)
        {
            var w = new SvgXmlTextWriter(stream, utf8);
            w.Formatting = Formatting.None;
            return w;
        }
        public override IXmlReader CreateSvgTextReader(Stream stream, Dictionary<string, string> entities)
        {
            return new SvgXmlReader(stream, entities);
        }

        public override IXmlReader CreateSvgTextReader(StringReader r, Dictionary<string, string> entities)
        {
            return new SvgXmlReader(r, entities);
        }

    }
}
