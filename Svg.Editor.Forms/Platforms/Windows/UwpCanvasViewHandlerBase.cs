using System;
using System.ComponentModel;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Handlers;
using SkiaSharp.Views.Maui.Controls;
using SkiaSharp.Views.UWP;
using SkiaSharp.Views.Windows;
using Svg.Editor.Forms;

namespace SkiaSharp.Views.Forms
{
    public class UwpCanvasViewHandlerBase : ViewHandler<SvgCanvasEditorView, SKXamlCanvasX>
    {

        public static PropertyMapper<SvgCanvasEditorView, UwpCanvasViewHandlerBase> Mapper =
            new PropertyMapper<SvgCanvasEditorView, UwpCanvasViewHandlerBase>(ViewHandler.ViewMapper)
            {

            };

        public UwpCanvasViewHandlerBase() : base(Mapper)
        {

        }

        public UwpCanvasViewHandlerBase(IPropertyMapper mapper, CommandMapper commandMapper = null) : base(mapper, commandMapper)
        {
        }

        protected override SKXamlCanvasX CreatePlatformView()
        {
            throw new NotImplementedException();
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