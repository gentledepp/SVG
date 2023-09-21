using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Svg.Editor.Extensions;
using Svg.Editor.Gestures;
using Svg.Editor.Interfaces;
using Svg.Editor.UndoRedo;
using Svg.Interfaces;
using Svg.Pathing;

namespace Svg.Editor.Tools
{
    public abstract class ShapeToolBase<T> : UndoableToolBase where T : SvgVisualElement
    {

        protected const double MaxPointerDistance = 20.0;
        private Brush _brush;
        private bool _isActive;
        private ITool _activatedFrom;
        private PointF _translate;

        protected PointF AnchorPosition;
        protected PointF Offset;
        protected MovementHandle Handle;
        protected Brush BlueBrush => _brush ?? (_brush = SvgEngine.Factory.CreateSolidBrush(SvgEngine.Factory.CreateColorFromArgb(255, 80, 210, 210)));
        protected IEnumerable<SvgMarker> Markers { get; }

        protected T CurrentShape;

        public ShapeToolBase(string name, IDictionary<string, object> properties, IUndoRedoService undoRedoService) : base(name, properties, undoRedoService)
        {
            ToolUsage = ToolUsage.Explicit;
            ToolType = ToolType.Create;
            HandleDragExit = true;

            var markers = new List<SvgMarker>();
            var marker = new SvgMarker { ID = "arrowStart", Orient = new SvgOrient() { IsAuto = true }, RefX = new SvgUnit(SvgUnitType.Pixel, -2.5f), MarkerWidth = 2 };
            marker.Children.Add(new SvgPath
            {
                PathData = new SvgPathSegmentList(new SvgPathSegment[]
                {
                    new SvgMoveToSegment(PointF.Create(0, -2.0f)),
                    new SvgLineSegment(PointF.Create(0, -2.0f), PointF.Create(0, 2f)),
                    new SvgLineSegment(PointF.Create(0, 2.0f), PointF.Create(-4.0f, 0)),
                    new SvgClosePathSegment()
                }),
                Stroke = SvgColourServer.ContextStroke, // inherit stroke color from parent/aka context
                Fill = SvgColourServer.ContextFill, // inherit stroke color from parent/aka context
            });
            markers.Add(marker);
            marker = new SvgMarker { ID = "arrowEnd", Orient = new SvgOrient { IsAuto = true }, RefX = new SvgUnit(SvgUnitType.Pixel, 2.5f), MarkerWidth = 2 };
            marker.Children.Add(new SvgPath
            {
                PathData = new SvgPathSegmentList(new SvgPathSegment[]
                {
                    new SvgMoveToSegment(PointF.Create(0, -2.0f)),
                    new SvgLineSegment(PointF.Create(0, -2.0f), PointF.Create(0, 2.0f)),
                    new SvgLineSegment(PointF.Create(0, 2.0f), PointF.Create(4.0f, 0)),
                    new SvgClosePathSegment()
                }),
                Stroke = SvgColourServer.ContextStroke, // inherit stroke color from parent/aka context
                Fill = SvgColourServer.ContextFill, // inherit stroke color from parent/aka context
            });
            markers.Add(marker);
            marker = new SvgMarker { ID = "circle", Orient = new SvgOrient() { IsAuto = true }/*, RefX = new SvgUnit(SvgUnitType.Pixel, -1.5f)*/, MarkerWidth = 2 };
            marker.Children.Add(new SvgEllipse
            {
                RadiusX = 1.5f,
                RadiusY = 1.5f,
                Stroke = SvgColourServer.ContextStroke, // inherit stroke color from parent/aka context
                Fill = SvgColourServer.ContextFill, // inherit stroke color from parent/aka context
            });
            markers.Add(marker);

            Markers = markers;
        }

        public override bool IsActive
        {
            get { return _isActive; }
            set
            {
                if (_isActive == value)
                    return;
                _isActive = value;
                // if tool was activated, reduce selection to a single line and set it as current line
                CurrentShape = _isActive ? Canvas.SelectedElements.OfType<T>().FirstOrDefault() : null;
                Canvas.SelectedElements.Clear();
                if (CurrentShape != null)
                    Canvas.SelectedElements.Add(CurrentShape);
                Canvas.FireInvalidateCanvas();
            }
        }

