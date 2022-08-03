//using System;
//using Android.Content;
//using JetBrains.Annotations;
//using Microsoft.Maui.Controls.Compatibility;
//using Microsoft.Maui.Controls.Platform;
//using SkiaSharp.Views.Forms;
//using Svg.Editor.Forms.Droid;
//using Svg.Editor.Views.Droid;
//using SKFormsView = Svg.Editor.Forms.SvgCanvasEditorView;

//namespace Svg.Editor.Forms.Droid
//{
//    public class DroidCanvasEditorViewRenderer : DroidCanvasViewRendererBase<SKFormsView, AndroidSvgCanvasEditorView>
//    {
//        //protected override void OnElementChanged(ElementChangedEventArgs<SKFormsView> e)
//        //{
//        //    // do clean up old control
//        //    if (Control != null)
//        //    {
//        //        Control.DrawingCanvas?.Dispose();
//        //        Control.DrawingCanvas = null;
//        //    }

//        //    // do clean up old element
//        //    if (Element != null)
//        //    {
//        //        var oldElement = (SKFormsView)Element;
//        //        oldElement.BindingContextChanged -= OnElementBindingContextChanged;
//        //    }

//        //    base.OnElementChanged(e);

//        //    // setup new element
//        //    if (e.NewElement != null)
//        //    {
//        //        var newElement = e.NewElement;
//        //        newElement.BindingContextChanged += OnElementBindingContextChanged;
//        //        UpdateBindings(newElement);
//        //    }
//        //}

//        //protected override AndroidSvgCanvasEditorView CreateNativeView()
//        //{
//        //    var nv = base.CreateNativeView();
//        //    nv.IsFormsMode = true;
//        //    return nv;
//        //}

//        //private void OnElementBindingContextChanged(object sender, EventArgs e)
//        //{
//        //    UpdateBindings(sender as BindableObject);
//        //}

//        //private void UpdateBindings(BindableObject fwe)
//        //{
//        //    if (fwe != null)
//        //    {
//        //        if (Control != null)
//        //        {
//        //            Control.DrawingCanvas?.Dispose();
//        //            Control.DrawingCanvas = fwe.BindingContext as SvgDrawingCanvas;
//        //        }
//        //    }
//        //}

//        //public DroidCanvasEditorViewRenderer(Context context) : base(context)
//        //{
//        //}
//        public DroidCanvasEditorViewRenderer([NotNull] IPropertyMapper mapper, CommandMapper commandMapper = null) : base(mapper, commandMapper)
//        {
//        }
//    }
//}
