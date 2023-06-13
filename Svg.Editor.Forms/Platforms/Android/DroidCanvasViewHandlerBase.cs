using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Handlers;
using SkiaSharp.Views.Android;
using Svg.Editor;
using Svg.Editor.Forms;
using Svg.Editor.Views.Droid;
using SKPaintSurfaceEventArgs = SkiaSharp.Views.Maui.SKPaintSurfaceEventArgs;

namespace SkiaSharp.Views.Forms
{
    public class DroidCanvasViewHandlerBase : ViewHandler<SvgCanvasEditorView, SKCanvasView>
    {

        public static PropertyMapper<SvgCanvasEditorView, DroidCanvasViewHandlerBase> Mapper =
            new PropertyMapper<SvgCanvasEditorView, DroidCanvasViewHandlerBase>(ViewHandler.ViewMapper)
            {
                [nameof(SvgCanvasEditorView.IgnorePixelScaling)] = MapIgnorePixelScaling,
                //[nameof(SvgCanvasEditorView.ParentChanged)] = OnPropertyChanged,
                //[nameof(SvgCanvasEditorView.InvalidateSurface)] = OnInvalidateSurface
            };

        private static void OnPropertyChanged(SKCanvasView handler, SvgCanvasEditorView view)
        {
            //UpdateBinding(handler, view);
        }

        private static void OnInvalidateSurface(SKCanvasView handler, SvgCanvasEditorView view)
        {
            //handler.PlatformView.Invalidate();
        }


        protected override void ConnectHandler(SKCanvasView platformView)
        {
            platformView.PaintSurface += new EventHandler<Android.SKPaintSurfaceEventArgs>(this.OnPaintSurface);

            base.ConnectHandler(platformView);
        }

        protected override void DisconnectHandler(SKCanvasView platformView)
        {
            platformView.PaintSurface -= new EventHandler<Android.SKPaintSurfaceEventArgs>(this.OnPaintSurface);

            base.DisconnectHandler(platformView);
        }

        private void OnPaintSurface(object sender, Android.SKPaintSurfaceEventArgs e)
        {
            var controller = this.VirtualView as ISKCanvasViewController;

            // the control is being repainted, let the user know
            controller?.OnPaintSurface(new SKPaintSurfaceEventArgs(e.Surface, e.Info));
        }

        private static void UpdateBinding(DroidCanvasViewHandlerBase handler, SvgCanvasEditorView view)
        {
            handler.PlatformView.Invalidate();
            //handler.PlatformView.DrawingCanvas?.Dispose();
            //handler.PlatformView.DrawingCanvas = view.BindingContext as SvgDrawingCanvas;
        }

        //private void UpdateBinding(object sender, EventArgs e)
        //{
        //    PlatformView.DrawingCanvas?.Dispose();
        //    PlatformView.DrawingCanvas = VirtualView.BindingContext as SvgDrawingCanvas;
        //}


        private static void MapIgnorePixelScaling(DroidCanvasViewHandlerBase handler, SKCanvasViewX view)
        {
            handler.PlatformView.IgnorePixelScaling = view.IgnorePixelScaling;
        }

        protected static void OnElementChanged(ElementChangedEventArgs<SvgCanvasEditorView> e)
        {
            //if (e.OldElement != null)
            //{
            //    var oldController = (ISKCanvasViewController)e.OldElement;

            //    //unsubscribe from events
            //    oldController.SurfaceInvalidated -= OnSurfaceInvalidated;
            //    oldController.GetCanvasSize -= OnGetCanvasSize;
            //}

            //PlatformView.PaintSurface -= OnPaintSurface;


            //if (e.NewElement != null)
            //{
            //    var newController = (ISKCanvasViewController)e.NewElement;

            //    //create the native view
            //    var view = CreateNativeView();
            //    view.IgnorePixelScaling = e.NewElement.IgnorePixelScaling;
            //    view.PaintSurface += OnPaintSurface;

            //    //subscribe to events from the user
            //    newController.SurfaceInvalidated += OnSurfaceInvalidated;
            //    newController.GetCanvasSize += OnGetCanvasSize;

            //    //paint for the first time

            //    PlatformView.Invalidate();

            //    // setup new element
            //    if (e.NewElement != null)
            //    {
            //        var newElement = e.NewElement;
            //        newElement.BindingContextChanged += UpdateBindings;
            //        UpdateBindings(newElement);
            //    }
            //}

        }

        public DroidCanvasViewHandlerBase() : base(Mapper)
        {
        }

        protected override SKCanvasView CreatePlatformView()
        {
            var view = new SKCanvasView(Context, null);
            return view;
        }
    }
}