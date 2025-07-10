using Svg.Interfaces;

namespace Svg.Transforms
{
    public abstract class SvgTransform : ICloneable
    {
        public abstract Matrix Matrix { get; }
        public abstract string WriteToString();

        public abstract object Clone();

        public virtual void ApplyTo(Matrix other)
        {
            other.Multiply(Matrix, MatrixOrder.Prepend);
        }

        public virtual void ApplyTo(ISvgRenderer renderer)
        {
            renderer.Graphics.Concat(Matrix);
        }

        #region Equals implementation
        public override bool Equals(object obj)
        {
            SvgTransform other = obj as SvgTransform;
            if (ReferenceEquals(other, null))
                return false;

            var thisMatrix = Matrix.Elements;
            var otherMatrix = other.Matrix.Elements;

            for (int i = 0; i < 6; i++)
            {
                if (!Equals(thisMatrix[i], otherMatrix[i]))
                    return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            int hashCode = Matrix.GetHashCode();
            return hashCode;
        }


        public static bool operator ==(SvgTransform lhs, SvgTransform rhs)
        {
            if (ReferenceEquals(lhs, null))
                return ReferenceEquals(rhs, null);
            return ReferenceEquals(lhs, rhs) || lhs.Equals(rhs);
        }

        public static bool operator !=(SvgTransform lhs, SvgTransform rhs)
        {
            return !(lhs == rhs);
        }
        #endregion

        public override string ToString()
        {
            return WriteToString();
        }
    }
}