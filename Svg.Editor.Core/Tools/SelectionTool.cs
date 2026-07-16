using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Svg.Editor.Gestures;
using Svg.Editor.Interfaces;
using Svg.Editor.UndoRedo;
using Svg.Interfaces;


namespace Svg.Editor.Tools
{
    public class SelectionTool : UndoableToolBase
    {
        #region Private fields

        private RectangleF _selectionPoint;
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

        public bool ShowHitTestMarker { get; set; } = false;

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
                sortFunc: t => 550, description: LocalizationService.GetString("Svg.Editor.SelectionTool.Delete.Description"))
            };
        }

        protected override async Task OnTap(TapGesture tap)
        {
            await base.OnTap(tap);

            if (!IsActive) return;
            
            System.Diagnostics.Debug.WriteLine(tap.Position);

            // select elements under pointer
            var pointerRect = Canvas.GetPointerRectangle(tap.Position);
            _selectionPoint = pointerRect;
            _selectionRectangle = null;

            SelectElementsUnderPoint(pointerRect, Canvas, SelectionType.Intersect);


            Canvas.FireInvalidateCanvas();
        }

        protected override async Task OnDrag(DragGesture drag)
        {
            await base.OnDrag(drag);

            if (!IsActive) return;

            _selectionPoint = null;

            System.Diagnostics.Debug.WriteLine(drag.Start);

            if (drag.State == DragState.Exit)
            {
                // select elements under rectangle
                if (_selectionRectangle != null)
                    SelectElementsUnderArea(_selectionRectangle, Canvas, SelectionType.Contain);

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
                // We invert only the editor's zoom/translate matrix (not the full graphics transform
                // which may include a platform DPI pre-scale) so we land in logical pixel space.
                // 
                // Watch out for DPI
                // - renderer.Graphics.Transform returns the full canvas matrix: DPI_Scale × Zoom × Translate
                // - Inverting that lands you in physical pixel space (identity matrix)
                // - But mouse coordinates from Avalonia are in logical pixels
                // 
                // The fix uses ws.GetCanvasTransformationMatrix() which returns only the editor's Zoom × Translate matrix (no DPI scale).
                // Inverting just that part:
                // 
                // DPI_Scale × ZoomTranslate × ZoomTranslate⁻¹ = DPI_Scale
                // 
                // This leaves the DPI scale intact, so the selection rectangle is drawn in logical pixel space — matching the mouse
                // coordinates. When IgnorePixelScaling=true (Scale=1), the behavior is unchanged since both matrices are identical.
                // 
                var m = ws.GetCanvasTransformationMatrix();
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

            // if enabled, draw a click/tap indicator
            if (ShowHitTestMarker && _selectionPoint != null)
            {
                renderer.Graphics.Save();
                // Watch out for DPI
                // - renderer.Graphics.Transform returns the full canvas matrix: DPI_Scale × Zoom × Translate
                // - Inverting that lands you in physical pixel space (identity matrix)
                // - But mouse coordinates from Avalonia are in logical pixels
                // 
                // The fix uses ws.GetCanvasTransformationMatrix() which returns only the editor's Zoom × Translate matrix (no DPI scale).
                // Inverting just that part:
                // 
                // DPI_Scale × ZoomTranslate × ZoomTranslate⁻¹ = DPI_Scale
                // 
                // This leaves the DPI scale intact, so the selection rectangle is drawn in logical pixel space — matching the mouse
                // coordinates. When IgnorePixelScaling=true (Scale=1), the behavior is unchanged since both matrices are identical.
                var m = ws.GetCanvasTransformationMatrix();
                m.Invert();
                renderer.Graphics.Concat(m);
                
                var center = _selectionPoint.GetCenterPoint();
                renderer.DrawCircle(center.X, center.Y, _selectionPoint.Width/2, BluePen);

                renderer.Graphics.Restore();
            }

            return Task.FromResult(true);
        }

        public override void OnDocumentChanged(SvgDocument oldDocument, SvgDocument newDocument)
        {
            _selectionRectangle = null;
            _selectionPoint = null;
        }

        public override void Reset()
        {
            _selectionRectangle = null;
            _selectionPoint = null;
        }

        public override string GetDescription()
        {
            return LocalizationService.GetString("Svg.Editor.SelectionTool.Description");
        }

        #endregion

        #region Private helpers

        private void SelectElementsUnderArea(RectangleF selectionRectangle, ISvgDrawingCanvas ws, SelectionType selectionType, int maxItems = int.MaxValue)
        {
            // When an area is drawn, we do not want an element that is NOT contained to be selected, just because some child element IS contained in the selection rectangle
            // therefore we MUST specify "recursionLevel = 1"!
            SelectElementsUnder(selectionRectangle, ws, selectionType, maxItems, 1);
        }

        private void SelectElementsUnderPoint(RectangleF selectionRectangle, ISvgDrawingCanvas ws, SelectionType selectionType, int maxItems = 1)
        {
            // when selecting by tapping a point, we do want an element to be selected, if the point intersects with ANY child element of it
            // therefore we need "recursionLevel=max"!
            // a tap is a single point, so it should only ever select the top-most (highest z-order) element under
            // it, not every overlapping element - that's what dragging out a selection area is for (see SelectElementsUnderArea).
            // HitTest already returns matches in top-to-bottom z-order, so taking just the first one is correct.
            SelectElementsUnder(selectionRectangle, ws, selectionType, maxItems, int.MaxValue);
        }

        private void SelectElementsUnder(RectangleF selectionRectangle, ISvgDrawingCanvas ws, SelectionType selectionType, int maxItems = int.MaxValue, int recursionLevel = int.MaxValue)
        {

            ws.SelectedElements.Clear();

            // the canvas has not been scaled and translated yet
            // we need to compare our rectangle to the translated boundingboxes of the svg elements
            var selected = ws.GetElementsUnder<SvgVisualElement>(selectionRectangle, selectionType, HitTestResultMode.ReturnRootElementOnly, maxItems, recursionLevel: recursionLevel);

            var x = this.GetHashCode();

            foreach (var element in selected)
            {
                ws.SelectedElements.Add(element);
            }
        }
        
    }

    #endregion
    
}
