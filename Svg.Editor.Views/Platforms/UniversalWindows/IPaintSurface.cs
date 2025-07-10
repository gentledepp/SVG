using System;
using SkiaSharp.Views.UWP;

namespace SkiaSharp.Views
{
    public interface IPaintSurface
    {
        event EventHandler<SKPaintSurfaceEventArgs> PaintSurface;
    }
}
