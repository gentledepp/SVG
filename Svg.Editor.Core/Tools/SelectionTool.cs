using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Svg.Editor.Extensions;
using Svg.Editor.Gestures;
using Svg.Editor.Interfaces;
using Svg.Editor.UndoRedo;
using Svg.Interfaces;


namespace Svg.Editor.Tools
{
    public class SelectionTool : UndoableToolBase
    {
        #region Private fields

        private RectangleF _selectionRectangle;
        private Brush _brush;
        private Pen _pen;

        #endregion

        #region Private properties

        private string DeleteIconName { get; } = "ic_delete.svg";
        private string SelectIconName { get; } = "ic_select.svg";
        private Brush BlueBrush => _brush ?? (_brush = SvgEngine.Factory.CreateSolidBrush(SvgEngine.Factory.CreateColorFromArgb(255, 80, 210, 210)));
        private Pen BluePen => _pen ?? (_pen = SvgEngine.Factory.CreatePen(BlueBrush, 5));

        #endregion

        #region Public properties

        public override int GestureOrder => 1000;

        public override bool IsActive
        {
            get { return base.IsActive; }
            set
            {
                base.IsActive = value;
                Reset();
            }
        }

        #endregion

        public SelectionTool(IUndoRedoService undoRedoService) : base("Select", undoRedoService)
        {
            IconName = SelectIconName;
            ToolUsage = ToolUsage.Explicit;
            ToolType = ToolType.Select;
            HandleDragExit = true;
        }

        #region Overrides

        public override async Task Initialize(ISvgDrawingCanvas ws)
        {
            await base.Initialize(ws);

            Commands = new List<IToolCommand>
            {
                new ToolCommand(this, "Delete", o =>
                {
                    UndoRedoService.ExecuteCommand(new UndoableActionCommand("Remove operation", x => {}));
                        foreach (var element in ws.SelectedElements)
                        {
                            var parent = element.Parent;
                            UndoRedoService.ExecuteCommand(new UndoableActionCommand("Remove element", x =>
                            {
                                parent.Children.Remove(element);
                                ws.FireInvalidateCanvas();
                            }, x =>
                            {
                                parent.Children.Add(element);
                                ws.FireInvalidateCanvas();
                            }), hasOwnUndoRedoScope: false);
                        }
                        ws.SelectedElements.Clear();
                        ws.FireInvalidateCanvas();
                },
                o => ws.SelectedElements.Any(), iconName:DeleteIconName,
                sortFunc: t => 550)
            };
        }

        protected override async Task OnTap(TapGesture tap)
        {
            await base.OnTap(tap);

            if (!IsActive) return;
            
            System.Diagnostics.Debug.WriteLine(tap.Position);

            // select elements under pointer
            var pointerRect = Canvas.GetPointerRectangle(tap.Position);
            SelectElementsUnder(pointerRect, Canvas, SelectionType.Intersect, 1);

            Reset();
            Canvas.FireInvalidateCanvas();
        }

        protected override async Task OnDrag(DragGesture drag)
        {
            await base.OnDrag(drag);

            if (!IsActive) return;

            System.Diagnostics.Debug.WriteLine(drag.Start);

            if (drag.State == DragState.Exit)
            {
                // select elements under rectangle
                if (_selectionRectangle != null)
                    SelectElementsUnder(_selectionRectangle, Canvas, SelectionType.Contain);

                Reset();
                Canvas.FireInvalidateCanvas();

                return;
            }

            var location = drag.Start;
            var size = drag.Delta;

            if (size.Width < 0 && size.Height < 0)
            {
                location = location + size;
                size = SizeF.Create(Math.Abs(size.Width), Math.Abs(size.Height));
            }
            else if (size.Height < 0)
            {
                location = PointF.Create(location.X, location.Y + size.Height);
                size = SizeF.Create(size.Width, Math.Abs(size.Height));
            }
            else if (size.Width < 0)
            {
                location = PointF.Create(location.X + size.Width, location.Y);
                size = SizeF.Create(Math.Abs(size.Width), size.Height);
            }

            _selectionRectangle = RectangleF.Create(location, size);

            Canvas.FireInvalidateCanvas();
        }

        public override Task OnDraw(IRenderer renderer, ISvgDrawingCanvas ws)
        {
            // we draw the selection rectangle
            if (_selectionRectangle != null)
            {
                renderer.Graphics.Save();

                // as we are in "OnDraw", an panning as well as zoomin tools have already translated 
                // the canvas in order to properly render the svg elements zoomed and panned
                // we need to undo that translation, as our selection rectangle must be drawn
                // in absolute screen coordinates (below the finger of the user - not translated and scaled)
                var m = renderer.Graphics.Transform.Clone();
                m.Invert();
                renderer.Graphics.Concat(m);

                BluePen.StrokeWidth = 5;
                renderer.DrawRectangle(_selectionRectangle, BluePen);

                renderer.Graphics.Restore();
            }

            // we draw the selection boundingboxes of all selected elements
            foreach (var element in ws.SelectedElements)
            {
                renderer.Graphics.Save();
                
                // we draw a selection adorner around all elements
                // as the canvas is already translated and scaled, we just need the plai boundingbox (as poopsed to transformed one using element.GetBoundingBox(ws.GetCanvasTransformationMatrix()))
                BluePen.StrokeWidth = 5 / ws.ZoomFactor;
                renderer.DrawRectangle(element.GetBoundingBox(), BluePen);

                renderer.Graphics.Restore();
            }

            return Task.FromResult(true);
        }

