using System;
using System.IO;
using System.Linq;
using Svg.Editor.Tools;
using Svg.Interfaces;

namespace Svg.Editor.Extensions
{
    public static class SvgVisualElementExtensions
    {
        public static Matrix CreateOriginRotation(this SvgVisualElement e, float angleDegrees)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));

            var m = e.Transforms.GetMatrix();
            var inv = m.Clone();
            inv.Invert();
            var b = inv.TransformRectangle(e.GetBoundingBox());

            m.RotateAt(angleDegrees, PointF.Create(b.X + b.Width/2, b.Y + b.Height/2), MatrixOrder.Prepend);

            return m;
        }

        public static Matrix CreateTranslation(this SvgVisualElement e, float tx, float ty)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));

            var m = e.Transforms.GetMatrix();
            m.Translate(tx, ty, MatrixOrder.Append);
            return m;
        }

        public static void SetTransformationMatrix(this SvgVisualElement e, Matrix m)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));

            if (e.Transforms.Count != 1)
            {
                e.Transforms.Clear();
                if (m != null)
                    e.Transforms.Add(m);
            }
            else if (m != null)
            {
                e.Transforms[0] = m;
            }
            else
            {
                e.Transforms.Clear();
            }
        }

        public static bool HasConstraints(this SvgElement e, params string[] attributes)
        {
            if (!attributes.Any()) return true;

            string constraints;
            return e.CustomAttributes.TryGetValue(ToolBase.ConstraintsCustomAttributeKey, out constraints) &&
                   !string.IsNullOrEmpty(constraints) && constraints.Split(',').Any(attributes.Contains);
        }

        public static void AddConstraints(this SvgElement e, params string[] attributes)
        {
            if (!attributes.Any()) return;

            string constraints;
            if (e.CustomAttributes.TryGetValue(ToolBase.ConstraintsCustomAttributeKey, out constraints) &&
                !string.IsNullOrEmpty(constraints))
            {
                var joined = string.Join(",",
                    constraints.Split(',')
                        .Where(x => !attributes.Contains(x)));
                if (!string.IsNullOrEmpty(joined))
                    constraints = $"{constraints},{joined}";
            }
            else
            {
                constraints = $"{string.Join(",", attributes)}";
            }
            e.CustomAttributes[ToolBase.ConstraintsCustomAttributeKey] = constraints;
        }
    }
}
