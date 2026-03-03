using SkiaSharp;
using Svg.Interfaces;

namespace Svg.Platform
{
    // maybe copy from http://stackoverflow.com/questions/15817888/fast-rotation-transformation-matrix-multiplications ?
    // see also exmplanations at https://www.willamette.edu/~gorr/classes/GeneralGraphics/Transforms/transforms2d.htm
    public class SkiaMatrix : Matrix
    {
        private SKMatrix _m;

        /// <summary>
        /// Creates a new instance of <see cref="SkiaMatrix"/>, the initial value is an identity matrix.
        /// </summary>
        public SkiaMatrix()
        {
            _m = SKMatrix.CreateIdentity();
        }

        public SkiaMatrix(SKMatrix src)
        {
            _m = new SKMatrix();
            _m.Persp0 = src.Persp0;
            _m.Persp1 = src.Persp1;
            _m.Persp2 = src.Persp2;
            _m.ScaleX = src.ScaleX;
            _m.ScaleY = src.ScaleY;
            _m.SkewX = src.SkewX;
            _m.SkewY = src.SkewY;
            _m.TransX = src.TransX;
            _m.TransY = src.TransY;
        }

        public SkiaMatrix(SKMatrix src, bool copy)
        {
            _m = src;
        }

        public SkiaMatrix(float[] e)
        {
            _m = new SKMatrix()
            {
                ScaleX = e[0],
                SkewX = e[1],
                TransX = e[2],
                SkewY = e[3],
                ScaleY = e[4],
                TransY = e[5],
                Persp0 = e[6],
                Persp1 = e[7],
                Persp2 = e[8]
            };
        }

        //
        // Summary:
        //     Initializes a new instance of the System.Drawing.Drawing2D.Matrix class with
        //     the specified elements.
        //
        // see: https://msdn.microsoft.com/en-us/library/system.drawing.drawing2d.matrix(v=vs.110).aspx
        public SkiaMatrix(float scaleX, float rotateX, float rotateY, float scaleY, float transX, float transY)
        {
            _m = new SKMatrix();

            /*
             * In android, rotateX and rotateY are switched for whatever reason!!
             */
            _m.ScaleX = scaleX;
            _m.SkewX = rotateY;
            _m.TransX = transX;
            _m.SkewY = rotateX;
            _m.ScaleY = scaleY;
            _m.TransY = transY;
            _m.Persp0 = 0;
            _m.Persp1 = 0;
            _m.Persp2 = 1;

            /*      see:https://github.com/google/skia/blob/master/src/core/SkMatrix.cpp
             *      [scale-x    skew-x      trans-x]   [X]   [X']
             *      [skew-y     scale-y     trans-y] * [Y] = [Y']
             *      [persp-0    persp-1     persp-2]   [1]   [1 ]
            */

        }

        public SKMatrix Matrix => _m;

        public override bool IsIdentity
        {
            get
            {
                return _m.ScaleX == 1f && _m.SkewX == 0f && _m.TransX == 0f &&
                       _m.SkewY == 0f && _m.ScaleY == 1f && _m.TransY == 0f &&
                       _m.Persp0 == 0f && _m.Persp1 == 0f && _m.Persp2 == 1f;
            }
        }

