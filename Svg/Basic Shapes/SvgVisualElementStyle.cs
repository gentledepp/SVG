using System;
using System.Collections.Generic;

using System.Text;
using System.Reflection;
using System.ComponentModel;
using Svg.DataTypes;
using System.Text.RegularExpressions;
using System.Linq;

namespace Svg
{
    public abstract partial class SvgVisualElement
    {
        private SvgAttributeCollection.InheritedAttribute<bool?> _visible;
        private SvgAttributeCollection.InheritedAttribute<string> _display;
        private SvgAttributeCollection.InheritedAttribute<string> _enableBackground;

        /// <summary>
        /// Gets or sets a value to determine whether the element will be rendered.
        /// </summary>
        //[TypeConverter(typeof(SvgBoolConverter))]
        [SvgAttribute("visibility")]
        public virtual bool Visible
        {
            get { 
                _visible??=this.Attributes.GetInheritedAttribute<bool?>("visibility");
                var v = _visible.GetValue();
                return  !v.HasValue || (bool)v.Value;
            }
            set { this.Attributes["visibility"] = value; }
        }

        /// <summary>
        /// Gets or sets a value to determine whether the element will be rendered.
        /// Needed to support SVG attribute display="none"
        /// </summary>
        [SvgAttribute("display")]
        public virtual string Display
        {
            get { return (_display ??=this.Attributes.GetInheritedAttribute<string>("display")).GetValue(); }
            set { this.Attributes["display"] = value; }
        }

        // Displayable - false if attribute display="none", true otherwise
        public virtual bool Displayable
        {
            get
            {
                string checkForDisplayNone = Display;
                if ((!string.IsNullOrEmpty(checkForDisplayNone)) && (checkForDisplayNone == "none"))
                    return false;
                else
                    return true;
            }
        }

        /// <summary>
        /// Gets or sets the fill <see cref="SvgPaintServer"/> of this element.
        /// </summary>
        [SvgAttribute("enable-background")]
        public virtual string EnableBackground
        {
            get { return (_enableBackground ??=this.Attributes.GetInheritedAttribute<string>("enable-background")).GetValue(); }
            set { this.Attributes["enable-background"] = value; }
        }

    }
}