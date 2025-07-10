using System;
using System.Xml;

namespace Svg.Interfaces.Xml
{
    public interface IXmlReader : IDisposable
    {
        bool Read();
        XmlNodeType NodeType { get; }
        bool IsEmptyElement { get;  }
        string Value { get; }
        string LocalName { get; }
        string NamespaceURI { get; }
        string Name { get; }
        void ResolveEntity();
        bool MoveToNextAttribute();
    }
}
