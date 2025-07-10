using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Svg.Editor.Events;
using Svg.Editor.Extensions;
using Svg.Editor.Interfaces;
using Svg.Editor.UndoRedo;
using Svg.Interfaces;

namespace Svg.Editor.Tools
{
    public class RotationTool : UndoableToolBase
    {

        #region Private fields

        private bool _wasImplicitlyActivated;
        private PointF _lastRotationCenter;
        private Brush _brush2;
        private Pen _pen2;
        private Brush RedBrush => _brush2 ?? (_brush2 = SvgEngine.Factory.CreateSolidBrush(SvgEngine.Factory.CreateColorFromArgb(255, 255, 150, 150)));
        private Pen RedPen => _pen2 ?? (_pen2 = SvgEngine.Factory.CreatePen(RedBrush, 3));
        private readonly Dictionary<SvgElement, float> _rotations = new Dictionary<SvgElement, float>();
        private ITool _activatedFrom;

        #endregion

        #region Public properties

        public bool IsDebugEnabled { get; set; }

        public const string FilterKey = @"filter";
        public const string RotationStepKey = @"rotationstep";

        public Func<SvgVisualElement, bool> Filter
        {
            get
            {
                object filter;
                if (!Properties.TryGetValue(FilterKey, out filter))
                {
                    return element => true;
                }
                return (Func<SvgVisualElement, bool>) filter ?? (element => true);
            }
            set { Properties[FilterKey] = value; }
        }

        public float RotationStep
        {
            get
            {
                object rotationStep;
                Properties.TryGetValue(RotationStepKey, out rotationStep);
                return Convert.ToSingle(rotationStep);
            }
            set { Properties[RotationStepKey] = value; }
        }

        #endregion

        public RotationTool(IDictionary<string, object> properties, IUndoRedoService undoRedoService) : base("Rotate", properties, undoRedoService)
        {
            ToolType = ToolType.Modify;
            IconName = "ic_rotate_right.svg";
        }

        #region Overrides

	    public override IDictionary<string, object> GetPropertiesForSerialization()
	    {
		    var properties = Properties.ToDictionary(d => d.Key, d => d.Value);
		    properties.Remove(FilterKey);
		    return properties;
	    }

	    public override async Task Initialize(ISvgDrawingCanvas ws)
        {
            await base.Initialize(ws);

            Commands = new[]
            {
                new RotateStepCommand(this, "Rotate right", "ic_rotate_right.svg", RotationStep),
                new RotateStepCommand(this, "Rotate left", "ic_rotate_left.svg", -RotationStep),
            };
        }

        public override Task OnUserInput(UserInputEvent @event, ISvgDrawingCanvas ws)
        {
            var re = @event as RotateEvent;

            // if a "RotateEvent" comes in
            if (re == null) return Task.FromResult(true);

            if (re.Status == RotateStatus.Start &&
                // and there is a single selected element
                Canvas.SelectedElements.Count == 1 &&
                // and the selectiontool is active
                Canvas.ActiveTool.ToolType == ToolType.Select &&
                // and the gesture is made with 3 fingers
                re.PointerCount == 3)
            {
                // implicitly activate
                _activatedFrom = Canvas.ActiveTool;
                Canvas.ActiveTool = this;
                _wasImplicitlyActivated = true;
                Canvas.ZoomEnabled = false;
                _rotations.Clear();

                UndoRedoService.ExecuteCommand(new UndoableActionCommand("Rotate operation", o => { }));
            }
            else if (re.Status == RotateStatus.Rotating &&
                     Canvas.SelectedElements.Count == 1 &&
                     re.PointerCount == 3)
            {
                RotateElementStepwise(Canvas.SelectedElements[0], re.AbsoluteRotationDegrees);
            }
            else if (re.Status == RotateStatus.End)
            {
                if (Canvas.ActiveTool == this && _wasImplicitlyActivated)
                {
                    Canvas.ActiveTool = _activatedFrom;
                }
                Canvas.ZoomEnabled = true;
                _lastRotationCenter = null;
                _rotations.Clear();
            }

            return Task.FromResult(true);
        }

        public override Task OnDraw(IRenderer renderer, ISvgDrawingCanvas ws)
        {
            if (IsDebugEnabled && _lastRotationCenter != null)
                renderer.DrawCircle(_lastRotationCenter.X, _lastRotationCenter.Y, 2, RedPen);

            return Task.FromResult(true);
        }

        public override void Dispose()
        {
            _brush2?.Dispose();
            _pen2?.Dispose();

            base.Dispose();
        }

        #endregion

        #region Private helpers

        private void RotateElementStepwise(SvgVisualElement element, float absoluteDegrees)
        {
            // if element must not be rotated
            if (!Filter.Invoke(element))
                return;

            // always rotate by absolute radius!
            float previousAngle;
            if (!_rotations.TryGetValue(element, out previousAngle))
            {
                previousAngle = 0f;
            }

            var absoluteAngle = absoluteDegrees;
            // calculate the next rotation within the stepsize
            var angle = CalculateNewRotation(absoluteAngle);
            var delta = angle - previousAngle;

            _rotations[element] = angle;

            RotateElementForDelta(element, delta);
        }

        private void RotateElementForDelta(SvgVisualElement element, float delta)
        {
            if (!CanRotate(element, delta)) return;

            var formerM = element.Transforms.GetMatrix().Clone();
            var m = element.CreateOriginRotation(delta % 360);

            UndoRedoService.ExecuteCommand(new UndoableActionCommand("Rotate element", o =>
            {
                element.SetTransformationMatrix(m);
                Canvas.FireInvalidateCanvas();
            }, o =>
            {
                element.SetTransformationMatrix(formerM);
                Canvas.FireInvalidateCanvas();
            }), hasOwnUndoRedoScope: false);
        }

        private bool CanRotate(SvgVisualElement element, float delta)
        {
            return Math.Abs(delta) >= 0.01f && Filter.Invoke(element);
        }

        private float CalculateNewRotation(float absoluteAngle)
        {
            // if we can rotate with any angle, just return the absolute one
            if (RotationStep <= 0)
                return absoluteAngle;

            // else make sure we only rotate with the specified step size (e.g. 45°)
            var rest = absoluteAngle % RotationStep;

            // if the remainder is less than half the step size, just remove it
            if (rest <= RotationStep / 2)
            {
                return absoluteAngle - rest;
            }
            // otherwise round up to the next allowed angle (add stepsize)
            return absoluteAngle - rest + RotationStep;
        }

        #endregion

        #region Inner types

        private class RotateStepCommand : ToolCommand
        {
            private float Step { get; }

            public RotateStepCommand(RotationTool tool, string name, string iconName, float step)
                : base(tool, name, o => { }, o => true, iconName: iconName)
            {
                Step = step;
            }

            public override void Execute(object parameter)
            {
                var tool = (RotationTool) Tool;

                if (tool.Canvas.SelectedElements.Count != 1) return;

                var selected = tool.Canvas.SelectedElements[0];

                tool.UndoRedoService.ExecuteCommand(new UndoableActionCommand(Name, _ => tool.RotateElementForDelta(selected, Step)));
            }

            public override bool CanExecute(object parameter)
            {
                var tool = (RotationTool) Tool;
                return tool.Canvas.SelectedElements.Count == 1 &&
                       tool.CanRotate(tool.Canvas.SelectedElements.First(), Step);
            }
        }

        #endregion
    }
}
