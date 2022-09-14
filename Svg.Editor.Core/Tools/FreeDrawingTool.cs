using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Svg.Editor.Extensions;
using Svg.Editor.Gestures;
using Svg.Editor.Interfaces;
using Svg.Editor.UndoRedo;
using Svg.Interfaces;
using Svg.Pathing;

namespace Svg.Editor.Tools
{
    public class FreeDrawingTool : UndoableToolBase
    {
        #region Private fields and properties

        private SvgPath _currentPath;
        private PointF _lastCanvasPointerPosition;
        private bool _isActive;

        #endregion

        #region Public properties

        public const string DefaultStrokeWidthKey = "defaultstrokewidth";

        public string LineStyleIconName { get; set; } = "ic_line_style.svg";

        public override bool IsActive
        {
            get { return _isActive; }
            set
            {
                if (_isActive == value) return;
                _isActive = value;
                if (!_isActive) return;
                Canvas.SelectedElements.Clear();
                Canvas.FireInvalidateCanvas();
            }
        }

        public int DefaultStrokeWidth
        {
            get
            {
                object defaultStrokeWidth;
                return Properties.TryGetValue(DefaultStrokeWidthKey, out defaultStrokeWidth) ? Convert.ToInt32(defaultStrokeWidth) : 2;
            }
            set { Properties[DefaultStrokeWidthKey] = value; }
        }

        #endregion

        public FreeDrawingTool(IDictionary<string, object> properties, IUndoRedoService undoRedoService) : base("Free draw", properties, undoRedoService)
        {
            IconName = "ic_brush.svg";
            ToolUsage = ToolUsage.Explicit;
            HandleDragExit = true;
        }

        #region Overrides

        public override void OnDocumentChanged(SvgDocument oldDocument, SvgDocument newDocument)
        {
            if (oldDocument != null) UnWatchDocument(oldDocument);
            WatchDocument(newDocument);
        }

        protected override async Task OnDrag(DragGesture drag)
        {
            await base.OnDrag(drag);

            if (!IsActive) return;

            if (drag.State == DragState.Exit)
            {
                _currentPath = null;
                _lastCanvasPointerPosition = null;
                return;
            }

            var canvasStartPosition = Canvas.ScreenToCanvas(drag.Start);
            var canvasPointerPosition = Canvas.ScreenToCanvas(drag.Position);

            if (_currentPath == null)
            {

                _currentPath = new SvgPath
                {
                    Stroke = new SvgColourServer(SvgEngine.Factory.CreateColorFromArgb(255, 0, 0, 0)),
                    Fill = SvgPaintServer.None,
                    PathData = new SvgPathSegmentList(new List<SvgPathSegment> { new SvgMoveToSegment(canvasStartPosition) }),
                    StrokeLineCap = SvgStrokeLineCap.Round,
                    StrokeLineJoin = SvgStrokeLineJoin.Round,
                    StrokeWidth = new SvgUnit(SvgUnitType.Pixel, DefaultStrokeWidth),
                    FillOpacity = 0
                };

                _currentPath.AddConstraints(NoSnappingConstraint, NoFillConstraint);

                var capturedCurrentPath = _currentPath;
                UndoRedoService.ExecuteCommand(new UndoableActionCommand("Add new freedrawing path", o =>
                {
                    Canvas.Document.Children.Add(capturedCurrentPath);
                    Canvas.FireInvalidateCanvas();
                }, o =>
                {
                    Canvas.Document.Children.Remove(capturedCurrentPath);
                    Canvas.FireInvalidateCanvas();
                }));
            }

            // Quadratic bezier curve to the approximate of the pointer position
            var nextControlPoint = _lastCanvasPointerPosition ?? _currentPath.PathData.Last.End;
            var nextEndPoint = (nextControlPoint + canvasPointerPosition) / 2;

            _currentPath.PathData.Add(new SvgQuadraticCurveSegment(_currentPath.PathData.Last.End, nextControlPoint, nextEndPoint));

            _lastCanvasPointerPosition = canvasPointerPosition;

            Canvas.FireInvalidateCanvas();
        }

        #endregion

        #region Private helpers

        private void UnWatchDocument(SvgDocument svgDocument)
        {
            svgDocument.ChildRemoved -= SvgDocumentOnChildRemoved;
        }

        private void WatchDocument(SvgDocument svgDocument)
        {
            svgDocument.ChildRemoved -= SvgDocumentOnChildRemoved;
            svgDocument.ChildRemoved += SvgDocumentOnChildRemoved;
        }

        private void SvgDocumentOnChildRemoved(object sender, ChildRemovedEventArgs args)
        {
            if (IsActive && args.RemovedChild == _currentPath)
            {
                _currentPath = null;
                Canvas.FireInvalidateCanvas();
            }
        }

        #endregion

        #region Inner types

        #endregion
    }
}
