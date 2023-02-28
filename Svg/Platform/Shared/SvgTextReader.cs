using System;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using Svg.Interfaces.Xml;

namespace Svg
{
    public sealed class SvgXmlReader : IXmlReader
    { 
        private readonly XmlReader _xml;
        private Dictionary<string, string> _entities;
        private string _value;
        private bool _customValue = false;
        private string _localName;

        public SvgXmlReader(Stream stream, Dictionary<string, string> entities)
        {
            var settings = new XmlReaderSettings()
            {
#if !WINDOWS_UWP
                XmlResolver = new SvgDtdResolver(),
#endif
                IgnoreWhitespace = false,
                DtdProcessing = DtdProcessing.Ignore,
                //WhitespaceHandling = WhitespaceHandling.Significant,
            };
            if (stream.Length == stream.Position)
                throw new InvalidOperationException("The provided streams position is at EOF! Cannot load svg document");
            _xml = XmlReader.Create(stream, settings);
            _entities = entities;
        }

        public SvgXmlReader(TextReader reader, Dictionary<string, string> entities)
        {
            var settings = new XmlReaderSettings()
            {
#if !WINDOWS_UWP
                XmlResolver = new SvgDtdResolver(),
#endif
                IgnoreWhitespace = false,
                DtdProcessing = DtdProcessing.Ignore,
                //WhitespaceHandling = WhitespaceHandling.Significant,
            };
            _xml = XmlReader.Create(reader, settings);
            _entities = entities;
        }

        public bool IsEmptyElement => _xml.IsEmptyElement;

        /// <summary>
        /// Gets the text value of the current node.
        /// </summary>
        /// <value></value>
        /// <returns>The value returned depends on the <see cref="P:System.Xml.XmlTextReader.NodeType"/> of the node. The following table lists node types that have a value to return. All other node types return String.Empty.Node Type Value AttributeThe value of the attribute. CDATAThe content of the CDATA section. CommentThe content of the comment. DocumentTypeThe internal subset. ProcessingInstructionThe entire content, excluding the target. SignificantWhitespaceThe white space within an xml:space= 'preserve' scope. TextThe content of the text node. WhitespaceThe white space between markup. XmlDeclarationThe content of the declaration. </returns>
        public string Value
        {
            get
            {
                return (this._customValue) ? this._value : _xml.Value;
            }
        }

        /// <summary>
        /// Gets the local name of the current node.
        /// </summary>
        /// <value></value>
        /// <returns>The name of the current node with the prefix removed. For example, LocalName is book for the element &lt;bk:book&gt;.For node types that do not have a name (like Text, Comment, and so on), this property returns String.Empty.</returns>
        public string LocalName
        {
            get
            {
                return (this._customValue) ? this._localName : _xml.LocalName;
            }
        }

        public string NamespaceURI => _xml.NamespaceURI;
        public string Name => _xml.Name;

        private IDictionary<string, string> Entities
        {
            get
            {
                if (this._entities == null)
                {
                    _entities = new Dictionary<string, string>();
                }

                return this._entities;
            }
        }

        /// <summary>
        /// Moves to the next attribute.
        /// </summary>
        /// <returns>
        /// true if there is a next attribute; false if there are no more attributes.
        /// </returns>
        public bool MoveToNextAttribute()
        {
            bool moved = _xml.MoveToNextAttribute();

            if (moved)
            {
                this._localName = _xml.LocalName;

                if (_xml.ReadAttributeValue())
                {
                    if (this.NodeType == XmlNodeType.EntityReference)
                    {
                        this.ResolveEntity();
                    }
                    else
                    {
                        this._value = _xml.Value;
                    }
                }
                this._customValue = true;
            }

            return moved;
        }

        /// <summary>
        /// Reads the next node from the stream.
        /// </summary>
        /// <returns>
        /// true if the next node was read successfully; false if there are no more nodes to read.
        /// </returns>
        /// <exception cref="T:System.Xml.XmlException">An error occurred while parsing the XML. </exception>
        public bool Read()
        {
            this._customValue = false;
            bool read = _xml.Read();

            if (this.NodeType == XmlNodeType.DocumentType)
            {
                this.ParseEntities();
            }

            return read;
        }

        public XmlNodeType NodeType => _xml.NodeType;

        private void ParseEntities()
        {
            const string entityText = "<!ENTITY";
            string[] entities = this.Value.Split(new string[]{entityText}, StringSplitOptions.None);
            string name = null;
            string value = null;
            int quoteIndex;

            foreach (string entity in entities)
            {
                if (string.IsNullOrEmpty(entity.Trim()))
                {
                    continue;
                }

                name = entity.Trim();
                quoteIndex = name.IndexOf(QuoteChar);
                if (quoteIndex > 0)
                {
                    value = name.Substring(quoteIndex + 1, name.LastIndexOf(QuoteChar) - quoteIndex - 1);
                    name = name.Substring(0, quoteIndex).Trim();
                    this.Entities.Add(name, value);
                }
            }
        }

        /// <summary>
        /// Resolves the entity reference for EntityReference nodes.
        /// </summary>
        public void ResolveEntity()
        {
            if (this.NodeType == XmlNodeType.EntityReference)
            {
                if (this._entities.TryGetValue(this.Name, out var v))
                {
                    this._value = v;
                }
                else
                {
                    this._value = string.Empty;
                }

                this._customValue = true;
            }
        }

        public void Dispose()
        {
            _xml.Dispose();
        }

#if WINDOWS_UWP
        private char QuoteChar => '"';
#else
        private char QuoteChar => _xml.QuoteChar;
#endif
    }
}