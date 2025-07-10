﻿using System;
using System.Globalization;

namespace Svg.Transforms
{
    /// <summary>
    /// The class which applies the specified skew vector to this Matrix.
    /// </summary>
    public sealed class SvgSkew : SvgTransform
    {
        private Matrix matrix;
        private float _angleX;
        private float _angleY;

        public float AngleX
        {
            get { return _angleX; }
            set { _angleX = value;
                matrix = null;
            }
        }

        public float AngleY
        {
            get { return _angleY; }
            set { _angleY = value;
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
                matrix.Shear(
                    (float)Math.Tan(AngleX/180*Math.PI),
                    (float)Math.Tan(AngleY/180*Math.PI));
                return matrix;
            }
        }

        public override string WriteToString()
        {
            if (this.AngleY == 0)
            {
                return string.Format(CultureInfo.InvariantCulture, "skewX({0})", this.AngleX);
            }
            else
            {
                return string.Format(CultureInfo.InvariantCulture, "skewY({0})", this.AngleY);
            }
        }

        public SvgSkew(float x, float y)
        {
            AngleX = x;
            AngleY = y;
        }


		public override object Clone()
		{
			return new SvgSkew(this.AngleX, this.AngleY);
		}
    }
}