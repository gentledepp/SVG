using System;
using Svg.Editor.Interfaces;
using Svg.Interfaces;

namespace Svg.Editor.Views.Droid
{
    internal class AndroidToolbarIconSizeProvider : IToolbarIconSizeProvider
    {
        private static Lazy<SizeF> _size = new Lazy<SizeF>(() =>
        {
            var d= Android.App.Application.Context.Resources.DisplayMetrics.Density;
            // according to: http://iconhandbook.co.uk/reference/chart/android/
            // and: http://stackoverflow.com/questions/5099550/how-to-check-an-android-device-is-hdpi-screen-or-mdpi-screen
            // return 0.75 if it's LDPI
            if (d <= 0.75f)
                return SizeF.Create(24, 24);
            // return 1.0 if it's MDPI
            if (d <= 1.0f)
                return SizeF.Create(32, 32);
            // return 1.5 if it's HDPI
            if (d <= 1.5f)
                return SizeF.Create(48, 48);
            // return 2.0 if it's XHDPI
            if (d <= 2.0f)
                return SizeF.Create(64, 64);
            // return 3.0 if it's XXHDPI
            if (d <= 3.0f)
                return SizeF.Create(96, 96);
            // return 4.0 if it's XXXHDPI
            if (d <= 4.0)
                return SizeF.Create(128, 128);

            return SizeF.Create(24, 24);
        });
        

        public SizeF GetSize()
        {
            return _size.Value;
        }
    }
}