using System.ComponentModel;
using Microsoft.Maui.Handlers;
using SkiaSharp.Views.UWP;
using SkiaSharp.Views.Windows;
using Svg.Editor.Forms;
using Svg.Editor.Views.UWP;

namespace SkiaSharp.Views.Forms
{
    public class UwpCanvasViewHandlerBase : ViewHandler<SvgCanvasEditorView, SKXamlCanvasX>
    {
        private UwpGestureRecognizer _gestureRecognizer;

        public static PropertyMapper<SvgCanvasEditorView, UwpCanvasViewHandlerBase> PropertyMapper =
                    new PropertyMapper<SvgCanvasEditorView, UwpCanvasViewHandlerBase>(ViewHandler.ViewMapper)
                    {
                        [nameof(SvgCanvasEditorView.IgnorePixelScaling)] = MapIgnorePixelScaling,

                    };

        public static CommandMapper<SvgCanvasEditorView, UwpCanvasViewHandlerBase> CommandMapper =
            new CommandMapper<SvgCanvasEditorView, UwpCanvasViewHandlerBase>()
            {
                [nameof(SvgCanvasEditorView.InvalidateSurface)] = OnInvalidateSurface,
                [nameof(SvgCanvasEditorView.PropertyChanged)] = OnPropertyChanged,
            };

        public UwpCanvasViewHandlerBase() : base(PropertyMapper, CommandMapper)
        {
        }

        protected override void ConnectHandler(SKXamlCanvasX platformView)
        {
            platformView.PaintSurface += new EventHandler<SKPaintSurfaceEventArgs>(OnPaintSurface);
            var controller = VirtualView as ISKCanvasViewController;
            controller.GetCanvasSize += OnGetCanvasSize;
            controller.SurfaceInvalidated += OnSurfaceInvalidated;

            platformView.Invalidate();

            _gestureRecognizer = new UwpGestureRecognizer(platformView);
            _gestureRecognizer.UserInputEvents.Subscribe(async uie => await VirtualView.DrawingCanvas.OnEvent(uie));
            VirtualView.DrawingCanvas.GestureRecognizer = _gestureRecognizer;

            base.ConnectHandler(platformView);
        }

        protected override void DisconnectHandler(SKXamlCanvasX platformView)
        {
            platformView.PaintSurface -= new EventHandler<SKPaintSurfaceEventArgs>(OnPaintSurface);
            var controller = VirtualView as ISKCanvasViewController;
            controller.GetCanvasSize -= OnGetCanvasSize;
            controller.SurfaceInvalidated -= OnSurfaceInvalidated;
            base.DisconnectHandler(platformView);
        }

        private static void OnPropertyChanged(UwpCanvasViewHandlerBase handler, SvgCanvasEditorView view, object arg)
        {
            if (arg is not PropertyChangedEventArgs e)
                return;

            if (e.PropertyName == nameof(SvgCanvasEditorView.IgnorePixelScaling))
            {
                handler.PlatformView.IgnorePixelScaling = view.IgnorePixelScaling;
            }
        }

        private void OnPaintSurface(object sender, Windows.SKPaintSurfaceEventArgs e)
        {
            var controller = this.VirtualView as ISKCanvasViewController;

            // the control is being repainted, let the user know
            controller?.OnPaintSurface(new Maui.SKPaintSurfaceEventArgs(e.Surface, e.Info));
        }

        private static void OnInvalidateSurface(UwpCanvasViewHandlerBase handler, SvgCanvasEditorView view, object arg)
        {
            handler.PlatformView.Invalidate();
        }


        private static void MapIgnorePixelScaling(UwpCanvasViewHandlerBase handler, SvgCanvasEditorView view)
        {
            handler.PlatformView.IgnorePixelScaling = view.IgnorePixelScaling;
        }



        protected override SKXamlCanvasX CreatePlatformView()
        {
            var view = Activator.CreateInstance<SKXamlCanvasX>();
            return view;
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