using System.IO;
using System.Text;
using System.Xml;
using Svg.Interfaces.Xml;

#if WINDOWS_UWP
using XmlTextWriter = System.Xml.XmlWriter;

namespace System.Xml
{
  /// <summary>To be added.</summary>
  /// <remarks>To be added.</remarks>
  public enum Formatting
  {
    None,
    Indented,
  }
}
#endif

namespace Svg
{
    public sealed class SvgXmlTextWriter : IXmlTextWriter
    {
        private XmlTextWriter _w;

        public SvgXmlTextWriter(Stream stream, Encoding encoding)
        {
#if WINDOWS_UWP
            _w = XmlWriter.Create(stream, new XmlWriterSettings() {Encoding = encoding});
#else
            _w = new XmlTextWriter(stream, encoding);
#endif
        }

        public SvgXmlTextWriter(StringWriter writer)
        {
#if WINDOWS_UWP
            _w = XmlWriter.Create(writer);
#else
            _w = new XmlTextWriter(writer);
#endif
        }

#if WINDOWS_UWP
        public Formatting Formatting { get { return _w.Settings.Indent?Formatting.Indented:Formatting.None; } set { /*_w.Settings.Indent = (value == Formatting.Indented);*/ } }

#else
        public Formatting Formatting { get { return _w.Formatting; } set { _w.Formatting = value; } }
#endif

        public void WriteAttributeString(string localName, string value)
        {
            _w.WriteAttributeString(localName, value);
        }

        public void WriteAttributeString(string localName, string namespaceKey, string value)
        {
            _w.WriteAttributeString(namespaceKey, localName, null, value);
        }

        public void WriteDocType(string name, string pubid, string sysid, string subset)
        {
            _w.WriteDocType(name, pubid, sysid, subset);
        }

        public void WriteProcessingInstruction(string name, string text)
        {
            _w.WriteProcessingInstruction(name, text);
        }

        public void Flush()
        {
            _w.Flush();
        }

        public void WriteStartElement(string elementName)
        {
            _w.WriteStartElement(elementName);
        }

        public void WriteStartElement(string elementName, string ns)
        {
            _w.WriteStartElement(elementName, ns);
        }
        
        public void WriteStartElement(string elementName, string prefix, string ns)
        {
            _w.WriteStartElement(prefix, elementName, ns);
        }

        public void WriteEndElement()
        {
            _w.WriteEndElement();
        }

        public void WriteString(string content)
        {
            if (string.IsNullOrWhiteSpace(content) && content != null)
                _w.WriteWhitespace(content);
            else if(content != null)
                _w.WriteString(content);
        }

        public void WriteRaw(string content)
        {
            _w.WriteRaw(content);
        }

        public void WriteStartDocument()
        {
            _w.WriteStartDocument(false);
        }

        public void Dispose()
        {
            _w.Dispose();
        }
    }
}