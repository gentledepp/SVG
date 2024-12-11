using System;
using Svg.Editor.Interfaces;
using Svg.Interfaces;
using UIKit;

namespace Svg.Editor.Views.iOS
{
    internal class TouchToolbarIconSizeProvider : IToolbarIconSizeProvider
    {
        private static readonly Lazy<SizeF> _size = new Lazy<SizeF>(() =>
        {
            var scale = (float)UIScreen.MainScreen.Scale;
            if (scale == 1)
                return SizeF.Create(22, 22);
            if (scale == 2f)
                return SizeF.Create(44, 44);
            if (scale == 3f)
                return SizeF.Create(66, 66);

            return SizeF.Create(22, 22);
        });

        public SizeF GetSize()
        {
            return _size.Value;
        }
    }
}
