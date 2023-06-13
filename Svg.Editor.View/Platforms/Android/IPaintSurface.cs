using System;
using SkiaSharp.Views.Android;

namespace SkiaSharp.Views
{
    public interface IPaintSurface
    {
        event EventHandler<SKPaintSurfaceEventArgs> PaintSurface;
    }
}
