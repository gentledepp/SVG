using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml;
using Svg.Interfaces.Xml;

namespace Svg.Interfaces
{
    /// <summary>
    /// Contains information about a type inheriting from <see cref="SvgElement"/>.
    /// </summary>
    [DebuggerDisplay("{ElementName}, {ElementType}")]
    public class ElementInfo
    {
        /// <summary>
        /// Gets the SVG name of the <see cref="SvgElement"/>.
        /// </summary>
        public string ElementName { get; set; }
        /// <summary>
        /// Gets the <see cref="Type"/> of the <see cref="SvgElement"/> subclass.
        /// </summary>
        public Type ElementType { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ElementInfo"/> struct.
        /// </summary>
        /// <param name="elementName">Name of the element.</param>
        /// <param name="elementType">Type of the element.</param>
        public ElementInfo(string elementName, Type elementType)
        {
            this.ElementName = elementName;
            this.ElementType = elementType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ElementInfo"/> class.
        /// </summary>
        public ElementInfo()
        {
        }
    }
    public interface ISvgElementFactory
    {
        Dictionary<string, ElementInfo> AvailableElements { get; }
        void SetPropertyValue(SvgElement svgElement, string key, string value, SvgDocument ownerDocument);
        SvgElement CreateElement(IXmlReader reader, SvgDocument svgDocument);
        T CreateDocument<T>(IXmlReader reader) where T : SvgDocument, new();
        bool IsStyleAttribute(string name);
    }
}
