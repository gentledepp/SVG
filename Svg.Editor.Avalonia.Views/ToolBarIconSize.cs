using Svg.Editor.Interfaces;
using System;
using Svg.Interfaces;

namespace Svg.Editor.Avalon.Views
{
    public class ToolBarIconSize : IToolbarIconSizeProvider
    {
        private static readonly Lazy<SizeF> _size = new Lazy<SizeF>(() =>
        {
            var scaling = 1.0;

            // Use similar scaling breakpoints as Android but adapted for Avalonia
            // LDPI equivalent
            if (scaling <= 0.75) return SizeF.Create(24, 24);
            // MDPI equivalent
            if (scaling <= 1.0) return SizeF.Create(32, 32);
            // HDPI equivalent
            if (scaling <= 1.5) return SizeF.Create(48, 48);
            // XHDPI equivalent
            if (scaling <= 2.0) return SizeF.Create(64, 64);
            // XXHDPI equivalent
            if (scaling <= 3.0) return SizeF.Create(96, 96);
            // XXXHDPI equivalent
            if (scaling <= 4.0) return SizeF.Create(128, 128);

            // Default fallback size
            return SizeF.Create(24, 24);
        });

        public SizeF GetSize()
        {
            return _size.Value;
        }
    }
}