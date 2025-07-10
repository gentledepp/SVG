using System;
using SkiaSharp;
#if WINDOWS_UWP
using SkiaSharp.Views.UWP;
#elif PLATFORM_ANDROID
using SkiaSharp.Views.Android;
#else
using SkiaSharp.Views.iOS;
#endif

namespace SkiaSharp.Views
{
    public interface IPaintSurface
    {
        event EventHandler<SKPaintSurfaceEventArgs> PaintSurface;
    }
}
