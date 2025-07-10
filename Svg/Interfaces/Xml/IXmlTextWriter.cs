using System;

namespace Svg.Interfaces.Xml
{
    public interface IXmlTextWriter : IDisposable
    {
        void WriteAttributeString(string localName, string value);
        void WriteAttributeString(string localName, string namespaceKey, string value);
        void WriteDocType(string name, string pubid, string sysid, string subset);
        void WriteProcessingInstruction(string name, string text);
        void Flush();
        void WriteStartElement(string elementName);
        void WriteStartElement(string elementName, string ns);
        void WriteStartElement(string elementName, string nsPrefix, string ns);
        void WriteEndElement();
        void WriteString(string content);
        void WriteRaw(string content);
        void WriteStartDocument();
    }
}
