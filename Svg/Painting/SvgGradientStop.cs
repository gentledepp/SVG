using System;
using System.Collections.Generic;
using System.Text;

using System.ComponentModel;
using Svg.Interfaces;

namespace Svg
{
    /// <summary>
    /// Represents a colour stop in a gradient.
    /// </summary>
    [SvgElement("stop")]
    public class SvgGradientStop : SvgElement
    {
        private SvgUnit _offset;
        private SvgAttributeCollection.InheritedAttribute<object> _stopColor;
        private SvgAttributeCollection.InheritedAttribute<string> _opacity;

        /// <summary>
        /// Gets or sets the offset, i.e. where the stop begins from the beginning, of the gradient stop.
        /// </summary>
        [SvgAttribute("offset")]
        public SvgUnit Offset
        {
            get { return this._offset; }
            set
            {
                SvgUnit unit = value;

                if (value.Type == SvgUnitType.Percentage)
                {
                    if (value.Value > 100)
                    {
                        unit = new SvgUnit(value.Type, 100);
                    }
                    else if (value.Value < 0)
                    {
                        unit = new SvgUnit(value.Type, 0);
                    }
                }
                else if (value.Type == SvgUnitType.User)
                {
                    if (value.Value > 1)
                    {
                        unit = new SvgUnit(value.Type, 1);
                    }
                    else if (value.Value < 0)
                    {
                        unit = new SvgUnit(value.Type, 0);
                    }
                }

                this._offset = unit.ToPercentage();
                this.Attributes["offset"] = this._offset;
            }
        }

        /// <summary>
        /// Gets or sets the colour of the gradient stop.
        /// </summary>
        [SvgAttribute("stop-color")]
        //[TypeConverter(typeof(SvgPaintServerFactory))]
        public new SvgPaintServer StopColor
        {
            get 
            {
                var value = (_stopColor ??= this.Attributes.GetInheritedAttribute<object>("stop-color")).GetValue();
                return value as SvgPaintServer ?? SvgColourServer.NotSet;
            }
            set { this.Attributes["stop-color"] = value; }
        }

        /// <summary>
        /// Gets or sets the opacity of the gradient stop (0-1).
        /// </summary>
        [SvgAttribute("stop-opacity")]
        public new string Opacity
        {
            get { return (_opacity ??= this.Attributes.GetInheritedAttribute<string>("stop-opacity")).GetValue(); }
            set { this.Attributes["stop-opacity"] = value; }
        }

        public float GetOpacity()
        {
            var opacity = this.Opacity;
            return string.IsNullOrEmpty(opacity) ? 1.0f : float.Parse(opacity);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SvgGradientStop"/> class.
        /// </summary>
        public SvgGradientStop()
        {
            this._offset = new SvgUnit(0.0f);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SvgGradientStop"/> class.
        /// </summary>
        /// <param name="offset">The offset.</param>
        /// <param name="colour">The colour.</param>
        public SvgGradientStop(SvgUnit offset, Color colour)
        {
            this._offset = offset;
        }

        public Color GetColor(SvgElement parent)
        {
            var core = SvgDeferredPaintServer.TryGet<SvgColourServer>(this.StopColor, parent);
            if (core == null) throw new InvalidOperationException("Invalid paint server for gradient stop detected.");
            return core.Colour;
        }

		public override SvgElement DeepCopy()
		{
			return DeepCopy<SvgGradientStop>();
		}

		public override SvgElement DeepCopy<T>()
		{
			var newObj = base.DeepCopy<T>() as SvgGradientStop;
			newObj.Offset = this.Offset;
			return newObj;
		}
    }
}