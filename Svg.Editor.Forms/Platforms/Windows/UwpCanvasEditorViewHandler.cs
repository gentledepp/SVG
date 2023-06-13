//using System;
//using Microsoft.Maui.Controls;
//using Microsoft.Maui.Controls.Compatibility;
//using Microsoft.Maui.Controls.Platform;
//using SkiaSharp.Views.Forms;
//using SkiaSharp.Views.UWP;
//using Svg.Editor.Forms;
//using Svg.Editor.Views.UWP;

//namespace Svg.Editor.Forms.UWP
//{
//    //public class UwpCanvasEditorViewHandler : UwpCanvasViewHandlerBase<SvgCanvasEditorView, SKXamlCanvasX>
//    //{
//        private UwpGestureRecognizer _gestureRecognizer;

//    protected override void OnElementChanged(ElementChangedEventArgs<SvgCanvasEditorView> e)
//    {
//        base.OnElementChanged(e);

//        if (Control != null)
//        {
//            _gestureRecognizer = new UwpGestureRecognizer(Control);
//            _gestureRecognizer.UserInputEvents.Subscribe(async uie => await Element.DrawingCanvas.OnEvent(uie));
//            Element.DrawingCanvas.GestureRecognizer = _gestureRecognizer;
//        }
//    }

//    protected override void Dispose(bool disposing)
//    {
//        base.Dispose(disposing);

//        _gestureRecognizer.Dispose();
//    }
//    //}
//}