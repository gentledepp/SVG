using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using Avalonia.Threading;
using Avalonia.Utilities;
using SkiaSharp;
//using SkiaSharp.Views.Desktop;

namespace Svg.Editor.Avalonia.Forms
{
    public class CustomControl : Control
    {
        // the user can subscribe to repaint
        //public event EventHandler<SKPaintSurfaceEventArgs> PaintSurface;
        private bool _IgnorePixelScaling;

        // the native listens to this event
        public event EventHandler SurfaceInvalidated;
        public event EventHandler<GetCanvasSizeEventArgs> GetCanvasSize;

        // the user asks the for the size
        public SKSize CanvasSize
        {
            get
            {
                // send a mesage to the native view
                var args = new GetCanvasSizeEventArgs();
                GetCanvasSize?.Invoke(this, args);
                return args.CanvasSize;
            }
        }

        public bool IgnorePixelScaling
        {
            get => this._IgnorePixelScaling;
            set
            {
                this._IgnorePixelScaling = value;
            }
        }

        // the user asks to repaint
        public void InvalidateSurface()
        {
            // send a mesage to the native view
            SurfaceInvalidated?.Invoke(this, EventArgs.Empty);
        }


        //// the native view tells the user to repaint
        //protected virtual void OnPaintSurface(SKPaintSurfaceEventArgs e)
        //{
        //    PaintSurface?.Invoke(this, e);
        //}

    }

    public class GetCanvasSizeEventArgs : EventArgs
    {
        public SKSize CanvasSize { get; set; }
    }
}