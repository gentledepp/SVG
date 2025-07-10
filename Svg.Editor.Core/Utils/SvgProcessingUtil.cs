using System;
using System.Linq;
using Svg.Interfaces;

namespace Svg.Editor.Utils
{
    public static class SvgProcessingUtil
    {
        public static Action<SvgDocument> ColorAction(Color color)
        {
            return document =>
            {
                document.Children.First(x => x is SvgVisualElement).Fill = new SvgColourServer(color);
            };
        }
    }
}
