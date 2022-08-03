using System;
using SkiaSharp.Views.Maui;


namespace SkiaSharp.Views
{
    public interface IPaintSurface
    {
        event EventHandler<SKPaintSurfaceEventArgs> PaintSurface;
    }
}
