using System;
using System.Collections.Generic;
using System.Linq;
using Svg.Interfaces;
using Svg.Transforms;

namespace Svg
{
    public enum SelectionType
    {
        Intersect,
        IntersectBoundingBoxes,
        Contain
    }

    public enum HitTestResultMode
    {
        /// <summary>
        /// If the hit test finds a child/descendant element being hit at the coordinates, this element is returned
        /// </summary>
        ReturnAllMatchingDescendants = 0,
        /// <summary>
        /// If the hit test finds a child/descendant element being hit at the coordinates, returns the root element (this element)
        /// </summary>
        ReturnRootElementOnly = 1,
    }

    /// <summary>
    /// Hit test book explaining the math behind all this:
    /// https://www.jeffreythompson.org/collision-detection/
    /// </summary>
    public static class SvgHitTestingExtensions
    {
        public static IEnumerable<TElement> HitTest<TElement>(this SvgDocument doc,
            RectangleF rectangle,
            SelectionType selectionType = SelectionType.Intersect,
            HitTestResultMode hitTestResultMode = HitTestResultMode.ReturnAllMatchingDescendants,
            Matrix matrix = null,
            int maxRecursion = int.MaxValue) where TElement : SvgVisualElement
        {
            matrix ??= Matrix.Create();
            foreach (var transform in doc.Transforms)
                transform.ApplyTo(matrix);

            return doc.Children.OfType<SvgVisualElement>()
                // reverse for correct z-order!
                .Reverse()
                .SelectMany(e =>
                    e.HitTest<TElement>(rectangle, selectionType, hitTestResultMode, matrix, maxRecursion));
        }

        public static IEnumerable<TElement> HitTest<TElement>(this SvgVisualElement e, 
            RectangleF rectangle,
            SelectionType selectionType = SelectionType.Intersect,
            HitTestResultMode hitTestResultMode = HitTestResultMode.ReturnAllMatchingDescendants,
            Matrix transform = null,
            int maxRecursion = int.MaxValue) where TElement : SvgVisualElement
        {
            return e.HitTestInternal<TElement>(rectangle, selectionType, hitTestResultMode, transform ?? Matrix.Create(), maxRecursion);
        }

        public static IEnumerable<TElement> HitTestInternal<TElement>(this SvgVisualElement e, RectangleF rectangle,
            SelectionType selectionType,
            HitTestResultMode hitTestResultMode,
            Matrix transform,
            int maxRecursion) where TElement : SvgVisualElement
        {
            if (transform == null)
                transform = Matrix.Create();
            else
                transform = transform.Clone();

            bool IsIntersect(SelectionType st) => st == SelectionType.Intersect || st == SelectionType.IntersectBoundingBoxes;
            bool WithBoundingBoxOnly(SelectionType st) => st == SelectionType.IntersectBoundingBoxes;

            if (e.Renderable)
            {
                var pts = e.GetTransformedElementPoints(transform);
                var box = RectangleF.FromPoints(pts);

                // in certain edge cases, the bounding box can be so 
                if (IsIntersect(selectionType))
                    box = box.InflateAndCopy(rectangle.Width, rectangle.Height);

                // if this element fits the type filter, check if it fits the hittest rectangle
                if (e is TElement te)
                {
                    if (IsIntersect(selectionType) && rectangle.IntersectsWith(box))
                    {
                        if (WithBoundingBoxOnly(selectionType) || e.IntersectsWith(rectangle, transform, maxRecursion))
                            yield return te;
                    }
                    else if ((selectionType == SelectionType.Contain) && rectangle.Contains(box))
                        yield return te;
                }
            }
            else
            {
                // recurse the hittest to the inner levels
                var recurs = maxRecursion - 1;
                if (recurs > 0)
                {
                    var t2 = transform.Clone();
                    foreach (SvgTransform transformation in e.Transforms)
                    {
                        transformation.ApplyTo(t2);
                    }

                    if (hitTestResultMode == HitTestResultMode.ReturnAllMatchingDescendants)
                    {
                        // reverse children because of z-index
                        foreach (var hit in e.Children.Reverse().OfType<SvgVisualElement>()
                                     .SelectMany(child =>
                                         child.HitTestInternal<TElement>(rectangle, selectionType, hitTestResultMode, t2,
                                             recurs)))
                        {
                            yield return hit;
                        }
                    }
                    // if root element is selected, and any child is hit
                    else if (hitTestResultMode == HitTestResultMode.ReturnRootElementOnly &&
                             e is TElement te &&
                             e.Children.Reverse().OfType<SvgVisualElement>()
                                 .SelectMany(child =>
                                     child.HitTestInternal<TElement>(rectangle, selectionType, hitTestResultMode, t2,
                                         recurs)).Any())
                    {
                        yield return te;
                        // we already found that the root element is a result, so we need not search further
                        yield break;
                    }
                }
                 
                // if this element fits the type filter, check if it fits the hit test rectangle
                if (e is TElement elt)
                {
                    var points = e.GetTransformedChildPoints(transform);
                    var box = RectangleF.FromPoints(points);

                    if (IsIntersect(selectionType))
                        box = box.InflateAndCopy(rectangle.Width, rectangle.Height);

                    if (IsIntersect(selectionType) && rectangle.IntersectsWith(box))
                    {
                        if (WithBoundingBoxOnly(selectionType) || e.IntersectsWith(rectangle, transform, maxRecursion))
                            yield return elt;
                    }
                    else if ((selectionType == SelectionType.Contain) && rectangle.Contains(box))
                        yield return elt;
                }
            }
        }

