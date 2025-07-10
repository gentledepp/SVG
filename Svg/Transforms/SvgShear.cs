using System.Globalization;

namespace Svg.Transforms
{
    /// <summary>
    /// The class which applies the specified shear vector to this Matrix.
    /// </summary>
    public sealed class SvgShear : SvgTransform
    {
        private float shearFactorX;
        private float shearFactorY;
        private Matrix matrix;

        public float X
        {
            get { return this.shearFactorX; }
            set { this.shearFactorX = value;
                matrix = null;
            }
        }

        public float Y
        {
            get { return this.shearFactorY; }
            set { this.shearFactorY = value;
                matrix = null;
            }
        }

        public override Matrix Matrix
        {
            get
            {
                if (matrix != null)
                    return matrix;
                matrix = SvgEngine.Factory.CreateMatrix();
                matrix.Shear(this.X, this.Y);
                return matrix;
            }
        }

        public override string WriteToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "shear({0}, {1})", this.X, this.Y);
        }

        public SvgShear(float x) : this(x, x) { }

        public SvgShear(float x, float y)
        {
            this.shearFactorX = x;
            this.shearFactorY = y;
        }


		public override object Clone()
		{
			return new SvgShear(this.X, this.Y);
		}
    }
}