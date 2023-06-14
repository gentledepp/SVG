using System.ComponentModel;
using Microsoft.Maui.Handlers;
using Svg.Editor.Forms;
using Svg.Editor.Views.Droid;
using SKPaintSurfaceEventArgs = SkiaSharp.Views.Maui.SKPaintSurfaceEventArgs;

namespace SkiaSharp.Views.Forms
{
    public class DroidCanvasViewHandlerBase : ViewHandler<SvgCanvasEditorView, AndroidSvgCanvasEditorView>
    {

        public static PropertyMapper<SvgCanvasEditorView, DroidCanvasViewHandlerBase> PropertyMapper =
            new PropertyMapper<SvgCanvasEditorView, DroidCanvasViewHandlerBase>(ViewHandler.ViewMapper)
            {
                [nameof(SvgCanvasEditorView.IgnorePixelScaling)] = MapIgnorePixelScaling,
                [nameof(SvgCanvasEditorView.BindingContextChanged)] = OnBindingContextChanged,
                [nameof(SvgCanvasEditorView.ParentChanged)] = OnParentChanged

            };


        public static CommandMapper<SvgCanvasEditorView, DroidCanvasViewHandlerBase> CommandMapper =
            new CommandMapper<SvgCanvasEditorView, DroidCanvasViewHandlerBase>()
            {
                [nameof(SvgCanvasEditorView.InvalidateSurface)] = OnInvalidateSurface,
                [nameof(SvgCanvasEditorView.PropertyChanged)] = OnPropertyChanged,

            };


        public DroidCanvasViewHandlerBase() : base(PropertyMapper, CommandMapper)
        {
        }

        protected override void ConnectHandler(AndroidSvgCanvasEditorView platformView)
        {
            platformView.PaintSurface += new EventHandler<Android.SKPaintSurfaceEventArgs>(OnPaintSurface);
            var controller = VirtualView as ISKCanvasViewController;
            controller.GetCanvasSize += OnGetCanvasSize;
            controller.SurfaceInvalidated += OnSurfaceInvalidated;
            base.ConnectHandler(platformView);
        }

        protected override void DisconnectHandler(AndroidSvgCanvasEditorView platformView)
        {
            platformView.PaintSurface -= new EventHandler<Android.SKPaintSurfaceEventArgs>(OnPaintSurface);
            var controller = VirtualView as ISKCanvasViewController;
            controller.GetCanvasSize -= OnGetCanvasSize;
            controller.SurfaceInvalidated -= OnSurfaceInvalidated;
            platformView.Dispose();
            base.DisconnectHandler(platformView);
        }

        private static void OnParentChanged(DroidCanvasViewHandlerBase handler, SvgCanvasEditorView view)
        {
            handler.PlatformView.Invalidate();
            UpdateBinding(handler, view);
        }

        private static void OnPropertyChanged(DroidCanvasViewHandlerBase handler, SvgCanvasEditorView view, object arg)
        {
            if (arg is not PropertyChangedEventArgs e)
                return;

            if (e.PropertyName == nameof(SvgCanvasEditorView.IgnorePixelScaling))
            {
                handler.PlatformView.IgnorePixelScaling = view.IgnorePixelScaling;
            }

            UpdateBinding(handler, view);
        }

        protected override AndroidSvgCanvasEditorView CreatePlatformView()
        {
            var view = (AndroidSvgCanvasEditorView)Activator.CreateInstance(typeof(AndroidSvgCanvasEditorView), new object[] { Context, null });
            view.IsFormsMode = true;
            return view;
        }        
        
        

        private static void OnInvalidateSurface(DroidCanvasViewHandlerBase handler, SvgCanvasEditorView view, object args)
        {
            handler.PlatformView.Invalidate();
        }

        private static void OnBindingContextChanged(DroidCanvasViewHandlerBase handler, SvgCanvasEditorView view)
        {
            UpdateBinding(handler, view);
        }

        private static void MapIgnorePixelScaling(DroidCanvasViewHandlerBase handler, SvgCanvasEditorView view)
        {
            handler.PlatformView.IgnorePixelScaling = view.IgnorePixelScaling;
        }


        private void OnPaintSurface(object sender, Android.SKPaintSurfaceEventArgs e)
        {
            var controller = VirtualView as ISKCanvasViewController;

            // the control is being repainted, let the user know
            controller.OnPaintSurface(new SKPaintSurfaceEventArgs(e.Surface, e.Info));
        }


        private static void UpdateBinding(DroidCanvasViewHandlerBase handler, SvgCanvasEditorView view)
        {
            handler.PlatformView.DrawingCanvas = view.DrawingCanvas;
        }

        private void OnSurfaceInvalidated(object sender, EventArgs e)
        {
            PlatformView.Invalidate();
        }

        private void OnGetCanvasSize(object sender, GetCanvasSizeEventArgs e)
        {
            e.CanvasSize = PlatformView.CanvasSize;
        }
    }
}