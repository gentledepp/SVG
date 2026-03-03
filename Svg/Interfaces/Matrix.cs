
using System;
using System.Collections.Generic;
using System.Linq;
using Svg.Interfaces;
using Svg.Transforms;

namespace Svg
{
    public abstract class Matrix : IDisposable
    {
        public static Matrix Create()
        {
            return SvgEngine.Factory.CreateMatrix();
        }

        public static Matrix Create(float scaleX, float rotateX, float rotateY, float scaleY, float transX, float transY)
        {
            return SvgEngine.Factory.CreateMatrix(scaleX, rotateX, rotateY, scaleY, transX, transY);
        }

        public abstract void Scale(float width, float height);
        public abstract void Scale(float width, float height, MatrixOrder order);
        public abstract void Translate(float left, float top, MatrixOrder order);
        public abstract void TransformVectors(PointF[] points);
        public abstract void Translate(float left, float top);
        public abstract void Multiply(Matrix matrix);
        public abstract void Multiply(Matrix matrix, MatrixOrder order);
        public abstract void TransformPoints(PointF[] points);
        public abstract void RotateAt(float angleDegrees, PointF midPoint, MatrixOrder order);
        public abstract void Rotate(float angleDegrees, MatrixOrder order);
        public abstract Matrix Clone();
        public abstract float[] Elements { get; }
        public abstract float OffsetX { get;  }
        public abstract float OffsetY { get;  }
        public abstract float ScaleX { get; }
        public abstract float ScaleY { get; }
        public abstract float SkewX { get; }
        public abstract float SkewY { get; }
        public abstract bool IsIdentity { get; }

        public abstract void Rotate(float angleDegrees);
        public abstract void Shear(float sx, float sy);
        public virtual void Dispose()
        { }
        public override string ToString()
        {
            var e = Elements;
            return $"[{e[0]};{e[1]};{e[2]}],[{e[3]};{e[4]};{e[5]}],[{e[6]};{e[7]};{e[8]}]";
        }
        public override bool Equals(object obj)
        {
            var matrix = obj as Matrix;
            if (matrix == null)
                return false;

            return Elements.SequenceEqual(matrix.Elements);
        }

        public float RotationDegrees
        {
            get { return (float)RadianToDegree(Math.Atan(SkewY/ScaleY)); }
        }

        public float Rotation
        {
            get { return (float)(Math.Atan(SkewY / ScaleY)); }
        }

        /// <summary>
        /// Multiplies matriy a with b like "a*b"
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public float[] Multiply(float[] a, float[] b)
        {
            //var a = new float[9];
            //var b = new float[9];
            //_m.GetValues(a);
            //other._m.GetValues(b);

            var a1 = new float[3, 3];
            a1[0, 0] = a[0];
            a1[0, 1] = a[1];
            a1[0, 2] = a[2];
            a1[1, 0] = a[3];
            a1[1, 1] = a[4];
            a1[1, 2] = a[5];
            a1[2, 0] = a[6];
            a1[2, 1] = a[7];
            a1[2, 2] = a[8];

            var b1 = new float[3, 3];
            b1[0, 0] = b[0];
            b1[0, 1] = b[1];
            b1[0, 2] = b[2];
            b1[1, 0] = b[3];
            b1[1, 1] = b[4];
            b1[1, 2] = b[5];
            b1[2, 0] = b[6];
            b1[2, 1] = b[7];
            b1[2, 2] = b[8];


            var r = MultiplyMatrix(a1, b1);
            var result = new float[]
            {
                r[0, 0], r[0, 1], r[0, 2],
                r[1, 0], r[1, 1], r[1, 2],
                r[2, 0], r[2, 1], r[2, 2],
            };
            return result;
            //_m.SetValues();
        }

        /// <summary>
        /// Multiplies matriy a with b like "a*b"
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public float[,] MultiplyMatrix(float[,] a, float[,] b)
        {
            int rA = a.GetLength(0);
            int cA = a.GetLength(1);
            int rB = b.GetLength(0);
            int cB = b.GetLength(1);
            float temp = 0;
            float[,] kHasil = new float[rA, cB];
            if (cA != rB)
            {
                throw new InvalidOperationException("matrix cannot be multiplied");
            }
            else
            {
                for (int i = 0; i < rA; i++)
                {
                    for (int j = 0; j < cB; j++)
                    {
                        temp = 0;
                        for (int k = 0; k < cA; k++)
                        {
                            temp += a[i, k] * b[k, j];
                        }
                        kHasil[i, j] = temp;
                    }
                }
                return kHasil;
            }
        }

        /// <summary>
        /// Transforms a single point, avoiding array allocations.
        /// Override in platform implementations for inline math.
        /// </summary>
        public virtual PointF TransformPoint(float x, float y)
        {
            var pt = PointF.Create(x, y);
            TransformPoints(new[] { pt });
            return pt;
        }

        public abstract void Invert();

        public abstract RectangleF TransformRectangle(RectangleF bounds);

        public SvgMatrix ToSvgMatrix()
        {
            var points = new List<float>
            {
                ScaleX,
                SkewY, // x and y need to be swapped!
                SkewX, // x and y need to be swapped!
                ScaleY,
                OffsetX,
                OffsetY
            };
            return new SvgMatrix(points);
        }

        public static implicit operator SvgMatrix(Matrix other)
        {
            return other.ToSvgMatrix();
        }

        public static double DegreeToRadian(double angle)
        {
            return Math.PI * angle / 180.0;
        }

        public static double RadianToDegree(double angle)
        {
            return angle * (180.0 / Math.PI);
        }
    }
}