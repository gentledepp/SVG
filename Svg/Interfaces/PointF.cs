using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Svg.Interfaces
{
    [DebuggerDisplay("{DebuggerDisplay}")]
    public abstract class PointF
        : IEquatable<PointF>
    {
        // Private x and y coordinate fields.
        private float x, y;

        public static PointF Create()
        {
            return Create(0f, 0f);
        }

        public static PointF Create(float x, float y)
        {
            return Svg.SvgEngine.Factory.CreatePointF(x, y);
        }

        // -----------------------
        // Public Shared Members
        // -----------------------

        protected PointF()
        {

        }

        /// <summary>
        ///	Empty Shared Field
        /// </summary>
        ///
        /// <remarks>
        ///	An uninitialized PointF Structure.
        /// </remarks>

        public static readonly PointF Empty;

        /// <summary>
        ///	Addition Operator
        /// </summary>
        ///
        /// <remarks>
        ///	Translates a PointF using the Width and Height
        ///	properties of the given Size.
        /// </remarks>

        public static PointF operator +(PointF pt, SizeF sz)
        {
            return PointF.Create(pt.X + sz.Width, pt.Y + sz.Height);
        }

        public static PointF operator +(PointF pt, PointF sz)
        {
            return PointF.Create(pt.X + sz.X, pt.Y + sz.Y);
        }

        /// <summary>
        ///	Equality Operator
        /// </summary>
        ///
        /// <remarks>
        ///	Compares two PointF objects. The return value is
        ///	based on the equivalence of the X and Y properties 
        ///	of the two points.
        /// </remarks>

        public static bool operator ==(PointF left, PointF right)
        {
            return ((left?.X == right?.X) && (left?.Y == right?.Y));
        }

        /// <summary>
        ///	Inequality Operator
        /// </summary>
        ///
        /// <remarks>
        ///	Compares two PointF objects. The return value is
        ///	based on the equivalence of the X and Y properties 
        ///	of the two points.
        /// </remarks>

        public static bool operator !=(PointF left, PointF right)
        {
            return ((left?.X != right?.X) || (left?.Y != right?.Y));
        }

        public static PointF operator /(PointF pt, float k)
        {
            return Create(pt.X / k, pt.Y / k);
        }

        public static PointF operator *(PointF pt, float k)
        {
            return Create(pt.X * k, pt.Y * k);
        }

        public static PointF operator -(PointF pt)
        {
            return Create(-pt.X, -pt.Y);
        }

        /// <summary>
        ///	Subtraction Operator
        /// </summary>
        ///
        /// <remarks>
        ///	Translates a PointF using the negation of the Width 
        ///	and Height properties of the given Size.
        /// </remarks>

        public static PointF operator -(PointF pt, SizeF sz)
        {
            return PointF.Create(pt.X - sz.Width, pt.Y - sz.Height);
        }
        /// <summary>
        ///	Subtraction Operator
        /// </summary>
        ///
        /// <remarks>
        ///	Translates a PointF using the negation of the Width 
        ///	and Height properties of the given Size.
        /// </remarks>

        public static PointF operator -(PointF pt, PointF sz)
        {
            return PointF.Create(pt.X - sz.X, pt.Y - sz.Y);
        }

        // -----------------------
        // Public Constructor
        // -----------------------

        /// <summary>
        ///	PointF Constructor
        /// </summary>
        ///
        /// <remarks>
        ///	Creates a PointF from a specified x,y coordinate pair.
        /// </remarks>

        public PointF(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        // -----------------------
        // Public Instance Members
        // -----------------------

        /// <summary>
        ///	IsEmpty Property
        /// </summary>
        ///
        /// <remarks>
        ///	Indicates if both X and Y are zero.
        /// </remarks>

        public bool IsEmpty
        {
            get
            {
                return ((x == 0.0) && (y == 0.0));
            }
        }

        /// <summary>
        ///	X Property
        /// </summary>
        ///
        /// <remarks>
        ///	The X coordinate of the PointF.
        /// </remarks>

        public float X
        {
            get
            {
                return x;
            }
            set
            {
                x = value;
            }
        }

        /// <summary>
        ///	Y Property
        /// </summary>
        ///
        /// <remarks>
        ///	The Y coordinate of the PointF.
        /// </remarks>

        public float Y
        {
            get
            {
                return y;
            }
            set
            {
                y = value;
            }
        }

        /// <summary>
        ///	Equals Method
        /// </summary>
        ///
        /// <remarks>
        ///	Checks equivalence of this PointF and another PointF.
        /// </remarks>

        public bool Equals(PointF other)
        {
            if (other == null)
                return false;

            return ((this.X == other.X) && (this.Y == other.Y));
        }

        /// <summary>
        ///	Equals Method
        /// </summary>
        ///
        /// <remarks>
        ///	Checks equivalence of this PointF and another object.
        /// </remarks>

        public override bool Equals(object obj)
        {
            if (!(obj is PointF))
                return false;

            return this.Equals((PointF)obj);
        }

        /// <summary>
        ///	GetHashCode Method
        /// </summary>
        ///
        /// <remarks>
        ///	Calculates a hashing value.
        /// </remarks>

        public override int GetHashCode()
        {
            return (int)x ^ (int)y;
        }

        /// <summary>
        ///	ToString Method
        /// </summary>
        ///
        /// <remarks>
        ///	Formats the PointF as a string in coordinate notation.
        /// </remarks>

        public override string ToString()
        {
            return String.Format("{{X={0}, Y={1}}}", x.ToString(CultureInfo.CurrentCulture),
                y.ToString(CultureInfo.CurrentCulture));
        }

        public string DebuggerDisplay => $"{X} {Y}";

        public PointF Clone()
        {
            return PointF.Create(this.X, this.Y);
        }
    }
}
