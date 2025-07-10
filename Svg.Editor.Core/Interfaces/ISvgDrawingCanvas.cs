using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using Svg.Editor.Events;
using Svg.Editor.Gestures;
using Svg.Editor.Tools;
using Svg.Interfaces;

namespace Svg.Editor.Interfaces
{
    public interface ISvgDrawingCanvas
    {
        SvgDocument Document { get; set; }
        bool DocumentIsDirty { get; }
        ObservableCollection<SvgVisualElement> SelectedElements { get; }
        ObservableCollection<ITool> Tools { get; }
        ObservableCollection<IEnumerable<IToolCommand>> ToolCommands { get; }
        List<Func<SvgVisualElement, Task>> DefaultEditors { get; }
        PointF Translate { get; set; }
        float ZoomFactor { get; set; }
        PointF ZoomFocus { get; set; }
        bool ZoomEnabled { get; set; }
        int ScreenWidth { get; set; }
        int ScreenHeight { get; set; }
        PointF ScreenCenter { get; }
        RectangleF Constraints { get; set; }
        ConstraintsMode ConstraintsMode { get; set; }

        /// <summary>
        /// If enabled, adds a DebugTool that brings some helpful visualizations
        /// </summary>
        bool IsDebugEnabled { get; set; }

        ITool ActiveTool { get; set; }
        Color BackgroundColor { get; set; }
        Func<SvgVisualElement, bool> DefaultHitTestFilter { get; set; }
        IGestureRecognizer GestureRecognizer { set; }
        event EventHandler CanvasInvalidated;
        event EventHandler ToolCommandsChanged;

        /// <summary>
        /// Called by the platform specific input event detector whenever the user interacts with the model
        /// </summary>
        /// <param name="ev"></param>
        Task OnEvent(UserInputEvent ev);

        /// <summary>
        /// Called when a gesture - like tap, double tap, long press, etc - is recognized.
        /// </summary>
        /// <param name="gesture"></param>
        Task OnGesture(UserGesture gesture);

        /// <summary>
        /// Called by platform specific implementation to allow tools to draw something onto the canvas
        /// </summary>
        /// <param name="renderer"></param>
        Task OnDraw(IRenderer renderer);

        Task EnsureInitialized();
        Bitmap CreateBitmap(int width, int height);

        /// <summary>
        /// Returns a rectangle with width and height 20px that surrounds the given point
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        RectangleF GetPointerRectangle(PointF p);

        /// <summary>
        /// the selection rectangle must be in absolute screen coordinates (so not transformed by canvas.Translate or canvas.ZoomFactor)
        /// </summary>
        /// <param name="selectionRectangle"></param>
        /// <param name="selectionType"></param>
        /// <param name="maxItems"></param>
        /// <param name="recursionLevel"></param>
        /// <returns></returns>
        IList<TElement> GetElementsUnder<TElement>(RectangleF selectionRectangle, 
            SelectionType selectionType,
            HitTestResultMode resultMode = HitTestResultMode.ReturnRootElementOnly, 
            int maxItems = int.MaxValue, 
            int recursionLevel = Int32.MaxValue)
            where TElement : SvgVisualElement;

        IList<TElement> GetElementsUnder<TElement>(
            RectangleF selectionRectangle,
            SelectionType selectionType,
            HitTestResultMode resultMode,
            Func<SvgVisualElement, bool> filter,
            int maxItems = int.MaxValue,
            int recursionLevel = Int32.MaxValue)
            where TElement : SvgVisualElement;
        /// <summary>
        /// gets all visual elements under the given pointer (a 20px rectangle surrounding the given point to simulate thick finger)
        /// </summary>
        /// <param name="pointer1Position"></param>
        /// <param name="recursionLevel"></param>
        /// <returns></returns>
        IList<TElement> GetElementsUnderPointer<TElement>(PointF pointer1Position,
            SelectionType selectionType = SelectionType.Intersect,
            int recursionLevel = Int32.MaxValue)
            where TElement : SvgVisualElement;

        Task AddItemInScreenCenter(SvgDocument document);
        Task AddItemInScreenCenter(SvgVisualElement element);
        Matrix GetCanvasTransformationMatrix();
        PointF CanvasToScreen(float x, float y);
        PointF CanvasToScreen(PointF canvasPointF);
        PointF ScreenToCanvas(float x, float y);
        PointF ScreenToCanvas(PointF screenPointF);

        /// <summary>
        /// Stores the document with a viewbox that surrounds all contained visual elements
        /// then resets the viewbox
        /// </summary>
        /// <param name="stream"></param>
        void SaveDocumentWithBoundsAsViewbox(Stream stream);

        /// <summary>
        /// Stores the document with a viewbox that surrounds the current screen capture
        /// then resets the viewbox
        /// </summary>
        /// <param name="stream"></param>
        void SaveDocumentWithScreenAsViewbox(Stream stream);

        Bitmap CaptureDocumentBitmap(int maxSize = 4096, Color backgroundColor = null);
        Bitmap CaptureScreenBitmap(Color backgroundColor = null);
        void FireInvalidateCanvas();
        void FireToolCommandsChanged();
        void Dispose();
        event PropertyChangedEventHandler PropertyChanged;
        string GetToolPropertiesJson();
        void LoadTools(params Func<ITool>[] tools);
    }
}