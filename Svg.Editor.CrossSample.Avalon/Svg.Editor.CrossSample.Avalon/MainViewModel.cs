using System.Collections.Generic;
using Svg.Editor.Avalon.Forms.Services;
using Svg.Editor.Avalon.Forms.Tools;
using Svg.Editor.Interfaces;
using Svg.Editor.Sample.Avalon.Resources.svg;


using Svg.Editor.Tools;

namespace Svg.Editor.CrossSample.Avalon
{
    public class MainViewModel
    {
        public MainViewModel()
        {
            #region Register services

            SvgEngine.Resolve<ISvgCachingService>().Clear();

            // register pseudo-type which is only used to mark an assembly that has embedded resources (Resources\svg)
            SvgEngine.Resolve<IEmbeddedResourceRegistry>().Register(typeof(ResourceMarker));

            #endregion

            #region Register tools

            // this part should be in the designer, when the iCL is created
            var gridToolProperties = new Dictionary<string, object>
                    {
                        { "angle", 30.0f },
                        { "stepsizey", 20.0f },
                        { "issnappingenabled", false }
                    };

            var colorToolProperties = new Dictionary<string, object>
            {
                { ColorTool.SelectableColorsKey, new [] { "#000000","#FF0000","#00FF00","#0000FF","#FFFF00","#FF00FF","#00FFFF", "#FFFFFF" } },
                { ColorTool.SelectableColorNamesKey, new [] { "Black","Red","Green","Blue","Yellow","Magenta","Cyan","White" } },
            };

            var strokeStyleToolProperties = new Dictionary<string, object>
            {
                { StrokeStyleTool.StrokeDashesKey, new[] {"none", "3 3", "17 17", "34 34"} },
                { StrokeStyleTool.StrokeDashNamesKey, new [] { "----------", "- - - - - -", "--  --  --", "---   ---" } },
                { StrokeStyleTool.StrokeWidthsKey, new [] { 2, 6, 12, 24 } },
                { StrokeStyleTool.StrokeWidthNamesKey, new [] { "thin", "normal", "thick", "ultra-thick" } }
            };

            var lineToolProperties = new Dictionary<string, object>
            {
                { "markerstartids", new [] { "none", "arrowStart", "circle" } },
                { "markerstartnames", new [] { "---", "<--", "O--" } },
                { "markerendids", new [] { "none", "arrowEnd", "circle" } },
                { "markerendnames", new [] { "---", "-->", "--O" } },
                { LineTool.SelectedMarkerEndIndexKey, 1 },
                { LineTool.DefaultStrokeWidthKey, 2 }
            };

            var freeDrawToolProperties = new Dictionary<string, object>
            {
                { FreeDrawingTool.DefaultStrokeWidthKey, 12 }
            };

            var textToolProperties = new Dictionary<string, object>
            {
                { "fontsizes", new [] { 12f, 16f, 20f, 24f, 36f, 48f } },
                { "selectedfontsizeindex", 1 },
                { "fontsizenames", new [] { "12px", "16px", "20px", "24px", "36px", "48px" } }
            };

            var zoomToolProperties = new Dictionary<string, object>();

            var panToolProperties = new Dictionary<string, object>();

            var placeAsBackgroundToolProperties = new Dictionary<string, object>
            {
                { PlaceAsBackgroundTool.ChooseBackgroundEnabledKey, true }
            };

            var rotationToolProperties = new Dictionary<string, object>
            {
                { RotationTool.RotationStepKey, 45.0f },
                //{ RotationTool.FilterKey, (Func<SvgVisualElement, bool>) (e => e is SvgTextBase) }
            };

            var undoRedoService = SvgEngine.Resolve<IUndoRedoService>();

            var pinToolProperties = new Dictionary<string, object>
            {
                {"pinsizenames", new[] {"Small", "Medium", "Large", "ExtraLarge" } }
            };

            var polygonProperties = new Dictionary<string, object>
            {
                {"submit", false }
            };

            #endregion

            DrawingCanvas = new SvgDrawingCanvas();
            DrawingCanvas.LoadTools(
                () => new GridTool(gridToolProperties, undoRedoService),
                () => new MoveTool(undoRedoService),
                () => new PanTool(panToolProperties),
                () => new RotationTool(rotationToolProperties, undoRedoService),
                () => new ZoomTool(zoomToolProperties),
                () => new SelectionTool(undoRedoService),
                () => new TextTool(textToolProperties, undoRedoService),
                () => new LineTool(lineToolProperties, undoRedoService),
                () => new EllipseTool(null, undoRedoService),
                () => new RectangleTool(null, undoRedoService),
                () => new FreeDrawingTool(freeDrawToolProperties, undoRedoService),
                () => new ColorTool(colorToolProperties, undoRedoService),
                () => new StrokeStyleTool(strokeStyleToolProperties, undoRedoService),
                () => new UndoRedoTool(undoRedoService),
                () => new ArrangeTool(undoRedoService),
                () => new SaveTool(false),
                () => new PlaceAsBackgroundTool(placeAsBackgroundToolProperties, undoRedoService),
                () => new AddItemTool(),
                () => new PolygonTool(polygonProperties, undoRedoService),
                () => new PinTool(pinToolProperties, undoRedoService));

        }

        public SvgDrawingCanvas DrawingCanvas { get; set; }

        public void OnDisappearing()
        {
            DrawingCanvas.Dispose();
        }
    }
}
