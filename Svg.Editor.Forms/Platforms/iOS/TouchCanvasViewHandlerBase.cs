using System.ComponentModel;
using Microsoft.Maui.Handlers;
using SkiaSharp.Views.iOS;
using Svg.Editor;
using Svg.Editor.Forms;
using Svg.Editor.Views.iOS;

namespace SkiaSharp.Views.Forms
{
    public class TouchCanvasViewHandlerBase : ViewHandler<SvgCanvasEditorView, TouchSvgCanvasEditorView>
    {

        public static PropertyMapper<SvgCanvasEditorView, TouchCanvasViewHandlerBase> PropertyMapper =
            new PropertyMapper<SvgCanvasEditorView, TouchCanvasViewHandlerBase>(ViewHandler.ViewMapper)
            {
                [nameof(SvgCanvasEditorView.IgnorePixelScaling)] = MapIgnorePixelScaling,
                [nameof(SvgCanvasEditorView.BindingContextChanged)] = OnBindingContextChanged,
                [nameof(SvgCanvasEditorView.ParentChanged)] = OnParentChanged

            };

        public static CommandMapper<SvgCanvasEditorView, TouchCanvasViewHandlerBase> CommandMapper =
            new CommandMapper<SvgCanvasEditorView, TouchCanvasViewHandlerBase>()
            {
                [nameof(SvgCanvasEditorView.InvalidateSurface)] = OnInvalidateSurface,
                [nameof(SvgCanvasEditorView.PropertyChanged)] = OnPropertyChanged,
            };

        public TouchCanvasViewHandlerBase() : base(PropertyMapper, CommandMapper)
        {
        }

        protected override void ConnectHandler(TouchSvgCanvasEditorView platformView)
        {
            platformView.PaintSurface += new EventHandler<SKPaintSurfaceEventArgs>(OnPaintSurface);
            var controller = VirtualView as ISKCanvasViewController;
            controller.GetCanvasSize += OnGetCanvasSize;
            controller.SurfaceInvalidated += OnSurfaceInvalidated;
            base.ConnectHandler(platformView);
        }

        protected override void DisconnectHandler(TouchSvgCanvasEditorView platformView)
        {
            platformView.PaintSurface -= new EventHandler<SKPaintSurfaceEventArgs>(OnPaintSurface);
            var controller = VirtualView as ISKCanvasViewController;
            controller.GetCanvasSize -= OnGetCanvasSize;
            controller.SurfaceInvalidated -= OnSurfaceInvalidated;
            platformView.Dispose();
            base.DisconnectHandler(platformView);
        }

        private static void OnParentChanged(TouchCanvasViewHandlerBase handler, SvgCanvasEditorView view)
        {
            handler.PlatformView.SetNeedsDisplay();
            UpdateBinding(handler, view);
        }

        private static void OnPropertyChanged(TouchCanvasViewHandlerBase handler, SvgCanvasEditorView view, object arg)
        {
            if (arg is not PropertyChangedEventArgs e)
                return;

            if (e.PropertyName == nameof(SvgCanvasEditorView.IgnorePixelScaling))
            {
                handler.PlatformView.IgnorePixelScaling = view.IgnorePixelScaling;
            }

            UpdateBinding(handler, view);
        }

        private void OnPaintSurface(object sender, iOS.SKPaintSurfaceEventArgs e)
        {
            var controller = this.VirtualView as ISKCanvasViewController;

            // the control is being repainted, let the user know
            controller?.OnPaintSurface(new Maui.SKPaintSurfaceEventArgs(e.Surface, e.Info));
        }

        private static void OnInvalidateSurface(TouchCanvasViewHandlerBase handler, SvgCanvasEditorView view, object arg)
        {
            handler.PlatformView.SetNeedsDisplay();
        }

        private static void OnBindingContextChanged(TouchCanvasViewHandlerBase handler, SvgCanvasEditorView view)
        {
            UpdateBinding(handler, view);
        }

        private static void MapIgnorePixelScaling(TouchCanvasViewHandlerBase handler, SvgCanvasEditorView view)
        {
            handler.PlatformView.IgnorePixelScaling = view.IgnorePixelScaling;
        }

        private static void UpdateBinding(TouchCanvasViewHandlerBase handler, SvgCanvasEditorView view)
        {
            handler.PlatformView.DrawingCanvas = view.BindingContext as SvgDrawingCanvas;
        }


        protected override TouchSvgCanvasEditorView CreatePlatformView()
        {
            var view = Activator.CreateInstance<TouchSvgCanvasEditorView>();
            return view;
        }

        private void OnSurfaceInvalidated(object sender, EventArgs e)
        {
            PlatformView.SetNeedsDisplay();
        }

        private void OnGetCanvasSize(object sender, GetCanvasSizeEventArgs e)
        {
            e.CanvasSize = PlatformView.CanvasSize;
        }
    }
}