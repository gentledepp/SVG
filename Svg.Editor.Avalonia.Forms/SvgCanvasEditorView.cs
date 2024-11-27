
using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using SkiaSharp;
//using SkiaSharp.Views.Desktop;
using Svg.Editor.Avalonia.Views.Platforms.Windows;
using Svg.Editor.Interfaces;
using Svg.Editor.Services;

namespace Svg.Editor.Avalonia.Forms;

public class SvgCanvasEditorView : CustomControl
{
    private UwpGestureRecognizer _gestureRecognizer;

    public ISvgDrawingCanvas DrawingCanvas
    {
        get { return DataContext as ISvgDrawingCanvas; }
        set { DataContext = value; }
    }

    public SvgCanvasEditorView()
    {

    }

    protected override void OnInitialized()
    {
        _gestureRecognizer = new UwpGestureRecognizer(this);
        _gestureRecognizer.UserInputEvents.Subscribe(async uie => await DrawingCanvas.OnEvent(uie));
        DrawingCanvas.GestureRecognizer = _gestureRecognizer;
        var canvas = DrawingCanvas;
        if(canvas != null)
            RegisterCallbacks();

        base.OnInitialized();
    }


    protected override void OnDataContextChanged(EventArgs e)
    {
        RegisterCallbacks();
        base.OnDataContextChanged(e);
    }

    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        UnregisterCallbacks();
        base.OnDetachedFromLogicalTree(e);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        UnregisterCallbacks();
        _gestureRecognizer.Dispose();
        base.OnDetachedFromVisualTree(e);
    }

    private void UnregisterCallbacks()
    {
        var canvas = DrawingCanvas;
        if (canvas == null)
            return;

        canvas.CanvasInvalidated -= DrawingCanvas_CanvasInvalidated;
        canvas.ToolCommandsChanged -= DrawingCanvas_ToolCommandsChanged;

        SurfaceInvalidated -= OnSurfaceInvalidated;
        GetCanvasSize -= OnGetCanvasSize;

        _gestureRecognizer.Dispose();

    }

    private void RegisterCallbacks()
    {
        var canvas = DrawingCanvas;
        if (canvas == null)
            return;
        
        SurfaceInvalidated += OnSurfaceInvalidated;
        GetCanvasSize += OnGetCanvasSize;
        canvas.CanvasInvalidated += DrawingCanvas_CanvasInvalidated;
        canvas.ToolCommandsChanged += DrawingCanvas_ToolCommandsChanged;
    }

    private void OnSurfaceInvalidated(object sender, EventArgs eventArgs)
    {
        // repaint the native control
        //this.Invalidate();
    }

    // the user asked for the size
    private void OnGetCanvasSize(object sender, GetCanvasSizeEventArgs e)
    {
        e.CanvasSize = this?.CanvasSize ?? SKSize.Empty;
    }


    private void DrawingCanvas_ToolCommandsChanged(object sender, System.EventArgs e)
    {
    }

    private void DrawingCanvas_CanvasInvalidated(object sender, System.EventArgs e)
    {
        InvalidateSurface();
    }

    //protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
    //{
    //    base.OnPaintSurface(e);

    //    DrawingCanvas?.OnDraw(new SKCanvasRenderer(e.Surface, e.Info.Width, e.Info.Height));
    //}
}