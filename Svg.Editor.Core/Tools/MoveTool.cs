using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Svg.Editor.Extensions;
using Svg.Editor.Gestures;
using Svg.Editor.Interfaces;
using Svg.Editor.UndoRedo;
using Svg.Interfaces;

namespace Svg.Editor.Tools
{
    public interface ISupportMoving { }

    public class MoveTool : UndoableToolBase
    {
        #region Private fields

        private readonly Dictionary<object, PointF> _offsets = new Dictionary<object, PointF>();
        private readonly Dictionary<object, PointF> _translates = new Dictionary<object, PointF>();
        private ITool _activatedFrom;

        #endregion

        public MoveTool(IUndoRedoService undoRedoService) : base("Move", undoRedoService)
        {
            ToolType = ToolType.Modify;
            HandleDragEnter = true;
            HandleDragExit = true;
        }

        #region Overrides

        public override int InputOrder => 200; // must be before pantool as it decides whether or not it is active based on selection

        protected override async Task OnDrag(DragGesture drag)
        {
            await base.OnDrag(drag);

            if (drag.State == DragState.Enter)
            {
                if (Canvas.SelectedElements.Any() &&
                    Canvas.GetElementsUnderPointer<SvgVisualElement>(drag.Start, SelectionType.IntersectBoundingBoxes)
                        .Any(eup => Canvas.SelectedElements.Contains(eup)))
                {
                    // move tool is only active, if SelectionTool is the "ActiveTool"
                    // or the active tool is PinTool
                    // otherwise we'd move and pan at the same time, yielding confusing results... :)
                    if ((Canvas.ActiveTool.ToolType != ToolType.Select) &&
                        !(Canvas.ActiveTool is ISupportMoving)) return;
                    

                    // save the active tool for restoring later
                    _activatedFrom = Canvas.ActiveTool;
                    Canvas.ActiveTool = this;
                }
                else
                {
                    IsActive = false;
                }

                return;
            }

            if (!IsActive) return;

            if (drag.State == DragState.Exit)
            {
                _offsets.Clear();
                _translates.Clear();

                if (_activatedFrom != null)
                {
                    Canvas.ActiveTool = _activatedFrom;
                }

                IsActive = false;

                return;
            }

            // if there is no selection, we just skip
            if (Canvas.SelectedElements.Any())
            {

                // check if offsets were cleared, that means we started a new move operation
                if (!_offsets.Any())
                {
                    // when we start a move operation, we execute an empty undoable command first,
                    // so the other ones will be added to this command as on undo step
                    UndoRedoService.ExecuteCommand(new UndoableActionCommand("Move operation", o => { Canvas.FireInvalidateCanvas(); }, o => { Canvas.FireInvalidateCanvas(); }));
                }

                var absoluteDeltaX = drag.Delta.Width / Canvas.ZoomFactor;
                var absoluteDeltaY = drag.Delta.Height / Canvas.ZoomFactor;

                // add translation to every element
                foreach (var element in Canvas.SelectedElements)
                {
                    PointF previousDelta;
                    if (!_offsets.TryGetValue(element, out previousDelta))
                    {
                        previousDelta = PointF.Create(0f, 0f);
                    }

                    var relativeDeltaX = absoluteDeltaX - previousDelta.X;
                    var relativeDeltaY = absoluteDeltaY - previousDelta.Y;

                    previousDelta.X = absoluteDeltaX;
                    previousDelta.Y = absoluteDeltaY;
                    _offsets[element] = previousDelta;

                    AddTranslate(element, relativeDeltaX, relativeDeltaY);
                }

                Canvas.FireInvalidateCanvas();
            }
        }

        #endregion

        #region Private helpers

        private void AddTranslate(SvgVisualElement element, float deltaX, float deltaY)
        {
            // the movetool stores the last translation explicitly for each element
            // that way, if another tool manipulates the translation (e.g. the snapping tool)
            // the movetool is not interfered by that
            var b = element.GetBoundingBox();
            PointF translate;
            if (!_translates.TryGetValue(element, out translate))
            {
                translate = PointF.Create(b.X, b.Y);
            }

            translate.X += deltaX;
            translate.Y += deltaY;

            _translates[element] = translate;

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

        #endregion
    }
}
