using System;
using NUnit.Framework;
using Svg.Editor.Extensions;
using Svg.Interfaces;
using Svg.Transforms;

namespace Svg.Editor.Core.Test
{
    [TestFixture]
    public class TransformationTests : SvgDrawingCanvasTestBase
    {
        [Test]
        public void CanCombine_ScaleTranslateRotate_ToMatrix()
        {
            // Arrange
            var sx = 1;
            var sy = 1;
            var tx = 100;
            var ty = 50;
            var rx = -135f;
            var width = 200;
            var height = 120;
            var rect = new SvgRectangle()
            {
                Width = new SvgUnit(SvgUnitType.Pixel, width),
                Height = new SvgUnit(SvgUnitType.Pixel, height),
            };
            var scale = new SvgScale(sx, sy);
            var trans = new SvgTranslate(tx, ty);
            var rot = new SvgRotate(rx, width/2, height/2);
            
            // Act
            rect.Transforms.Add(scale);
            rect.Transforms.Add(trans);
            rect.Transforms.Add(rot);
            var box1 = rect.GetBoundingBox();

            var m = Matrix.Create();
            m.Scale(sx, sy);
            m.Translate(tx, ty);
            m.RotateAt(rx, PointF.Create(width/2, height/2), MatrixOrder.Prepend);
            rect.Transforms.Clear();
            rect.Transforms.Add(m);
            var box2 = rect.GetBoundingBox();

            // Assert
            Assert.AreEqual(box1, box2);
        }
        
        [Test]
        public void CanAddRotation_ToExistingTransforMatrix()
        {
            // Arrange
            var width = 200;
            var height = 120;
            var rect = new SvgRectangle()
            {
                Width = new SvgUnit(SvgUnitType.Pixel, width),
                Height = new SvgUnit(SvgUnitType.Pixel, height),
            };

            rect.Transforms.Add(new SvgTranslate(100, 150));
            rect.Transforms.Add(new SvgRotate(135, width/2, height/2));
            var box1 = rect.GetBoundingBox();

            // Act
            // version 1: append translate
            var m2 = Matrix.Create();
            m2.Translate(200, 300);
            m2.RotateAt(135, PointF.Create(width/2, height/2), MatrixOrder.Prepend);
            m2.Translate(-100, -150, MatrixOrder.Append);
            rect.Transforms.Clear();
            rect.Transforms.Add(m2);
            var box2 = rect.GetBoundingBox();

            // version 2: append translate manually
            var m3 = Matrix.Create();
            m3.Translate(200, 300);
            m3.RotateAt(135, PointF.Create(width/2, height/2), MatrixOrder.Prepend);
            var m3b = Matrix.Create();
            m3b.Translate(-100, -150, MatrixOrder.Prepend);
            m3b.Multiply(m3);
            rect.Transforms.Clear();
            rect.Transforms.Add(m3b);
            var box3 = rect.GetBoundingBox();

            // version 3: add rotation
            var m4 = Matrix.Create();
            m4.Translate(200, 300, MatrixOrder.Append);
            m4.RotateAt(45, PointF.Create(width / 2, height / 2), MatrixOrder.Prepend);
            m4.Translate(-100, -150, MatrixOrder.Append);
            m4.RotateAt(45, PointF.Create(width / 2, height / 2), MatrixOrder.Prepend);
            m4.RotateAt(45, PointF.Create(width / 2, height / 2), MatrixOrder.Prepend);
            rect.Transforms.Clear();
            rect.Transforms.Add(m4);
            var box4 = rect.GetBoundingBox();

            // version 4: rotate around boundingbox center
            var m5 = Matrix.Create();
            m5.Translate(200, 300, MatrixOrder.Append);
            rect.Transforms.Clear();
            rect.Transforms.Add(m5);
            var box5b = rect.GetBoundingBox();
            m5.RotateAt(90, PointF.Create(box5b.Width / 2, box5b.Height / 2), MatrixOrder.Prepend);
            m5.Translate(-100, -150, MatrixOrder.Append);
            rect.Transforms.Clear();
            rect.Transforms.Add(m5);
            box5b = rect.GetBoundingBox();
            var m5b = m5.Clone();
            m5b.Invert();
            box5b = m5b.TransformRectangle(box5b); // when rotating, boundingbox must be reverted to original stance
            m5.RotateAt(45, PointF.Create(box5b.Width / 2, box5b.Height / 2), MatrixOrder.Prepend);
            rect.Transforms.Clear();
            rect.Transforms.Add(m5);
            var box5 = rect.GetBoundingBox();

            // Assert
            EnsureRectanglesEqual(box1, box2);
            EnsureRectanglesEqual(box1, box3);
            EnsureRectanglesEqual(box1, box4);
            EnsureRectanglesEqual(box1, box5);
        }

        [Test]
        public void CanAddRotationAndTranslation_ToExistingTransforMatrix_UsingHelpers()
        {
            // Arrange
            var width = 200;
            var height = 120;
            var rect = new SvgRectangle()
            {
                Width = new SvgUnit(SvgUnitType.Pixel, width),
                Height = new SvgUnit(SvgUnitType.Pixel, height),
            };

            rect.Transforms.Add(new SvgTranslate(100, 150));
            rect.Transforms.Add(new SvgRotate(135, width / 2, height / 2));
            var box1 = rect.GetBoundingBox();

            // Act
            rect.Transforms.Clear();
            rect.SetTransformationMatrix(rect.CreateTranslation(100, 150));
            rect.SetTransformationMatrix(rect.CreateOriginRotation(135f));
            var box2 = rect.GetBoundingBox();

            rect.Transforms.Clear();
            rect.SetTransformationMatrix(rect.CreateTranslation(200, -150));
            rect.SetTransformationMatrix(rect.CreateOriginRotation(90f));
            rect.SetTransformationMatrix(rect.CreateTranslation(-100, 300));
            rect.SetTransformationMatrix(rect.CreateOriginRotation(45f));
            var box3 = rect.GetBoundingBox();

            // Assert
            EnsureRectanglesEqual(box1, box2);
            EnsureRectanglesEqual(box1, box3);
        }
        
        private void EnsureRectanglesEqual(RectangleF expected, RectangleF actual)
        {
            Assert.AreEqual(Math.Round(expected.X, 2), Math.Round(actual.X, 2), $"{expected} \nvs {actual}");
            Assert.AreEqual(Math.Round(expected.Y, 2), Math.Round(actual.Y, 2), $"{expected} \nvs {actual}");
            Assert.AreEqual(Math.Round(expected.Width, 2), Math.Round(actual.Width, 2), $"{expected} \nvs {actual}");
            Assert.AreEqual(Math.Round(expected.Height, 2), Math.Round(actual.Height, 2), $"{expected} \nvs {actual}");
        }
    }
}
