using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Skia;
using SkiaSharp;

namespace Svg.Editor.Avalon.Forms
{
    public class CustomControl : SKCanvasView
    {
        // the user can subscribe to repaint

        private bool _IgnorePixelScaling;

        // the native listens to this event
        public event EventHandler SurfaceInvalidated;
        public event EventHandler<GetCanvasSizeEventArgs> GetCanvasSize;


        // the user asks to repaint
        public override void InvalidateSurface()
        {
            base.InvalidateSurface();
        }

    }

    public class GetCanvasSizeEventArgs : EventArgs
    {
        public SKSize CanvasSize { get; set; }
    }
}