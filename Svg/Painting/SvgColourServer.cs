using System;
using Svg.Interfaces;

namespace Svg
{
    public sealed class SvgColourServer : SvgPaintServer
    {
    	
    	/// <summary>
        /// An unspecified <see cref="SvgPaintServer"/>.
        /// </summary>
        public static readonly SvgPaintServer NotSet = new SvgColourServer(SvgEngine.Factory.Colors.Black);
        /// <summary>
        /// A <see cref="SvgPaintServer"/> that should inherit from its parent.
        /// </summary>
        public static readonly SvgPaintServer Inherit = new SvgColourServer(SvgEngine.Factory.Colors.Black);
        /// <summary>
        /// A <see cref="SvgPaintServer"/> that should get the stroke color from its context object (e.g. marker or svg use)
        /// </summary>
        public static readonly SvgPaintServer ContextStroke = new SvgColourServer(SvgEngine.Factory.Colors.Black);
        /// <summary>
        /// A <see cref="SvgPaintServer"/> that should get the fill color from its context object (e.g. marker or svg use)
        /// </summary>
        public static readonly SvgPaintServer ContextFill = new SvgColourServer(SvgEngine.Factory.Colors.Black);

        public SvgColourServer()
            : this(SvgEngine.Factory.Colors.Black)
        {
        }

        public SvgColourServer(Color colour)
        {
            _colour = colour;
        }

        private Color _colour;

        public Color Colour
        {
            get { return _colour; }
            set { _colour = value; }
        }

        public override Brush GetBrush(SvgVisualElement styleOwner, ISvgRenderer renderer, float opacity, bool forStroke = false)
        {
            //is none?
            if (this == None) return SvgEngine.Factory.CreateSolidBrush(SvgEngine.Factory.Colors.Transparent);
                
            int alpha = (int)((opacity * (Colour.A/255.0f) ) * 255);
            Color colour = SvgEngine.Factory.CreateColorFromArgb(alpha, Colour);

            return SvgEngine.Factory.CreateSolidBrush(colour);
        }

        public override string ToString()
        {
        	if(this == None)
        		return "none";
            if(this == NotSet)
                return "";
            if (this == Inherit)
                return "inherit";
            if (this == ContextFill)
                return "context-fill";
            if (this == ContextStroke)
                return "context-stroke";

            Color c = Colour;

            // Return the name if it exists
            if (c.IsKnownColor)
            {
                return c.Name;
            }

            // Return the hex value
            return String.Format("#{0}", c.ToArgb().ToString("x").Substring(2));
        }


		public override SvgElement DeepCopy()
		{
			return DeepCopy<SvgColourServer>();
		}


		public override SvgElement DeepCopy<T>()
		{
			var newObj = base.DeepCopy<T>() as SvgColourServer;
			newObj.Colour = Colour;
			return newObj;

		}

        public override bool Equals(object obj)
        {
            var objColor = obj as SvgColourServer;
            if (objColor == null)
                return false;

            if ((this == None && obj != None) ||
                (this != None && obj == None) ||
                (this == NotSet && obj != NotSet) ||
                (this != NotSet && obj == NotSet) ||
                (this == Inherit && obj != Inherit) ||
                (this != Inherit && obj == Inherit) ||
                (this == ContextFill && obj != ContextFill) ||
                (this != ContextFill && obj == ContextFill) ||
                (this == ContextStroke && obj != ContextStroke) ||
                (this != ContextStroke && obj == ContextStroke)) return false;

            return GetHashCode() == objColor.GetHashCode();
        }

        public override int GetHashCode()
        {
            return _colour?.GetHashCode() ?? base.GetHashCode();
        }
    }
}