        public override void Invert()
        {
            //// copied from SkMatrix::invertNonIdentity
            //// see: https://github.com/google/skia/blob/master/src/core/SkMatrix.cpp
            //if (IsIdentity)
            //    return;

            //var m = _m;

            //bool isScaleMatrix = m.ScaleX != 1f || m.ScaleY != 1f;
            //bool isTranslateMatrix = m.TransX != 0f || m.TransY != 0f;
            //bool isRotateMatrix = m.SkewX != 0f || m.SkewX != 0f;

            //if (!isRotateMatrix && isScaleMatrix && isTranslateMatrix)
            //{
            //    var invX = m.ScaleX;
            //    var invY = m.ScaleY;
            //    if (invX == 0 || invY == 0)
            //    {
            //        // not invertible
            //        return;
            //    }
            //    invX = 1/invX;
            //    invY = 1/invY;
            //    m.SkewX = 0;
            //    m.SkewY = 0;
            //    m.Persp0 = 0;
            //    m.Persp1 = 0;

            //    m.ScaleX = invX;
            //    m.ScaleY = invY;
            //    m.Persp2 = 1;
            //    m.TransX = -m.TransX*invX;
            //    m.TransY = -m.TransY*invY;

            //    _m = m;
            //    return;
            //}
            //else if (!isRotateMatrix && isTranslateMatrix)
            //{
            //    m.TransX = -m.TransX;
            //    m.TransY = -m.TransY;
            //    _m = m;
            //    return;
            //}

            //var m = _m;
            //float det = m.ScaleX * (m.ScaleY * m.Persp2 - m.Persp1 * m.TransY) -
            // m.SkewX * (m.SkewY * m.Persp2 - m.TransY * m.Persp0) +
            // m.TransX * (m.SkewY * m.Persp1 - m.ScaleY * m.Persp0);

            //float invdet = 1 / det;

            //var m1 = new SKMatrix();
            //m1.ScaleX = (m.ScaleY * m.Persp2 - m.Persp1 * m.TransY) * invdet;
            //m1.SkewX = (m.TransX * m.Persp1 - m.SkewX * m.Persp2) * invdet;
            //m1.TransX = (m.SkewX * m.TransY - m.TransX * m.ScaleY) * invdet;
            //m1.SkewY = (m.TransY * m.Persp0 - m.SkewY * m.Persp2) * invdet;
            //m1.ScaleY = (m.ScaleX * m.Persp2 - m.TransX * m.Persp0) * invdet;
            //m1.TransY = (m.SkewY * m.TransX - m.ScaleX * m.TransY) * invdet;
            //m1.Persp0 = (m.SkewY * m.Persp1 - m.Persp0 * m.ScaleY) * invdet;
            //m1.Persp1 = (m.Persp0 * m.SkewX - m.ScaleX * m.Persp1) * invdet;
            //m1.Persp2 = (m.ScaleX * m.ScaleY - m.SkewY * m.SkewX) * invdet;
            //_m = m1;

            SKMatrix m;
            if (_m.TryInvert(out m))
            {
                _m = m;
            }
        }

        public override void Scale(float width, float height)
        {
            Scale(width, height, MatrixOrder.Prepend);
        }

        public override void Scale(float width, float height, MatrixOrder order)
        {
            var m = SKMatrix.CreateScale(width, height);

            if (order == MatrixOrder.Append) 
                _m = _m.PostConcat(m);
            else
                _m = _m.PreConcat(m);
        }

        public override void Translate(float left, float top)
        {
            Translate(left, top, MatrixOrder.Prepend);
        }

        public override void Translate(float left, float top, MatrixOrder order)
        {
            var m = SKMatrix.CreateTranslation(left, top);

            if (order == MatrixOrder.Append)
                _m = _m.PostConcat(m);
            else 
                _m = _m.PreConcat(m);
        }
        
        /// <summary>
        /// Does a pre-pend multiplication
        /// </summary>
        /// <param name="matrix"></param>
        public override void Multiply(Matrix matrix)
        {
            Multiply(matrix, MatrixOrder.Prepend);
        }

        public override void Multiply(Matrix matrix, MatrixOrder order)
        {
            var m = ((SkiaMatrix)matrix).Matrix;

            if (order == MatrixOrder.Append)
                _m = _m.PostConcat(m);
            else
                _m = _m.PreConcat(m);
        }

        public override void Rotate(float angleDegrees, MatrixOrder order)
        {
            var m = SKMatrix.CreateRotationDegrees(angleDegrees);

            if (order == MatrixOrder.Append)
                _m = _m.PostConcat(m);
            else
                _m = _m.PreConcat(m);
        }

