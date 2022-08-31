using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Svg.Interfaces
{
    public abstract class RectangleF
        : IEquatable<RectangleF>
    {
        private float x, y, width, height;

        /// <summary>
        ///	Empty Shared Field
        /// </summary>
        ///
        /// <remarks>
        ///	An uninitialized RectangleF Structure.
        /// </remarks>

        public static readonly RectangleF Empty = RectangleF.Create();

        protected RectangleF() { }

        /// <summary>
        ///	FromLTRB Shared Method
        /// </summary>
        ///
        /// <remarks>
        ///	Produces a RectangleF structure from left, top, right,
        ///	and bottom coordinates.
        /// </remarks>

        public static RectangleF FromLTRB(float left, float top,
                           float right, float bottom)
        {
            return SvgEngine.Factory.CreateRectangleF(left, top, right - left, bottom - top);
        }

        public static RectangleF Create()
        {
            return SvgEngine.Factory.CreateRectangleF();
        }

        public static RectangleF Create(float x, float y, float width, float height)
        {
            return SvgEngine.Factory.CreateRectangleF(x, y, width, height);
        }

        public static RectangleF Create(PointF location, SizeF size)
        {
            return SvgEngine.Factory.CreateRectangleF(location, size);
        }

        /// <summary>
        /// Creates a rectangle that includes all points (bounding box)
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        public static RectangleF FromPoints(PointF[] points)
        {
            if (points.Length == 0)
                return RectangleF.Create();

            var minX = points.Select(p => p.X).Min();
            var minY = points.Select(p => p.Y).Min();
            var maxX = points.Select(p => p.X).Max();
            var maxY = points.Select(p => p.Y).Max();

            return RectangleF.Create(minX, minY, maxX - minX, maxY - minY);
        }

        /// <summary>
        ///	Inflate Shared Method
        /// </summary>
        ///
        /// <remarks>
        ///	Produces a new RectangleF by inflating an existing 
        ///	RectangleF by the specified coordinate values.
        /// </remarks>

        public static RectangleF Inflate(RectangleF rect,
                          float x, float y)
        {
            RectangleF ir = RectangleF.Create(rect.X, rect.Y, rect.Width, rect.Height);
            ir.Inflate(x, y);
            return ir;
        }

        /// <summary>
        ///	Inflate Method
        /// </summary>
        ///
        /// <remarks>
        ///	Inflates the RectangleF by a specified width and height.
        /// </remarks>

        public void Inflate(float x, float y)
        {
            Inflate(SizeF.Create(x, y));
        }

        /// <summary>
        ///	Inflate Method
        /// </summary>
        ///
        /// <remarks>
        ///	Inflates the RectangleF by a specified Size.
        /// </remarks>

        public void Inflate(SizeF size)
        {
            x -= size.Width;
            y -= size.Height;
            width += size.Width * 2;
            height += size.Height * 2;
        }

        /// <summary>
        ///	Intersect Shared Method
        /// </summary>
        ///
        /// <remarks>
        ///	Produces a new RectangleF by intersecting 2 existing 
        ///	RectangleFs. Returns null if there is no intersection.
        /// </remarks>

        public static RectangleF Intersect(RectangleF a,
                            RectangleF b)
        {
            // MS.NET returns a non-empty rectangle if the two rectangles
            // touch each other
            if (!a.IntersectsWithInclusive(b))
                return Empty;

            return FromLTRB(
                Math.Max(a.Left, b.Left),
                Math.Max(a.Top, b.Top),
                Math.Min(a.Right, b.Right),
                Math.Min(a.Bottom, b.Bottom));
        }

        /// <summary>
        ///	Intersect Method
        /// </summary>
        ///
        /// <remarks>
        ///	Replaces the RectangleF with the intersection of itself
        ///	and another RectangleF.
        /// </remarks>

        public void Intersect(RectangleF rect)
        {
            var r = RectangleF.Intersect(this, rect);
            this.x = r.x;
            this.y = r.y;
            this.width = r.width;
            this.height = r.height;
        }

        /// <summary>
        ///	Union Shared Method
        /// </summary>
        ///
        /// <remarks>
        ///	Produces a new RectangleF from the union of 2 existing 
        ///	RectangleFs.
        /// </remarks>

        public static RectangleF Union(RectangleF a, RectangleF b)
        {
            return FromLTRB(Math.Min(a.Left, b.Left),
                     Math.Min(a.Top, b.Top),
                     Math.Max(a.Right, b.Right),
                     Math.Max(a.Bottom, b.Bottom));
        }

        /// <summary>
        ///	Equality Operator
        /// </summary>
        ///
        /// <remarks>
        ///	Compares two RectangleF objects. The return value is
        ///	based on the equivalence of the Location and Size 
        ///	properties of the two RectangleFs.
        /// </remarks>

        public static bool operator ==(RectangleF left, RectangleF right)
        {
            return (left?.X == right?.X) && (left?.Y == right?.Y) &&
                                (left?.Width == right?.Width) && (left?.Height == right?.Height);
        }

        /// <summary>
        ///	Inequality Operator
        /// </summary>
        ///
        /// <remarks>
        ///	Compares two RectangleF objects. The return value is
        ///	based on the equivalence of the Location and Size 
        ///	properties of the two RectangleFs.
        /// </remarks>

        public static bool operator !=(RectangleF left, RectangleF right)
        {
            return (left?.X != right?.X) || (left?.Y != right?.Y) ||
                                (left?.Width != right?.Width) || (left?.Height != right?.Height);
        }

        // -----------------------
        // Public Constructors
        // -----------------------

        /// <summary>
        ///	RectangleF Constructor
        /// </summary>
        ///
        /// <remarks>
        ///	Creates a RectangleF from PointF and SizeF values.
        /// </remarks>

        public RectangleF(PointF location, SizeF size)
        {
            x = location.X;
            y = location.Y;
            width = size.Width;
            height = size.Height;
        }

        /// <summary>
        ///	RectangleF Constructor
        /// </summary>
        ///
        /// <remarks>
        ///	Creates a RectangleF from a specified x,y location and
        ///	width and height values.
        /// </remarks>

        public RectangleF(float x, float y, float width, float height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }

        /// <summary>
        ///	Bottom Property
        /// </summary>
        ///
        /// <remarks>
        ///	The Y coordinate of the bottom edge of the RectangleF.
        ///	Read only.
        /// </remarks>

        public float Bottom
        {
            get
            {
                return Y + Height;
            }
        }

        /// <summary>
        ///	Height Property
        /// </summary>
        ///
        /// <remarks>
        ///	The Height of the RectangleF.
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
        ///	IsEmpty Property
        /// </summary>
        ///
        /// <remarks>
        ///	Indicates if the width or height are zero. Read only.
        /// </remarks>
        //		
        public bool IsEmpty
        {
            get
            {
                return (width <= 0 || height <= 0);
            }
        }

        /// <summary>
        ///	Left Property
        /// </summary>
        ///
        /// <remarks>
        ///	The X coordinate of the left edge of the RectangleF.
        ///	Read only.
        /// </remarks>

        public float Left
        {
            get
            {
                return X;
            }
        }

        /// <summary>
        ///	Location Property
        /// </summary>
        ///
        /// <remarks>
        ///	The Location of the top-left corner of the RectangleF.
        /// </remarks>

        public PointF Location
        {
            get
            {
                return PointF.Create(x, y);
            }
            set
            {
                x = value.X;
                y = value.Y;
            }
        }

        /// <summary>
        ///	Right Property
        /// </summary>
        ///
        /// <remarks>
        ///	The X coordinate of the right edge of the RectangleF.
        ///	Read only.
        /// </remarks>

        public float Right
        {
            get
            {
                return X + Width;
            }
        }

        /// <summary>
        ///	Size Property
        /// </summary>
        ///
        /// <remarks>
        ///	The Size of the RectangleF.
        /// </remarks>

        public SizeF Size
        {
            get
            {
                return SizeF.Create(width, height);
            }
            set
            {
                width = value.Width;
                height = value.Height;
            }
        }

        /// <summary>
        ///	Top Property
        /// </summary>
        ///
        /// <remarks>
        ///	The Y coordinate of the top edge of the RectangleF.
        ///	Read only.
        /// </remarks>

        public float Top
        {
            get
            {
                return Y;
            }
        }

        /// <summary>
        ///	Width Property
        /// </summary>
        ///
        /// <remarks>
        ///	The Width of the RectangleF.
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
        ///	X Property
        /// </summary>
        ///
        /// <remarks>
        ///	The X coordinate of the RectangleF.
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
        ///	The Y coordinate of the RectangleF.
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
        ///	Contains Method
        /// </summary>
        ///
        /// <remarks>
        ///	Checks if an x,y coordinate lies within this RectangleF.
        /// </remarks>

        public bool Contains(float x, float y)
        {
            return ((x >= Left) && (x < Right) &&
                (y >= Top) && (y < Bottom));
        }

        /// <summary>
        ///	Contains Method
        /// </summary>
        ///
        /// <remarks>
        ///	Checks if a Point lies within this RectangleF.
        /// </remarks>

        public bool Contains(PointF pt)
        {
            return Contains(pt.X, pt.Y);
        }

        /// <summary>
        ///	Contains Method
        /// </summary>
        ///
        /// <remarks>
        ///	Checks if a RectangleF lies entirely within this 
        ///	RectangleF.
        /// </remarks>

        public bool Contains(RectangleF rect)
        {
            return (rect == Intersect(this, rect));
        }

        /// <summary>
        ///	Equals Method
        /// </summary>
        ///
        /// <remarks>
        ///	Checks equivalence of this RectangleF and an RectangleF.
        /// </remarks>

        public bool Equals(RectangleF other)
        {
            return ((this.Location == other.Location) &&
                (this.Size == other.Size));
        }

        /// <summary>
        ///	Equals Method
        /// </summary>
        ///
        /// <remarks>
        ///	Checks equivalence of this RectangleF and an object.
        /// </remarks>

        public override bool Equals(object obj)
        {
            if (!(obj is RectangleF))
                return false;

            return this.Equals((RectangleF)obj);
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
            return (int)(x + y + width + height);
        }

        /// <summary>
        ///	IntersectsWith Method
        /// </summary>
        ///
        /// <remarks>
        ///	Checks if a RectangleF intersects with this one.
        /// </remarks>

        public bool IntersectsWith(RectangleF rect)
        {
            return !((Left >= rect.Right) || (Right <= rect.Left) ||
                (Top >= rect.Bottom) || (Bottom <= rect.Top));
        }

        private bool IntersectsWithInclusive(RectangleF r)
        {
            return !((Left > r.Right) || (Right < r.Left) ||
                (Top > r.Bottom) || (Bottom < r.Top));
        }

        /// <summary>
        ///	Offset Method
        /// </summary>
        ///
        /// <remarks>
        ///	Moves the RectangleF a specified distance.
        /// </remarks>

        public void Offset(float x, float y)
        {
            X += x;
            Y += y;
        }

        /// <summary>
        ///	Offset Method
        /// </summary>
        ///
        /// <remarks>
        ///	Moves the RectangleF a specified distance.
        /// </remarks>

        public void Offset(PointF pos)
        {
            Offset(pos.X, pos.Y);
        }

        /// <summary>
        ///	ToString Method
        /// </summary>
        ///
        /// <remarks>
        ///	Formats the RectangleF in (x,y,w,h) notation.
        /// </remarks>

        public override string ToString()
        {
            return $"<rect x=\"{x}\" y=\"{y}\" width=\"{width}\" height=\"{height}\" style=\"fill:black\" />";
            //return String.Format("{{X={0},Y={1},Width={2},Height={3}}}",
            //             x, y, width, height);
        }

        public RectangleF UnionAndCopy(RectangleF childBounds)
        {
            var newRect = RectangleF.Create(this.x, this.y, this.width, this.height);

            return Union(newRect, childBounds);
        }
        
        public RectangleF InflateAndCopy(float x, float y)
        {
            var newRect = RectangleF.Create(this.x, this.y, this.width, this.height);
            newRect.Inflate(x, y);
            return newRect;
        }

    }
}
