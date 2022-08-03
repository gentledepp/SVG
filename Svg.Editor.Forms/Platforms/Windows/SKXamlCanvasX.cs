using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Windows;
using SKPaintSurfaceEventArgs = SkiaSharp.Views.Maui.SKPaintSurfaceEventArgs;

namespace SkiaSharp.Views.UWP
{
    public class SKXamlCanvasX : SKXamlCanvas, IPaintSurface
    {
        public event EventHandler<SKPaintSurfaceEventArgs> PaintSurface;
    }
}
