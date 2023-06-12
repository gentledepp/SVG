using SkiaSharp.Views.Windows;

namespace SkiaSharp.Views.UWP
{
    public class SKXamlCanvasX : SKXamlCanvas, IPaintSurface
    {
        public event EventHandler<SKPaintSurfaceEventArgs> PaintSurface;
    }
}