        public override void RotateAt(float angleDegrees, PointF midPoint, MatrixOrder order)
        {
            var m = SKMatrix.CreateRotationDegrees(angleDegrees, midPoint.X, midPoint.Y);

            if (order == MatrixOrder.Append)
                _m = _m.PostConcat(m);
            else
                _m = _m.PreConcat(m);
        }

        public override void Rotate(float angleDegrees)
        {
            Rotate(angleDegrees, MatrixOrder.Prepend);
        }

        public override void Shear(float sx, float sy)
        {
            var m = SKMatrix.CreateSkew(sx, sy);

            _m = _m.PreConcat(m);
        }

        public override RectangleF TransformRectangle(RectangleF b)
        {
            var p1 = PointF.Create(b.Left, b.Top);
            var p2 = PointF.Create(b.Right, b.Top);
            var p3 = PointF.Create(b.Right, b.Bottom);
            var p4 = PointF.Create(b.Left, b.Bottom);
            var pts = new[] {p1, p2, p3, p4};

            TransformPoints(pts);

            return RectangleF.FromPoints(pts);
        }

        public override void TransformVectors(PointF[] points)
        {
            var m = _m;
            for (int i = 0; i < points.Length; i++)
            {
                var p = points[i];
                var px = p.X;
                var py = p.Y;
                p.X = m.ScaleX * px + m.SkewX * py;
                p.Y = m.SkewY * px + m.ScaleY * py;
            }
        }

        public override void TransformPoints(PointF[] points)
        {
            var m = _m;
            for (int i = 0; i < points.Length; i++)
            {
                var p = points[i];
                var px = p.X;
                var py = p.Y;
                var w = m.Persp0 * px + m.Persp1 * py + m.Persp2;
                p.X = m.ScaleX * px + m.SkewX * py + m.TransX;
                p.Y = m.SkewY * px + m.ScaleY * py + m.TransY;
                if (w != 1f) { p.X /= w; p.Y /= w; }
            }
        }

        public override PointF TransformPoint(float x, float y)
        {
            var m = _m;
            var rx = m.ScaleX * x + m.SkewX * y + m.TransX;
            var ry = m.SkewY * x + m.ScaleY * y + m.TransY;
            var w = m.Persp0 * x + m.Persp1 * y + m.Persp2;
            if (w != 1f) { rx /= w; ry /= w; }
            return PointF.Create(rx, ry);
        }
        
        public override float[] Elements
        {
            get
            {
                var res = new float[9]
                {
                    _m.ScaleX,
                    _m.SkewX,
                    _m.TransX,
                    _m.SkewY,
                    _m.ScaleY,
                    _m.TransY,
                    _m.Persp0,
                    _m.Persp1,
                    _m.Persp2,
                };
                return res;
            }
        }

        public override float OffsetX
        {
            get { return _m.TransX; }
        }

        public override float OffsetY
        {
            get { return _m.TransY; }
        }

        public override float ScaleX
        {
            get { return _m.ScaleX; }
        }

        public override float ScaleY
        {
            get { return _m.ScaleY; }
        }

        public override float SkewX
        {
            get { return _m.SkewX; }
        }

        public override float SkewY
        {
            get { return _m.SkewY; }
        }

        private static float[] GetElements(SKMatrix m)
        {
            return new float[9]
                {
                    m.ScaleX,
                    m.SkewX,
                    m.TransX,
                    m.SkewY,
                    m.ScaleY,
                    m.TransY,
                    m.Persp0,
                    m.Persp1,
                    m.Persp2,
                };
        }
        
        public static implicit operator SkiaMatrix(SKMatrix other)
        {
            return new SkiaMatrix(other, true);
        }

        public static implicit operator SKMatrix(SkiaMatrix other)
        {
            return other.Matrix;
        }

        public override Matrix Clone()
        {
            return new SkiaMatrix(_m);
        }

        public override void Dispose()
        {
        }
    }
}