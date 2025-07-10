using System.Globalization;

namespace Svg.Transforms
{
    public sealed class SvgTranslate : SvgTransform
    {
        private float x;
        private float y;
        private Matrix matrix;
        public float X
        {
            get { return x; }
            set { this.x = value;
                matrix = null;
            }
        }

        public float Y
        {
            get { return y; }
            set { this.y = value;
                matrix = null;
            }
        }

        public override Matrix Matrix
        {
            get
            {
                if (matrix != null) return matrix;

                matrix = SvgEngine.Factory.CreateMatrix();
                matrix.Translate(this.X, this.Y);
                return matrix;
            }
        }

        public override string WriteToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "translate({0}, {1})", this.X, this.Y);
        }

        public SvgTranslate(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public SvgTranslate(float x)
            : this(x, 0.0f)
        {
        }


		public override object Clone()
		{
			return new SvgTranslate(this.x, this.y);
		}

    }
}