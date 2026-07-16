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
                var box = e.GetTransformedElementBounds(transform);

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
                    var box = e.GetTransformedChildBounds(transform);

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

        internal static RectangleF GetTransformedElementBounds(this SvgVisualElement e, Matrix transform)
        {
            var b = e.Bounds;

            foreach (SvgTransform transformation in e.Transforms)
            {
                transformation.ApplyTo(transform);
            }

            return transform.TransformRectangle(b);
        }

        internal static RectangleF GetTransformedChildBounds(this SvgVisualElement e, Matrix transform)
        {
            RectangleF totalBounds = null;

            foreach (SvgTransform transformation in e.Transforms)
            {
                transformation.ApplyTo(transform);
            }

            foreach (var c in e.Children)
            {
                if (c is SvgVisualElement)
                {
                    var childBounds = ((SvgVisualElement)c).GetBoundingBox(transform);

                    if (totalBounds is null)
                        totalBounds = childBounds;
                    else
                        totalBounds.Union(childBounds);
                }
            }

            if(totalBounds is null)
                return RectangleF.Empty;

            return totalBounds;
        }

        /// <summary>
        /// provide the list of points, the transformed tap point and the matrix of the current element.
        /// Applies the matrix to all points and checks if the line intersects with a cube with center = "tap" and radius ="selectionWidthHeight"
        /// </summary>
        /// <param name="lines">the line segments to check</param>
        /// <param name="transform">the total transformation matrix for the current lines</param>
        /// <param name="hitTestArea">the area we want to intersect with the lines</param>
        /// <param name="extraToleranceInLocalUnits">
        /// additional tolerance, in the same (untransformed) local units as <paramref name="lines"/>, to widen the hit area by.
        /// Used to account for a shape's own rendered stroke width ("fat finger" tolerance for thin/curved borders), so callers
        /// don't need to manually scale it - it is converted to screen space using <paramref name="transform"/>'s scale.
        /// </param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static bool IsIntersectingWithLine(this IList<(PointF from, PointF to)> lines, Matrix transform, RectangleF hitTestArea, double extraToleranceInLocalUnits = 0)
        {
            if (lines == null) throw new ArgumentNullException(nameof(lines));
            if (transform == null) throw new ArgumentNullException(nameof(transform));
            if (hitTestArea == null) throw new ArgumentNullException(nameof(hitTestArea));

            PointF tap = hitTestArea.GetCenterPoint();
            double selectionWidthHeight = hitTestArea.Width / 2 + transform.ScaleTolerance(extraToleranceInLocalUnits);

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
        /// <param name="extraToleranceInLocalUnits">
        /// additional tolerance, in the same (untransformed) local units as <paramref name="lineSegment"/>, to widen the hit area by.
        /// Used to account for a shape's own rendered stroke width ("fat finger" tolerance for thin/curved borders), so callers
        /// don't need to manually scale it - it is converted to screen space using <paramref name="transform"/>'s scale.
        /// </param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static bool IsIntersectingWithLine(this (PointF from, PointF to) lineSegment, Matrix transform, RectangleF hitTestArea, double extraToleranceInLocalUnits = 0)
        {
            if (transform == null) throw new ArgumentNullException(nameof(transform));
            if (hitTestArea == null) throw new ArgumentNullException(nameof(hitTestArea));

            PointF tap = hitTestArea.GetCenterPoint();
            double selectionWidthHeight = hitTestArea.Width / 2 + transform.ScaleTolerance(extraToleranceInLocalUnits);


            transform.TransformPoints(new[] { lineSegment.from, lineSegment.to });

            if (IsLineHit(lineSegment.from, lineSegment.to, tap, selectionWidthHeight))
                return true;

            return false;
        }

        /// <summary>
        /// Converts a tolerance expressed in local (untransformed) units into screen-space units, using the
        /// average of the matrix's X/Y scale factors. Used to bring a shape's own StrokeWidth (a local-space
        /// value) into the same space as the (already screen-space) hit test tolerance.
        ///
        /// ASSUMPTION - KNOWN INACCURATE UNDER ROTATION: transform.ScaleX/ScaleY are the raw m00/m11 matrix
        /// components, not a decomposed scale magnitude - they only equal the true axis scale for a
        /// non-rotated (and, for this average, roughly uniform) transform. Under rotation they mix with
        /// SkewX/SkewY (e.g. both collapse toward 0 at a 90° rotation), and under strongly non-uniform scale
        /// the average over/under-estimates one axis, so the resulting tolerance band is wrong in either case.
        /// This is NOT just a theoretical edge case: the `transform` passed in here is not only the canvas's
        /// zoom+pan matrix - HitTestInternal clones the incoming transform and mutates it with each element's
        /// own SvgTransform (and any ancestor group's, accumulated during recursion) before calling
        /// IntersectsWith. So a shape with its own transform="rotate(...)" attribute (e.g. via RotationTool)
        /// hits this exact code path today, and its "fat finger" tolerance silently shrinks toward 0 near a
        /// 90/270-degree rotation. Fix: use a rotation-invariant scale, e.g. sqrt(|ScaleX*ScaleY - SkewX*SkewY|)
        /// (the determinant-based uniform scale, which reduces to this average for a pure uniform/non-rotated
        /// scale but stays correct under rotation).
        /// </summary>
        private static double ScaleTolerance(this Matrix transform, double toleranceInLocalUnits)
        {
            if (toleranceInLocalUnits <= 0)
                return 0;

            var scale = (Math.Abs(transform.ScaleX) + Math.Abs(transform.ScaleY)) / 2.0;
            return toleranceInLocalUnits * scale;
        }

        /// <summary>
        /// Half of the element's own rendered stroke width, in local (untransformed) units, or 0 if it has no
        /// stroke. StrokeWidth is resolved via ToDeviceValue (same conversion RenderStroke uses) rather than
        /// its raw .Value, so non-pixel units (%, em, cm, ...) produce a meaningful tolerance instead of just
        /// the bare number - no renderer is available during hit testing, so font-relative units (em/ex) fall
        /// back to ToDeviceValue's built-in default-font-size guess.
        /// Meant to be passed as extra tolerance to <see cref="IsIntersectingWithLine(IList{(PointF,PointF)}, Matrix, RectangleF, double)"/>
        /// so that tapping anywhere within a thick border's visible band counts as a hit, not just the exact
        /// geometric outline ("fat finger" tolerance for border-only hit testing on unfilled shapes).
        /// </summary>
        internal static float GetStrokeHitTestTolerance(this SvgVisualElement element)
        {
            return element.HasStroke() ? element.StrokeWidth.ToDeviceValue(null, UnitRenderingType.Other, element) / 2 : 0;
        }

        private const int EllipticalOutlineSegmentCount = 36;

        /// <summary>
        /// Shared "is a tap near the outline" check for SvgCircle and SvgEllipse (a circle is just an ellipse
        /// with rx == ry == r). Approximates the outline as a polygon, since there is no closed-form
        /// line-intersection test for a curve like there is for straight-edged shapes (rectangle/polygon/path).
        /// </summary>
        internal static bool IntersectsWithEllipticalOutline(this SvgVisualElement element, RectangleF rectangle, Matrix transform, float cx, float cy, float rx, float ry)
        {
            if (element.HasFill())
                return true;

            var lineSegments = new List<(PointF from, PointF to)>();
            PointF previous = null;
            for (var i = 0; i <= EllipticalOutlineSegmentCount; i++)
            {
                var angle = 2 * Math.PI * i / EllipticalOutlineSegmentCount;
                var current = PointF.Create(
                    (float)(cx + rx * Math.Cos(angle)),
                    (float)(cy + ry * Math.Sin(angle)));

                if (previous != null)
                    lineSegments.Add((previous.Clone(), current.Clone()));

                previous = current;
            }

            return lineSegments.IsIntersectingWithLine(transform, rectangle, element.GetStrokeHitTestTolerance());
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