        internal static PointF[] GetTransformedElementPoints(this SvgVisualElement e, Matrix transform)
        {
            var b = e.Bounds;
            var p1 = PointF.Create(b.Left, b.Top);
            var p2 = PointF.Create(b.Right, b.Top);
            var p3 = PointF.Create(b.Right, b.Bottom);
            var p4 = PointF.Create(b.Left, b.Bottom);

            var pts = new[] { p1, p2, p3, p4 };

            foreach (SvgTransform transformation in e.Transforms)
            {
                transformation.ApplyTo(transform);
            }

            transform.TransformPoints(pts);
            return pts.Select(p => p.Clone()).ToArray();
        }

        internal static PointF[] GetTransformedChildPoints(this SvgVisualElement e, Matrix transform)
        {
            var pts = new List<PointF>();

            foreach (SvgTransform transformation in e.Transforms)
            {
                transformation.ApplyTo(transform);
            }

            foreach (var c in e.Children)
            {
                if (c is SvgVisualElement)
                {
                    var childBounds = ((SvgVisualElement)c).GetTransformedPoints(transform);
                    pts.AddRange(childBounds);
                }
            }

            if (pts.Count == 0)
                return Array.Empty<PointF>();
            
            return pts.Select(p => p.Clone()).ToArray();
        }

        /// <summary>
        /// provide the list of points, the transformed tap point and the matrix of the current element.
        /// Applies the matrix to all points and checks if the line intersects with a cube with center = "tap" and radius ="selectionWidthHeight"
        /// </summary>
        /// <param name="lines">the line segments to check</param>
        /// <param name="transform">the total transformation matrix for the current lines</param>
        /// <param name="hitTestArea">the area we want to intersect with the lines</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static bool IsIntersectingWithLine(this IList<(PointF from, PointF to)> lines, Matrix transform, RectangleF hitTestArea)
        {
            if (lines == null) throw new ArgumentNullException(nameof(lines));
            if (transform == null) throw new ArgumentNullException(nameof(transform));
            if (hitTestArea == null) throw new ArgumentNullException(nameof(hitTestArea));

            PointF tap = hitTestArea.GetCenterPoint();
            double selectionWidthHeight = hitTestArea.Width / 2;

            foreach (var lineSegment in lines)
            {
                transform.TransformPoints(new[] { lineSegment.from, lineSegment.to });
                if (IsLineHit(lineSegment.from, lineSegment.to, tap, selectionWidthHeight))
                    return true;
            }

            return false;
        }
        /// <summary>
        /// provide a line segment (from, to), the transformed tap point and the matrix of the current element.
        /// Applies the matrix to all points and checks if the line intersects with a cube with center = "tap" and radius ="selectionWidthHeight"
        /// </summary>
        /// <param name="lineSegment">the line segments to check</param>
        /// <param name="transform">the total transformation matrix for the current lines</param>
        /// <param name="hitTestArea">the area we want to intersect with the lines</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static bool IsIntersectingWithLine(this (PointF from, PointF to) lineSegment, Matrix transform, RectangleF hitTestArea)
        {
            if (transform == null) throw new ArgumentNullException(nameof(transform));
            if (hitTestArea == null) throw new ArgumentNullException(nameof(hitTestArea));

            PointF tap = hitTestArea.GetCenterPoint();
            double selectionWidthHeight = hitTestArea.Width / 2;


            transform.TransformPoints(new[] { lineSegment.from, lineSegment.to });

            if (IsLineHit(lineSegment.from, lineSegment.to, tap, selectionWidthHeight))
                return true;
         
            return false;
        }

        /// <summary>
        /// Google paste https://stackoverflow.com/a/13741803/333571
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="point"></param>
        /// <param name="selectionWidthHeight"></param>
        /// <returns></returns>
        private static bool IsLineHit(PointF start, PointF end, PointF point, double selectionWidthHeight)
        {
            PointF leftPoint;
            PointF rightPoint;

            // Normalize start/end to left right to make the offset calc simpler.
            if (start.X <= end.X)
            {
                leftPoint = start;
                rightPoint = end;
            }
            else
            {
                leftPoint = end;
                rightPoint = start;
            }

            // If point is out of bounds, no need to do further checks.                  
            if (point.X + selectionWidthHeight < leftPoint.X || rightPoint.X < point.X - selectionWidthHeight)
                return false;
            if (point.Y + selectionWidthHeight < Math.Min(leftPoint.Y, rightPoint.Y) ||
                Math.Max(leftPoint.Y, rightPoint.Y) < point.Y - selectionWidthHeight)
                return false;

            // https://de.wikipedia.org/wiki/Lineare_Funktion
            double deltaX = rightPoint.X - leftPoint.X;
            double deltaY = rightPoint.Y - leftPoint.Y;

            // If the line is straight, the earlier boundary check is enough to determine that the point is on the line.
            // Also prevents division by zero exceptions.
            if (deltaX == 0 || deltaY == 0)
                return true;

            double slope = deltaY / deltaX;
            double offset = leftPoint.Y - leftPoint.X * slope;
            double calculatedY = point.X * slope + offset;

            //adjustment of offset 
            double c = point.Y - offset - slope * point.X;

            double actualX = (point.Y - offset) / slope;
            double outOfBounds = point.X - actualX >= 0 ? point.X - actualX : actualX - point.X;

            if (Math.Abs(outOfBounds) > selectionWidthHeight && Math.Abs(c) > selectionWidthHeight)
            {
                return false;
            }

            calculatedY += c;

            //Check calculated Y matches the points Y coord with some easing.
            bool lineContains = point.Y - selectionWidthHeight <= calculatedY &&
                                calculatedY <= point.Y + selectionWidthHeight;

            return lineContains;
        }
    }
}