        protected abstract T CreateShape(PointF relativeStart);

        protected abstract void ApplyGesture(DragGesture drag, PointF canvasEnd);

        protected abstract void DrawCurrentShape(IRenderer renderer, ISvgDrawingCanvas ws);

        public override void OnDocumentChanged(SvgDocument oldDocument, SvgDocument newDocument)
        {
            if (oldDocument != null) UnWatchDocument(oldDocument);
            WatchDocument(newDocument);
            InitializeDefinitions(newDocument);
        }

        protected override async Task OnTap(TapGesture tap)
        {
            await base.OnTap(tap);

            if (!IsActive) return;

            var pointerRect = Canvas.GetPointerRectangle(tap.Position);
            CurrentShape = Canvas.GetElementsUnder<T>(pointerRect, SelectionType.Intersect).FirstOrDefault();

            if (CurrentShape != null)
            {
                Canvas.SelectedElements.Clear();
                Canvas.SelectedElements.Add(CurrentShape);
            }
            else
            {
                Canvas.SelectedElements.Clear();

                if (_activatedFrom != null)
                {
                    Canvas.ActiveTool = _activatedFrom;
                    _activatedFrom = null;
                }
            }

            Canvas.FireToolCommandsChanged();
            Canvas.FireInvalidateCanvas();
        }

        protected override async Task OnDoubleTap(DoubleTapGesture doubleTap)
        {
            await base.OnDoubleTap(doubleTap);

            if (Canvas.ActiveTool.ToolType != ToolType.Select) return;

            var ellipse = Canvas.GetElementsUnderPointer<T>(doubleTap.Position).FirstOrDefault();
            if (ellipse != null)
            {
                Canvas.SelectedElements.Clear();
                Canvas.SelectedElements.Add(ellipse);

                // save the active tool for restoring later
                _activatedFrom = Canvas.ActiveTool;
                Canvas.ActiveTool = this;
                Canvas.FireInvalidateCanvas();
            }
        }

