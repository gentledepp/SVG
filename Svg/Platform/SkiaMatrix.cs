using System.Linq;
using SkiaSharp;
using Svg.Interfaces;

namespace Svg.Platform;

/// <summary>
/// A matrix implementation using SkiaSharp for 2D transformations in SVG rendering.
/// Provides functionality for scaling, translation, rotation, and other geometric transformations.
/// </summary>
// maybe copy from http://stackoverflow.com/questions/15817888/fast-rotation-transformation-matrix-multiplications ?
// see also explanations at https://www.willamette.edu/~gorr/classes/GeneralGraphics/Transforms/transforms2d.htm
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

    /// <summary>
    /// Creates a new instance of <see cref="SkiaMatrix"/> by copying values from the source matrix.
    /// </summary>
    /// <param name="src">The source SKMatrix to copy from.</param>
    public SkiaMatrix(SKMatrix src)
    {
        _m = new SKMatrix
        {
            Persp0 = src.Persp0,
            Persp1 = src.Persp1,
            Persp2 = src.Persp2,
            ScaleX = src.ScaleX,
            ScaleY = src.ScaleY,
            SkewX = src.SkewX,
            SkewY = src.SkewY,
            TransX = src.TransX,
            TransY = src.TransY
        };
    }

    /// <summary>
    /// Creates a new instance of <see cref="SkiaMatrix"/> with optional copying behavior.
    /// </summary>
    /// <param name="src">The source SKMatrix.</param>
    /// <param name="copy">If true, creates a copy; otherwise, references the original (not recommended).</param>
    public SkiaMatrix(SKMatrix src, bool copy)
    {
        _m = copy ? new SKMatrix
        {
            Persp0 = src.Persp0,
            Persp1 = src.Persp1,
            Persp2 = src.Persp2,
            ScaleX = src.ScaleX,
            ScaleY = src.ScaleY,
            SkewX = src.SkewX,
            SkewY = src.SkewY,
            TransX = src.TransX,
            TransY = src.TransY
        } : src;
    }

    /// <summary>
    /// Creates a new instance of <see cref="SkiaMatrix"/> from a 9-element array.
    /// </summary>
    /// <param name="e">Array of 9 matrix elements in row-major order.</param>
    public SkiaMatrix(float[] e)
    {
        if (e.Length != 9)
            throw new System.ArgumentException("Array must contain exactly 9 elements", nameof(e));

        _m = new SKMatrix
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

    /// <summary>
    /// Initializes a new instance of the SkiaMatrix class with the specified elements.
    /// Based on System.Drawing.Drawing2D.Matrix constructor pattern.
    /// </summary>
    /// <param name="scaleX">The scale factor in the X direction.</param>
    /// <param name="rotateX">The rotation/skew factor for X (actually maps to SkewY in SkiaSharp).</param>
    /// <param name="rotateY">The rotation/skew factor for Y (actually maps to SkewX in SkiaSharp).</param>
    /// <param name="scaleY">The scale factor in the Y direction.</param>
    /// <param name="transX">The translation in the X direction.</param>
    /// <param name="transY">The translation in the Y direction.</param>
    // see: https://msdn.microsoft.com/en-us/library/system.drawing.drawing2d.matrix(v=vs.110).aspx
    public SkiaMatrix(float scaleX, float rotateX, float rotateY, float scaleY, float transX, float transY)
    {
        /*
         * In android, rotateX and rotateY are switched for whatever reason!!
         */
        _m = new SKMatrix
        {
            ScaleX = scaleX,
            SkewX = rotateY,
            TransX = transX,
            SkewY = rotateX,
            ScaleY = scaleY,
            TransY = transY,
            Persp0 = 0,
            Persp1 = 0,
            Persp2 = 1
        };

        /*      see:https://github.com/google/skia/blob/master/src/core/SkMatrix.cpp
         *      [scale-x    skew-x      trans-x]   [X]   [X']
         *      [skew-y     scale-y     trans-y] * [Y] = [Y']
         *      [persp-0    persp-1     persp-2]   [1]   [1 ]
        */
    }

    /// <summary>
    /// Gets the underlying SKMatrix instance.
    /// </summary>
    public SKMatrix Matrix => _m;

    /// <summary>
    /// Gets a value indicating whether this matrix is an identity matrix.
    /// </summary>
    public override bool IsIdentity
    {
        get
        {
            return _m.ScaleX == 1f && _m.SkewX == 0f && _m.TransX == 0f &&
                   _m.SkewY == 0f && _m.ScaleY == 1f && _m.TransY == 0f &&
                   _m.Persp0 == 0f && _m.Persp1 == 0f && _m.Persp2 == 1f;
        }
    }

    /// <summary>
    /// Inverts this matrix if it is invertible.
    /// </summary>
    public override void Invert()
    {
        if (_m.TryInvert(out SKMatrix inverted))
        {
            _m = inverted;
        }
        // If inversion fails, the matrix remains unchanged
    }

    /// <summary>
    /// Applies scaling transformation to the matrix with prepend order.
    /// </summary>
    /// <param name="width">The scale factor in the X direction.</param>
    /// <param name="height">The scale factor in the Y direction.</param>
    public override void Scale(float width, float height)
    {
        Scale(width, height, MatrixOrder.Prepend);
    }

    /// <summary>
    /// Applies scaling transformation to the matrix with the specified order.
    /// </summary>
    /// <param name="width">The scale factor in the X direction.</param>
    /// <param name="height">The scale factor in the Y direction.</param>
    /// <param name="order">The order of matrix multiplication.</param>
    public override void Scale(float width, float height, MatrixOrder order)
    {
        var scaleMatrix = SKMatrix.CreateScale(width, height);

        if (order == MatrixOrder.Append)
            _m = _m.PostConcat(scaleMatrix);
        else
            _m = _m.PreConcat(scaleMatrix);
    }

    /// <summary>
    /// Applies translation transformation to the matrix with prepend order.
    /// </summary>
    /// <param name="left">The translation distance in the X direction.</param>
    /// <param name="top">The translation distance in the Y direction.</param>
    public override void Translate(float left, float top)
    {
        Translate(left, top, MatrixOrder.Prepend);
    }

    /// <summary>
    /// Applies translation transformation to the matrix with the specified order.
    /// </summary>
    /// <param name="left">The translation distance in the X direction.</param>
    /// <param name="top">The translation distance in the Y direction.</param>
    /// <param name="order">The order of matrix multiplication.</param>
    public override void Translate(float left, float top, MatrixOrder order)
    {
        var translateMatrix = SKMatrix.CreateTranslation(left, top);

        if (order == MatrixOrder.Append)
            _m = _m.PostConcat(translateMatrix);
        else
            _m = _m.PreConcat(translateMatrix);
    }

    /// <summary>
    /// Multiplies this matrix by the specified matrix using prepend order.
    /// </summary>
    /// <param name="matrix">The matrix to multiply with.</param>
    public override void Multiply(Matrix matrix)
    {
        Multiply(matrix, MatrixOrder.Prepend);
    }

    /// <summary>
    /// Multiplies this matrix by the specified matrix using the specified order.
    /// </summary>
    /// <param name="matrix">The matrix to multiply with.</param>
    /// <param name="order">The order of matrix multiplication.</param>
    public override void Multiply(Matrix matrix, MatrixOrder order)
    {
        var otherMatrix = ((SkiaMatrix)matrix).Matrix;

        if (order == MatrixOrder.Append)
            _m = _m.PostConcat(otherMatrix);
        else
            _m = _m.PreConcat(otherMatrix);
    }

    /// <summary>
    /// Applies rotation transformation to the matrix with the specified order.
    /// </summary>
    /// <param name="angleDegrees">The rotation angle in degrees.</param>
    /// <param name="order">The order of matrix multiplication.</param>
    public override void Rotate(float angleDegrees, MatrixOrder order)
    {
        var rotationMatrix = SKMatrix.CreateRotationDegrees(angleDegrees);

        if (order == MatrixOrder.Append)
            _m = _m.PostConcat(rotationMatrix);
        else
            _m = _m.PreConcat(rotationMatrix);
    }

    /// <summary>
    /// Applies rotation transformation around a specified point with the specified order.
    /// </summary>
    /// <param name="angleDegrees">The rotation angle in degrees.</param>
    /// <param name="midPoint">The center point of rotation.</param>
    /// <param name="order">The order of matrix multiplication.</param>
    public override void RotateAt(float angleDegrees, PointF midPoint, MatrixOrder order)
    {
        var rotationMatrix = SKMatrix.CreateRotationDegrees(angleDegrees, midPoint.X, midPoint.Y);

        if (order == MatrixOrder.Append)
            _m = _m.PostConcat(rotationMatrix);
        else
            _m = _m.PreConcat(rotationMatrix);
    }

    /// <summary>
    /// Applies rotation transformation to the matrix with prepend order.
    /// </summary>
    /// <param name="angleDegrees">The rotation angle in degrees.</param>
    public override void Rotate(float angleDegrees)
    {
        Rotate(angleDegrees, MatrixOrder.Prepend);
    }

    /// <summary>
    /// Applies shear transformation to the matrix.
    /// </summary>
    /// <param name="sx">The shear factor in the X direction.</param>
    /// <param name="sy">The shear factor in the Y direction.</param>
    public override void Shear(float sx, float sy)
    {
        var shearMatrix = SKMatrix.CreateSkew(sx, sy);
        _m = _m.PreConcat(shearMatrix);
    }

    /// <summary>
    /// Transforms a rectangle by applying this matrix transformation.
    /// </summary>
    /// <param name="b">The rectangle to transform.</param>
    /// <returns>The transformed rectangle.</returns>
    public override RectangleF TransformRectangle(RectangleF b)
    {
        var p1 = PointF.Create(b.Left, b.Top);
        var p2 = PointF.Create(b.Right, b.Top);
        var p3 = PointF.Create(b.Right, b.Bottom);
        var p4 = PointF.Create(b.Left, b.Bottom);
        var pts = new[] { p1, p2, p3, p4 };

        TransformPoints(pts);

        return RectangleF.FromPoints(pts);
    }

    /// <summary>
    /// Transforms an array of vectors by applying this matrix transformation.
    /// Vectors are transformed without translation (only rotation, scaling, and skewing).
    /// </summary>
    /// <param name="points">The array of vectors to transform.</param>
    public override void TransformVectors(PointF[] points)
    {
        var pts = points.Select(p => new SKPoint(p.X, p.Y)).ToArray();

        var mappedPoints = _m.MapVectors(pts);
        for (int i = 0; i < mappedPoints.Length; i++)
        {
            points[i].X = mappedPoints[i].X;
            points[i].Y = mappedPoints[i].Y;
        }
    }

    /// <summary>
    /// Transforms an array of points by applying this matrix transformation.
    /// Points are transformed with full transformation including translation.
    /// </summary>
    /// <param name="points">The array of points to transform.</param>
    public override void TransformPoints(PointF[] points)
    {
        var pts = points.Select(p => new SKPoint(p.X, p.Y)).ToArray();

        var mappedPoints = _m.MapPoints(pts);
        for (int i = 0; i < mappedPoints.Length; i++)
        {
            points[i].X = mappedPoints[i].X;
            points[i].Y = mappedPoints[i].Y;
        }
    }

    /// <summary>
    /// Gets the elements of this matrix as a 9-element array in row-major order.
    /// </summary>
    public override float[] Elements
    {
        get
        {
            return new float[9]
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
        }
    }

    /// <summary>
    /// Gets the X translation component of the matrix.
    /// </summary>
    public override float OffsetX => _m.TransX;

    /// <summary>
    /// Gets the Y translation component of the matrix.
    /// </summary>
    public override float OffsetY => _m.TransY;

    /// <summary>
    /// Gets the X scale component of the matrix.
    /// </summary>
    public override float ScaleX => _m.ScaleX;

    /// <summary>
    /// Gets the Y scale component of the matrix.
    /// </summary>
    public override float ScaleY => _m.ScaleY;

    /// <summary>
    /// Gets the X skew component of the matrix.
    /// </summary>
    public override float SkewX => _m.SkewX;

    /// <summary>
    /// Gets the Y skew component of the matrix.
    /// </summary>
    public override float SkewY => _m.SkewY;

    /// <summary>
    /// Gets the elements of the specified SKMatrix as a 9-element array.
    /// </summary>
    /// <param name="m">The SKMatrix to extract elements from.</param>
    /// <returns>A 9-element array containing the matrix elements.</returns>
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

    /// <summary>
    /// Implicitly converts an SKMatrix to a SkiaMatrix.
    /// </summary>
    /// <param name="other">The SKMatrix to convert.</param>
    /// <returns>A new SkiaMatrix instance.</returns>
    public static implicit operator SkiaMatrix(SKMatrix other)
    {
        return new SkiaMatrix(other, true);
    }

    /// <summary>
    /// Implicitly converts a SkiaMatrix to an SKMatrix.
    /// </summary>
    /// <param name="other">The SkiaMatrix to convert.</param>
    /// <returns>The underlying SKMatrix instance.</returns>
    public static implicit operator SKMatrix(SkiaMatrix other)
    {
        return other.Matrix;
    }

    /// <summary>
    /// Creates a copy of this matrix.
    /// </summary>
    /// <returns>A new SkiaMatrix that is a copy of this instance.</returns>
    public override Matrix Clone()
    {
        return new SkiaMatrix(_m);
    }

    /// <summary>
    /// Releases all resources used by the SkiaMatrix.
    /// No resources need to be disposed for SKMatrix in SkiaSharp 3.x.
    /// </summary>
    public override void Dispose()
    {
        // SKMatrix is a struct in SkiaSharp 3.x and doesn't require disposal
    }
}