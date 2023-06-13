using System;
using System.ComponentModel;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Handlers;
using Microsoft.UI.Xaml.Controls;
using SkiaSharp.Views.Maui.Controls;
using SkiaSharp.Views.UWP;
using SkiaSharp.Views.Windows;
using Svg.Editor.Forms;
using Svg.Editor.Interfaces;
using Svg.Editor.Views.UWP;

namespace SkiaSharp.Views.Forms
{
    public class UwpCanvasViewHandlerBase : ViewHandler<SvgCanvasEditorView, SKXamlCanvasX>
    {

        private static UwpGestureRecognizer _gestureRecognizer;

        public static PropertyMapper<SvgCanvasEditorView, UwpCanvasViewHandlerBase> Mapper =
            new PropertyMapper<SvgCanvasEditorView, UwpCanvasViewHandlerBase>(ViewHandler.ViewMapper)
            {
                [nameof(SvgCanvasEditorView.ParentChanged)] = OnPropertyChanged,
                [nameof(SvgCanvasEditorView.InvalidateSurface)] = OnInvalidateSurface,
                [nameof(SvgCanvasEditorView.IgnorePixelScaling)] = MapIgnorePixelScaling,
                ["GetCanvasSize"] = GetCanvasSize



            };

        private static void GetCanvasSize(UwpCanvasViewHandlerBase handler, SvgCanvasEditorView view)
        {
            //view.CanvasSize = handler.PlatformView?.CanvasSize ?? SKSize.Empty;
        }

        private static void MapIgnorePixelScaling(UwpCanvasViewHandlerBase handler, SvgCanvasEditorView view)
        {
            handler.PlatformView.IgnorePixelScaling = view.IgnorePixelScaling;
        }

        private static void OnInvalidateSurface(UwpCanvasViewHandlerBase handler, SvgCanvasEditorView view)
        {
            handler.PlatformView.Invalidate();
        }

        private static void OnPropertyChanged(UwpCanvasViewHandlerBase handler, SvgCanvasEditorView view)
        {
            if(handler.PlatformView == null)
                return;

            _gestureRecognizer = new UwpGestureRecognizer(handler.PlatformView);
            _gestureRecognizer.UserInputEvents.Subscribe(async uie => await view.DrawingCanvas.OnEvent(uie));
            view.DrawingCanvas.GestureRecognizer = _gestureRecognizer;
            view.DrawingCanvas = view.BindingContext as ISvgDrawingCanvas;
            handler.PlatformView.Invalidate();
        }



        public UwpCanvasViewHandlerBase() : base(Mapper)
        {

        }

        protected override void ConnectHandler(SKXamlCanvasX platformView)
        {
            platformView.PaintSurface += new EventHandler<SKPaintSurfaceEventArgs>(this.OnPaintSurface);
            base.ConnectHandler(platformView);
        }

        protected override void DisconnectHandler(SKXamlCanvasX platformView)
        {
            platformView.PaintSurface -= new EventHandler<SKPaintSurfaceEventArgs>(this.OnPaintSurface);
            base.DisconnectHandler(platformView);
        }

        private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            var controller = this.VirtualView as ISKCanvasViewController;

            // the control is being repainted, let the user know
            controller?.OnPaintSurface(new Maui.SKPaintSurfaceEventArgs(e.Surface, e.Info));
        }

        protected override SKXamlCanvasX CreatePlatformView()
        {
            var view = Activator.CreateInstance<SKXamlCanvasX>();
            return view;
        }
        //protected override void OnElementChanged(ElementChangedEventArgs<TFormsView> e)
        //{
        //    if (e.OldElement != null)
        //    {
        //        var oldController = (ISKCanvasViewController)e.OldElement;

        //        // unsubscribe from events
        //        oldController.SurfaceInvalidated -= OnSurfaceInvalidated;
        //        oldController.GetCanvasSize -= OnGetCanvasSize;
        //    }
        //    if (Control != null)
        //    {
        //        Control.PaintSurface -= OnPaintSurface;
        //    }

        //    if (e.NewElement != null)
        //    {
        //        var newController = (ISKCanvasViewController)e.NewElement;

        //        // create the native view
        //        var view = CreateNativeView();
        //        view.IgnorePixelScaling = e.NewElement.IgnorePixelScaling;
        //        view.PaintSurface += OnPaintSurface;
        //        SetNativeControl(view);

        //        // subscribe to events from the user
        //        newController.SurfaceInvalidated += OnSurfaceInvalidated;
        //        newController.GetCanvasSize += OnGetCanvasSize;

        //        // paint for the first time
        //        if (Control != null) Control.Invalidate();
        //    }

        //    base.OnElementChanged(e);
        //}

        //protected virtual TNativeView CreateNativeView()
        //{
        //    var view = Activator.CreateInstance<TNativeView>();
        //    return view;
        //}

        //protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        //{
        //    base.OnElementPropertyChanged(sender, e);

        //    if (e.PropertyName == nameof(SKFormsView.IgnorePixelScaling))
        //    {
        //        if (Control != null) Control.IgnorePixelScaling = Element.IgnorePixelScaling;
        //    }
        //}

        //protected override void Dispose(bool disposing)
        //{
        //    // detach all events before disposing
        //    var controller = (ISKCanvasViewController)Element;
        //    if (controller != null)
        //    {
        //        controller.SurfaceInvalidated -= OnSurfaceInvalidated;
        //        controller.GetCanvasSize -= OnGetCanvasSize;
        //    }

        //    base.Dispose(disposing);
        //}

        //private void OnSurfaceInvalidated(object sender, EventArgs eventArgs)
        //{
        //    // repaint the native control
        //    if (Control != null) Control.Invalidate();
        //}

        //// the user asked for the size
        //private void OnGetCanvasSize(object sender, GetCanvasSizeEventArgs e)
        //{
        //    e.CanvasSize = Control?.CanvasSize ?? SKSize.Empty;
        //}

        //private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        //{
        //    var controller = this.Element as ISKCanvasViewController;

        //    // the control is being repainted, let the user know
        //    controller?.OnPaintSurface(new Maui.SKPaintSurfaceEventArgs(e.Surface, e.Info));
        //}

    }
}