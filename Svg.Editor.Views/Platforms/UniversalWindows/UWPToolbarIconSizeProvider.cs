using System;
using Svg.Editor.Interfaces;
using Svg.Interfaces;

namespace Svg.Editor.Views.UWP
{
    internal class UWPToolbarIconSizeProvider : IToolbarIconSizeProvider
    {
        private static readonly Lazy<SizeF> _size = new Lazy<SizeF>(() => SizeF.Create(32, 32));

        public SizeF GetSize()
        {
            return _size.Value;
        }
    }
}
