using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Svg.Interfaces
{
    public abstract class SizeF
        : IEquatable<SizeF>
    {
        // Private height and width fields.
        private float width, height;

        // -----------------------
        // Public Shared Members
        // -----------------------

        /// <summary>
        ///	Empty Shared Field
        /// </summary>
        ///
        /// <remarks>
        ///	An uninitialized SizeF Structure.
        /// </remarks>

        public static readonly SizeF Empty = SizeF.Create();

        protected SizeF()
        {
            
        }

        public static SizeF Create()
        {
            return Create(0f, 0f);
        }

        public static SizeF Create(float x, float y)
        {
            return SvgEngine.Factory.CreateSizeF(x, y);
        }

        /// <summary>
        ///	Addition Operator
        /// </summary>
        ///
        /// <remarks>
        ///	Addition of two SizeF structures.
        /// </remarks>

        public static SizeF operator +(SizeF sz1, SizeF sz2)
        {
            return SizeF.Create(sz1.Width + sz2.Width,
                      sz1.Height + sz2.Height);
        }

        /// <summary>
        ///	Equality Operator
        /// </summary>
        ///
        /// <remarks>
        ///	Compares two SizeF objects. The return value is
        ///	based on the equivalence of the Width and Height 
        ///	properties of the two Sizes.
        /// </remarks>

        public static bool operator ==(SizeF sz1, SizeF sz2)
        {
            return ((sz1?.Width == sz2?.Width) &&
                (sz1?.Height == sz2?.Height));
        }

        /// <summary>
        ///	Inequality Operator
        /// </summary>
        ///
        /// <remarks>
        ///	Compares two SizeF objects. The return value is
        ///	based on the equivalence of the Width and Height 
        ///	properties of the two Sizes.
        /// </remarks>

        public static bool operator !=(SizeF sz1, SizeF sz2)
        {
            return ((sz1?.Width != sz2?.Width) ||
                (sz1?.Height != sz2?.Height));
        }

        /// <summary>
        ///	Subtraction Operator
        /// </summary>
        ///
        /// <remarks>
        ///	Subtracts two SizeF structures.
        /// </remarks>

        public static SizeF operator -(SizeF sz1, SizeF sz2)
        {
            return SizeF.Create(sz1.Width - sz2.Width,
                      sz1.Height - sz2.Height);
        }

        /// <summary>
        ///	SizeF to PointF Conversion
        /// </summary>
        ///
        /// <remarks>
        ///	Returns a PointF based on the dimensions of a given 
        ///	SizeF. Requires explicit cast.
        /// </remarks>

        public static explicit operator PointF(SizeF size)
        {
            return PointF.Create(size.Width, size.Height);
        }


        // -----------------------
        // Public Constructors
        // -----------------------

        /// <summary>
        ///	SizeF Constructor
        /// </summary>
        ///
        /// <remarks>
        ///	Creates a SizeF from a PointF value.
        /// </remarks>

        public SizeF(PointF pt)
        {
            width = pt.X;
            height = pt.Y;
        }

        /// <summary>
        ///	SizeF Constructor
        /// </summary>
        ///
        /// <remarks>
        ///	Creates a SizeF from an existing SizeF value.
        /// </remarks>

        public SizeF(SizeF size)
        {
            width = size.Width;
            height = size.Height;
        }

        /// <summary>
        ///	SizeF Constructor
        /// </summary>
        ///
        /// <remarks>
        ///	Creates a SizeF from specified dimensions.
        /// </remarks>

        public SizeF(float width, float height)
        {
            this.width = width;
            this.height = height;
        }

        // -----------------------
        // Public Instance Members
        // -----------------------

        /// <summary>
        ///	IsEmpty Property
        /// </summary>
        ///
        /// <remarks>
        ///	Indicates if both Width and Height are zero.
        /// </remarks>

        public bool IsEmpty
        {
            get
            {
                return ((width == 0.0) && (height == 0.0));
            }
        }

        /// <summary>
        ///	Width Property
        /// </summary>
        ///
        /// <remarks>
        ///	The Width coordinate of the SizeF.
        /// </remarks>

        public float Width
        {
            get
            {
                return width;
            }
            set
            {
                width = value;
            }
        }

        /// <summary>
        ///	Height Property
        /// </summary>
        ///
        /// <remarks>
        ///	The Height coordinate of the SizeF.
        /// </remarks>

        public float Height
        {
            get
            {
                return height;
            }
            set
            {
                height = value;
            }
        }

        /// <summary>
        ///	Equals Method
        /// </summary>
        ///
        /// <remarks>
        ///	Checks equivalence of this SizeF and another SizeF.
        /// </remarks>

        public bool Equals(SizeF other)
        {
            return ((this.Width == other.Width) &&
                (this.Height == other.Height));
        }

        /// <summary>
        ///	Equals Method
        /// </summary>
        ///
        /// <remarks>
        ///	Checks equivalence of this SizeF and another object.
        /// </remarks>

        public override bool Equals(object obj)
        {
            if (!(obj is SizeF))
                return false;

            return this.Equals((SizeF)obj);
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
            return (int)width ^ (int)height;
        }

        public PointF ToPointF()
        {
            return PointF.Create(width, height);
        }

        /// <summary>
        ///	ToString Method
        /// </summary>
        ///
        /// <remarks>
        ///	Formats the SizeF as a string in coordinate notation.
        /// </remarks>

        public override string ToString()
        {
            return string.Format("{{Width={0}, Height={1}}}", width.ToString(CultureInfo.CurrentCulture),
                height.ToString(CultureInfo.CurrentCulture));
        }
    }
}
