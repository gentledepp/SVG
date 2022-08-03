using System;
using System.ComponentModel;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform;
using SkiaSharp.Views.Maui;
using SKFormsView = SkiaSharp.Views.Forms.SKCanvasViewX;
using SKNativeView = SkiaSharp.Views.UWP.SKXamlCanvasX;

namespace SkiaSharp.Views.Forms
{
    public class UwpCanvasViewRendererBase<TFormsView, TNativeView> : ViewRenderer<TFormsView, TNativeView>
        where TNativeView : SKNativeView, IPaintSurface
        where TFormsView : SKFormsView
    {
        protected override void OnElementChanged(ElementChangedEventArgs<TFormsView> e)
        {
            if (e.OldElement != null)
            {
                var oldController = (ISKCanvasViewController)e.OldElement;

                // unsubscribe from events
                oldController.SurfaceInvalidated -= OnSurfaceInvalidated;
                oldController.GetCanvasSize -= OnGetCanvasSize;
            }
            if (Control != null)
            {
                Control.PaintSurface -= OnPaintSurface;
            }

            if (e.NewElement != null)
            {
                var newController = (ISKCanvasViewController)e.NewElement;

                // create the native view
                var view = CreateNativeView();
                view.IgnorePixelScaling = e.NewElement.IgnorePixelScaling;
                view.PaintSurface += OnPaintSurface;
                SetNativeControl(view);

                // subscribe to events from the user
                newController.SurfaceInvalidated += OnSurfaceInvalidated;
                newController.GetCanvasSize += OnGetCanvasSize;

                // paint for the first time
                Control.Invalidate();
            }

            base.OnElementChanged(e);
        }

        protected virtual TNativeView CreateNativeView()
        {
            var view = Activator.CreateInstance<TNativeView>();
            return view;
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == nameof(SKFormsView.IgnorePixelScaling))
            {
                Control.IgnorePixelScaling = Element.IgnorePixelScaling;
            }
        }

        protected override void Dispose(bool disposing)
        {
            // detach all events before disposing
            var controller = (ISKCanvasViewController)Element;
            if (controller != null)
            {
                controller.SurfaceInvalidated -= OnSurfaceInvalidated;
                controller.GetCanvasSize -= OnGetCanvasSize;
            }

            base.Dispose(disposing);
        }

        private void OnSurfaceInvalidated(object sender, EventArgs eventArgs)
        {
            // repaint the native control
            Control.Invalidate();
        }

        // the user asked for the size
        private void OnGetCanvasSize(object sender, GetCanvasSizeEventArgs e)
        {
            e.CanvasSize = Control?.CanvasSize ?? SKSize.Empty;
        }

        private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            var controller = this.Element as ISKCanvasViewController;

            // the control is being repainted, let the user know
            controller?.OnPaintSurface(new SKPaintSurfaceEventArgs(e.Surface, e.Info));
        }
    }
}