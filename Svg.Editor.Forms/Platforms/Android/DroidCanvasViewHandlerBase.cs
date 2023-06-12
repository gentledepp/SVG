using System;
using System.ComponentModel;
using Android.Content;
using JetBrains.Annotations;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Handlers;
using SkiaSharp.Views.Maui;
using Svg.Editor;
using Svg.Editor.Forms;
using Svg.Editor.Interfaces;
using Svg.Editor.Views.Droid;

namespace SkiaSharp.Views.Forms
{
    public class DroidCanvasViewHandlerBase : ViewHandler<SvgCanvasEditorView, AndroidSvgCanvasEditorView>
    {

        public static PropertyMapper<SvgCanvasEditorView, DroidCanvasViewHandlerBase> Mapper =
            new PropertyMapper<SvgCanvasEditorView, DroidCanvasViewHandlerBase>(ViewHandler.ViewMapper)
            {
                //[nameof(SvgCanvasEditorView.IgnorePixelScaling)] = MapIgnorePixelScaling,
                //[nameof(SvgCanvasEditorView.PropertyChanged)] = UpdateBinding

            };

        //private static void UpdateBinding(DroidCanvasViewHandlerBase handler, SvgCanvasEditorView view)
        //{
        //    handler.PlatformView.Invalidate();
        //    handler.PlatformView.DrawingCanvas = view.BindingContext as SvgDrawingCanvas;
        //}

        //private static void MapIgnorePixelScaling(DroidCanvasViewHandlerBase handler, SKCanvasViewX view)
        //{
        //    handler.PlatformView.IgnorePixelScaling = view.IgnorePixelScaling;
        //}

        //protected void OnElementChanged(ElementChangedEventArgs<SvgCanvasEditorView> e)
        //{
        //    if (e.OldElement != null)
        //    {
        //        var oldController = (ISKCanvasViewController)e.OldElement;

        //        //unsubscribe from events
        //        oldController.SurfaceInvalidated -= OnSurfaceInvalidated;
        //        oldController.GetCanvasSize -= OnGetCanvasSize;
        //    }

        //    PlatformView.PaintSurface -= OnPaintSurface;
            

        //    if (e.NewElement != null)
        //    {
        //        var newController = (ISKCanvasViewController)e.NewElement;

        //        //create the native view
        //        var view = CreateNativeView();
        //        view.IgnorePixelScaling = e.NewElement.IgnorePixelScaling;
        //        view.PaintSurface += OnPaintSurface;

        //        //subscribe to events from the user
        //        newController.SurfaceInvalidated += OnSurfaceInvalidated;
        //        newController.GetCanvasSize += OnGetCanvasSize;

        //        //paint for the first time

        //        PlatformView.Invalidate();

        //        // setup new element
        //        if (e.NewElement != null)
        //        {
        //            var newElement = e.NewElement;
        //            newElement.BindingContextChanged += OnElementBindingContextChanged;
        //            UpdateBindings(newElement);
        //        }
        //    }

        //}

        protected virtual AndroidSvgCanvasEditorView CreateNativeView()
        {
            var view = (AndroidSvgCanvasEditorView)Activator.CreateInstance(typeof(AndroidSvgCanvasEditorView), new object[] { Context, null });
            view.IsFormsMode = true;
            return view;
        }


        //protected override void Dispose(bool disposing)
        //{
        //    //detach all events before disposing
        //    //var controller = (ISKCanvasViewController);
        //    //if (controller != null)
        //    //{
        //    //    controller.SurfaceInvalidated -= OnSurfaceInvalidated;
        //    //}

        //    base.Dispose(disposing);
        //}

        private void OnSurfaceInvalidated(object sender, EventArgs eventArgs)
        {
            //repaint the native control
            PlatformView.Invalidate();
        }

        //the user asked for the size
        //private void OnGetCanvasSize(object sender, GetCanvasSizeEventArgs e)
        //{
        //    e.CanvasSize = PlatformView?.CanvasSize ?? SKSize.Empty;
        //}

        private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
        //    var controller = this.Element as ISKCanvasViewController;

        //    //the control is being repainted, let the user know
        //    controller?.OnPaintSurface(new SKPaintSurfaceEventArgs(e.Surface, e.Info));
        }

        public DroidCanvasViewHandlerBase([NotNull] IPropertyMapper mapper, CommandMapper commandMapper = null) : base(mapper, commandMapper)
        {
            mapper = Mapper;
        }

        protected override AndroidSvgCanvasEditorView CreatePlatformView()
        {
            var view = (AndroidSvgCanvasEditorView)Activator.CreateInstance(typeof(AndroidSvgCanvasEditorView), new object[] { Context, null }); 
            return view;
        }

        //protected override SKCanvasViewX CreatePlatformView()
        //{
        //    var view = (SKCanvasViewX)Activator.CreateInstance(typeof(SKCanvasViewX), new object[] { Context, null });
        //    return view;
        //}
    }
}