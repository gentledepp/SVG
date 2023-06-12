using System;
using SkiaSharp.Views.Windows;


namespace SkiaSharp.Views
{
    public interface IPaintSurface
    {
        event EventHandler<SKPaintSurfaceEventArgs> PaintSurface;
    }
}
