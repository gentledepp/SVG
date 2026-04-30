using System;
using System.Collections.Generic;
using System.Text;
using Svg.Interfaces;

namespace Svg.Basic_Shapes
{
    public static class SvgVisualElementExtensions
    {
        public static RectangleF GetBoundsWithClipPaths(this SvgVisualElement element)
        {
            var bounds = element.Path(null).GetBounds();

            SvgElement current = element;
            while (current != null)
            {
                if (current is SvgVisualElement visual && visual.ClipPath != null)
                {
                    SvgClipPath clipPath = element.OwnerDocument.GetElementById<SvgClipPath>(visual.ClipPath.OriginalString);
                    if (clipPath != null && (clipPath.Bounds.Width < bounds.Width || clipPath.Bounds.Height < bounds.Height))
                    {
                        return clipPath.Bounds;
                    }
                }
                current = current.Parent;
            }

            return bounds;
        }
    }
}