        public override void OnDocumentChanged(SvgDocument oldDocument, SvgDocument newDocument)
        {
            _selectionRectangle = null;
        }

        public override void Reset()
        {
            _selectionRectangle = null;
        }

        #endregion

        #region Private helpers

        private void SelectElementsUnder(RectangleF selectionRectangle, ISvgDrawingCanvas ws, SelectionType selectionType, int maxItems = int.MaxValue)
        {
            ws.SelectedElements.Clear();

            // the canvas has not been scaled and translated yet
            // we need to compare our rectangle to the translated boundingboxes of the svg elements
            var selected = ws.GetElementsUnder<SvgVisualElement>(selectionRectangle, selectionType, maxItems);

            var canvasPoint = ws.ScreenToCanvas(PointF.Create(selectionRectangle.X+selectionRectangle.Width/2, selectionRectangle.Y+selectionRectangle.Height/2));

            if (!Canvas.Document.Children.Contains(_dumm))
                Canvas.Document.Children.Add(_dumm);

            _dumm.CenterX = canvasPoint.X;
            _dumm.CenterY = canvasPoint.Y;

            foreach (var element in selected)
            {
                if (element is SvgPolygon poly && selectionRectangle.Height <= 20 && selectionRectangle.Width <= 20)
                {
                    if (element.Stroke != null && element.Stroke.ToString() != "" && element.FillOpacity == 0)
                        if (!ValidatePolygonSelection((SvgPolygon)element, canvasPoint, 40 / ws.ZoomFactor))
                            break;
                }

                if (element.CustomAttributes.ContainsKey(BackgroundCustomAttributeKey)) continue;
                if (element.CustomAttributes.ContainsKey(HittestInvisibleAttributeKey)) continue;
                ws.SelectedElements.Add(element);
            }
        }

        protected bool ValidatePolygonSelection(SvgPolygon polygon, PointF tap, double selectionFuzzines)
        {
            if (polygon == null)
                return false;

            
            var m = Matrix.Create();
            var allTransForms = polygon.Parents.Reverse().Concat(new[] { polygon }).SelectMany(elt => elt.Transforms)
                .ToList();

            foreach (var t in allTransForms)
                t.ApplyTo(m);

            var units = polygon.Points.ToList();

            PointF firstPoint = PointF.Empty;
            PointF lastPoint = PointF.Empty;
            for (var i = 0; i < units.Count-3; i+=2)
            {
                PointF[] points = new[]
                {
                    PointF.Create(units[i].Value, units[i+1].Value),
                    PointF.Create(units[i + 2].Value, units[i + 3].Value),
                };

                m.TransformPoints(points);

                if (TapedOnBorder(points[0], points[1], tap, selectionFuzzines))
                    return true;

                if (i == 0)
                {
                    firstPoint = points[0];
                }
                lastPoint = points[1];
            }

            return TapedOnBorder(lastPoint, firstPoint, tap, selectionFuzzines);
        }

        /// <summary>
        /// Google paste https://stackoverflow.com/a/13741803/333571
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="point"></param>
        /// <param name="selectionFuzzines"></param>
        /// <returns></returns>
        private bool TapedOnBorder(PointF start, PointF end, PointF point, double selectionFuzzines)
        {
            PointF leftPoint;
            PointF rightPoint;

            // Normalize start/end to left right to make the offset calc simpler.
            if (start.X <= end.X)
            {
                leftPoint = start;
                rightPoint = end;
            }
            else
            {
                leftPoint = end;
                rightPoint = start;
            }

            // If point is out of bounds, no need to do further checks.                  
            if (point.X + selectionFuzzines < leftPoint.X || rightPoint.X < point.X - selectionFuzzines)
                return false;
            if (point.Y + selectionFuzzines < Math.Min(leftPoint.Y, rightPoint.Y) || Math.Max(leftPoint.Y, rightPoint.Y) < point.Y - selectionFuzzines)
                return false;

            // https://de.wikipedia.org/wiki/Lineare_Funktion
            double deltaX = rightPoint.X - leftPoint.X;
            double deltaY = rightPoint.Y - leftPoint.Y;

            // If the line is straight, the earlier boundary check is enough to determine that the point is on the line.
            // Also prevents division by zero exceptions.
            if (deltaX == 0 || deltaY == 0)
                return true;

            double slope = deltaY / deltaX;
            double offset = leftPoint.Y - leftPoint.X * slope;
            double calculatedY = point.X * slope + offset;

            //adjustment of offset 
            double c = point.Y - offset - slope * point.X;

            double actualX = (point.Y - offset) / slope;
            double outOfBounds = point.X - actualX >= 0 ? point.X - actualX : actualX - point.X;

            if (outOfBounds > selectionFuzzines && c > selectionFuzzines) {
                    return false;
            }
            
            calculatedY += c;

            //Check calculated Y matches the points Y coord with some easing.
            bool lineContains = point.Y - selectionFuzzines <= calculatedY && calculatedY <= point.Y + selectionFuzzines;

            return lineContains;
        }

        private SvgCircle _dumm = new SvgCircle
        {
            Radius = 5,
            Fill = new SvgColourServer(Color.Create("#FF0000"))
        };
    }

    #endregion
    
}
