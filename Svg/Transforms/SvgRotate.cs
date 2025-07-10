
using System.Globalization;

namespace Svg.Transforms
{
    public sealed class SvgRotate : SvgTransform
    {
        private Matrix matrix;
        private float _angle;
        private float _centerX;
        private float _centerY;

        public float Angle
        {
            get { return _angle; }
            set
            {
                _angle = value;
                matrix = null;
            }
        }

        public float CenterX
        {
            get { return _centerX; }
            set { _centerX = value;
                matrix = null;
            }
        }

        public float CenterY
        {
            get { return _centerY; }
            set { _centerY = value;
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
                matrix.Translate(this.CenterX, this.CenterY);
                matrix.Rotate(this.Angle);
                matrix.Translate(-this.CenterX, -this.CenterY);
                return matrix;
            }
        }

        public override string WriteToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "rotate({0}, {1}, {2})", this.Angle, this.CenterX, this.CenterY);
        }

        public SvgRotate(float angle)
        {
            this.Angle = angle;
        }

        public SvgRotate(float angle, float centerX, float centerY)
            : this(angle)
        {
            this.CenterX = centerX;
            this.CenterY = centerY;
        }


		public override object Clone()
		{
			return new SvgRotate(this.Angle, this.CenterX, this.CenterY);
		}
    }
}