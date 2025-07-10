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
            if (element.ClipPath != null)
            {
                SvgClipPath clipPath = element.OwnerDocument.GetElementById<SvgClipPath>(element.ClipPath.OriginalString);
                if (clipPath != null && (clipPath.Bounds.Width < bounds.Width || clipPath.Bounds.Height < bounds.Width))
                {
                    return clipPath.Bounds;
                }
            }

            return bounds;
        }
    }
}
