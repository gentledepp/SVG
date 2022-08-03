//using System;
//using System.ComponentModel;
//using Android.Content;
//using JetBrains.Annotations;
//using Microsoft.Maui.Controls.Compatibility.Platform.Android;
//using Microsoft.Maui.Controls.Platform;
//using Microsoft.Maui.Handlers;
//using SkiaSharp.Views.Maui;
//using SKFormsView = SkiaSharp.Views.Forms.SKCanvasViewX;
//using SKNativeView = SkiaSharp.Views.Android.SKCanvasView;

//namespace SkiaSharp.Views.Forms
//{
//    public class DroidCanvasViewRendererBase : ViewHandler<SKFormsView, SKNativeView>
//    {

//        public static PropertyMapper<SKFormsView, DroidCanvasViewRendererBase> Mappper =
//            new PropertyMapper<SKCanvasViewX, DroidCanvasViewRendererBase>(ViewHandler.ViewMapper)
//            {
//                [nameof(SKFormsView.IgnorePixelScaling)] = MapIgnorePixelScaling,
//            };

//        private static void MapIgnorePixelScaling(DroidCanvasViewRendererBase handler, SKCanvasViewX view)
//        {
//            handler.PlatformView.IgnorePixelScaling = view.IgnorePixelScaling;
//        }

//        protected void OnElementChanged(ElementChangedEventArgs<SKFormsView> e)
//        {
//            if (e.OldElement != null)
//            {
//                var oldController = (ISKCanvasViewController)e.OldElement;

//                //unsubscribe from events
//                oldController.SurfaceInvalidated -= OnSurfaceInvalidated;
//                oldController.GetCanvasSize -= OnGetCanvasSize;
//            }
            
//            PlatformView.PaintSurface -= OnPaintSurface;
            

//            if (e.NewElement != null)
//            {
//                var newController = (ISKCanvasViewController)e.NewElement;

//                //create the native view
//                var view = CreateNativeView();
//                view.IgnorePixelScaling = e.NewElement.IgnorePixelScaling;
//                view.PaintSurface += OnPaintSurface;
//                SetNativeControl(view);

//                //subscribe to events from the user
//                newController.SurfaceInvalidated += OnSurfaceInvalidated;
//                newController.GetCanvasSize += OnGetCanvasSize;

//                //paint for the first time

//               PlatformView.Invalidate();
//            }

//            base.OnElementChanged(e);
//        }

//        protected virtual TNativeView CreateNativeView()
//        {
//            var view = (TNativeView)Activator.CreateInstance(typeof(TNativeView), new object[] { Context, null });
//            return view;
//        }

//        protected override void Dispose(bool disposing)
//        {
//            //detach all events before disposing
//           var controller = (ISKCanvasViewController)Element;
//            if (controller != null)
//            {
//                controller.SurfaceInvalidated -= OnSurfaceInvalidated;
//            }

//            base.Dispose(disposing);
//        }

//        private void OnSurfaceInvalidated(object sender, EventArgs eventArgs)
//        {
//            //repaint the native control
//            PlatformView.Invalidate();
//        }

//        //the user asked for the size
//        private void OnGetCanvasSize(object sender, GetCanvasSizeEventArgs e)
//        {
//            e.CanvasSize = PlatformView?.CanvasSize ?? SKSize.Empty;
//        }

//        private void OnPaintSurface(object sender, Android.SKPaintSurfaceEventArgs e)
//        {
//            var controller = this.Element as ISKCanvasViewController;

//            //the control is being repainted, let the user know
//            controller?.OnPaintSurface(new SKPaintSurfaceEventArgs(e.Surface, e.Info));
//        }

//        public DroidCanvasViewRendererBase([NotNull] IPropertyMapper mapper, CommandMapper commandMapper = null) : base(mapper, commandMapper)
//        {
//        }

//        protected override SKNativeView CreatePlatformView()
//        {
//            var view = (SKNativeView)Activator.CreateInstance(typeof(SKNativeView), new object[] { Context, null });
//                return view;
//        }
//    }
//}