        protected override async Task OnDrag(DragGesture drag)
        {
            await base.OnDrag(drag);

            if (!IsActive) return;

            if (drag.State == DragState.Exit)
            {
                Handle = MovementHandle.None;
                Offset = null;
                _translate = null;
                return;
            }

            var canvasStart = Canvas.ScreenToCanvas(drag.Start);
            var canvasEnd = Canvas.ScreenToCanvas(drag.Position);

            if (CurrentShape == null)
            {
                SelectShape(CreateShape(canvasStart));

                // capture variables for use in lambda
                var children = Canvas.Document.Children;
                var capturedCurrentLine = CurrentShape;
                UndoRedoService.ExecuteCommand(new UndoableActionCommand("Add new ellipse", o =>
                {
                    children.Add(capturedCurrentLine);
                    Canvas.FireInvalidateCanvas();
                }, o =>
                {
                    children.Remove(capturedCurrentLine);
                    Canvas.FireInvalidateCanvas();
                }));

                Handle = MovementHandle.BottomRight;
                AnchorPosition = canvasStart;
            }
            else
            {
                var boundingBox = CurrentShape.GetBoundingBox();

                if (Handle == MovementHandle.None)
                {
                    var canvasPointer1Position = canvasStart;
                    // determine the handle (position) where the pointer was put down
                    Handle = Math.Abs(canvasPointer1Position.X - boundingBox.Left) * Canvas.ZoomFactor <= MaxPointerDistance &&
                                 Math.Abs(canvasPointer1Position.Y - boundingBox.Top) * Canvas.ZoomFactor <= MaxPointerDistance ? MovementHandle.TopLeft :
                                 Math.Abs(canvasPointer1Position.X - boundingBox.Right) * Canvas.ZoomFactor <= MaxPointerDistance &&
                                 Math.Abs(canvasPointer1Position.Y - boundingBox.Top) * Canvas.ZoomFactor <= MaxPointerDistance ? MovementHandle.TopRight :
                                 Math.Abs(canvasPointer1Position.X - boundingBox.Right) * Canvas.ZoomFactor <= MaxPointerDistance &&
                                 Math.Abs(canvasPointer1Position.Y - boundingBox.Bottom) * Canvas.ZoomFactor <= MaxPointerDistance ? MovementHandle.BottomRight :
                                 Math.Abs(canvasPointer1Position.X - boundingBox.Left) * Canvas.ZoomFactor <= MaxPointerDistance &&
                                 Math.Abs(canvasPointer1Position.Y - boundingBox.Bottom) * Canvas.ZoomFactor <= MaxPointerDistance ? MovementHandle.BottomLeft :
                                 CurrentShape.GetBoundingBox().Contains(canvasPointer1Position) ? MovementHandle.All : MovementHandle.None;

                    // set the anchor position to the opposite point of the handle
                    switch (Handle)
                    {
                        case MovementHandle.None:
                            // if no handle was touched, cancel
                            return;
                        case MovementHandle.TopLeft:
                            AnchorPosition = PointF.Create(boundingBox.Right, boundingBox.Bottom);
                            break;
                        case MovementHandle.TopRight:
                            AnchorPosition = PointF.Create(boundingBox.Left, boundingBox.Bottom);
                            break;
                        case MovementHandle.BottomLeft:
                            AnchorPosition = PointF.Create(boundingBox.Right, boundingBox.Top);
                            break;
                        case MovementHandle.BottomRight:
                        case MovementHandle.All:
                            AnchorPosition = boundingBox.Location;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(Handle));
                    }

                    // transform point with inverse matrix because we want the real canvas position
                    var matrix1 = CurrentShape.Transforms.GetMatrix();
                    matrix1.Invert();
                    matrix1.TransformPoints(new[] { AnchorPosition });

                    UndoRedoService.ExecuteCommand(new UndoableActionCommand("Edit ellipse", o => Canvas.FireInvalidateCanvas(), o => Canvas.FireInvalidateCanvas()));
                }

                ApplyGesture(drag, canvasEnd);

                Canvas.FireInvalidateCanvas();
            }
        }

        public override async Task OnDraw(IRenderer renderer, ISvgDrawingCanvas ws)
        {
            await base.OnDraw(renderer, ws);

            if (CurrentShape == null)
                return;

            DrawCurrentShape(renderer, ws);
        }

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
            if (IsActive && args.RemovedChild == CurrentShape)
            {
                CurrentShape = null;
                Canvas.FireInvalidateCanvas();
            }
        }

        private void InitializeDefinitions(SvgDocument document)
        {
            var definitions = document.Children.OfType<SvgDefinitionList>().FirstOrDefault();
            if (definitions == null)
            {
                definitions = new SvgDefinitionList();
                document.Children.Insert(0, definitions);
            }

            foreach (var marker in Markers)
            {
                if (document.GetElementById(marker.ID) != null) continue;

                definitions.Children.Add(marker);
            }
        }

        protected void AddTranslate(SvgVisualElement element, float deltaX, float deltaY)
        {
            // the movetool stores the last translation explicitly for each element
            // that way, if another tool manipulates the translation (e.g. the snapping tool)
            // the movetool is not interfered by that
            var b = element.GetBoundingBox();
            var translate = _translate ?? PointF.Create(b.X, b.Y);

            translate.X += deltaX;
            translate.Y += deltaY;

            _translate = translate;

            var dX = translate.X - b.X;
            var dY = translate.Y - b.Y;

            var m = element.CreateTranslation(dX, dY);
            var formerM = element.Transforms.GetMatrix().Clone();
            UndoRedoService.ExecuteCommand(new UndoableActionCommand("Move object", o =>
            {
                element.SetTransformationMatrix(m);
            }, o =>
            {
                element.SetTransformationMatrix(formerM);
            }), hasOwnUndoRedoScope: false);
        }

        private void SelectShape(T shape)
        {
            CurrentShape = shape;
            Canvas.SelectedElements.Clear();
            Canvas.SelectedElements.Add(shape);
        }

        protected enum MovementHandle { None, TopLeft, TopRight, BottomRight, BottomLeft, All }
    }
}
