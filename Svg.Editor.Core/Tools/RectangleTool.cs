using System;
using System.Collections.Generic;
using Svg.Editor.Gestures;
using Svg.Editor.Interfaces;
using Svg.Editor.UndoRedo;
using Svg.Interfaces;

namespace Svg.Editor.Tools
{
    public class RectangleTool : ShapeToolBase<SvgRectangle>
    {
        public override int InputOrder => 300;

        public RectangleTool(IDictionary<string, object> properties, IUndoRedoService undoRedoService) : base("Rectangle", properties, undoRedoService)
        {
            IconName = "ic_crop_square.svg";
        }

        protected override void ApplyGesture(DragGesture drag, PointF canvasEnd)
        {
            // capture _currentEllipse for use in lambda
            var capturedCurrentRect = CurrentShape;

            // capture variables for use in lambda
            var formerX = CurrentShape.X;
            var formerY = CurrentShape.Y;
            var formerWidth = CurrentShape.Width;
            var formerHeight = CurrentShape.Height;

            // transform point with inverse matrix because we want the real canvas position
            var matrix = CurrentShape.Transforms.GetMatrix();
            matrix.Invert();
            matrix.TransformPoints(new[] { canvasEnd });

            // the rectangle that is drawn over the two points, which will contain the ellipse
            var drawnRectangle = RectangleF.FromPoints(new[] { canvasEnd, AnchorPosition });

            // resize/move ellipse depending on where the pointer was put down
            switch (Handle)
            {
                case MovementHandle.TopRight:
                case MovementHandle.BottomLeft:
                case MovementHandle.BottomRight:
                case MovementHandle.TopLeft:

                    UndoRedoService.ExecuteCommand(new UndoableActionCommand("Resize rectangle topleft", o =>
                    {
                        capturedCurrentRect.X = new SvgUnit(SvgUnitType.Pixel, drawnRectangle.X);
                        capturedCurrentRect.Y = new SvgUnit(SvgUnitType.Pixel, drawnRectangle.Y);
                        capturedCurrentRect.Width = new SvgUnit(SvgUnitType.Pixel, drawnRectangle.Width);
                        capturedCurrentRect.Height = new SvgUnit(SvgUnitType.Pixel, drawnRectangle.Height);
                    }, o =>
                    {
                        capturedCurrentRect.X = formerX;
                        capturedCurrentRect.Y = formerY;
                        capturedCurrentRect.Width = formerWidth;
                        capturedCurrentRect.Height = formerHeight;
                    }), hasOwnUndoRedoScope: false);

                    break;

                case MovementHandle.All:

                    var absoluteDeltaX = drag.Delta.Width / Canvas.ZoomFactor;
                    var absoluteDeltaY = drag.Delta.Height / Canvas.ZoomFactor;

                    // add translation to current line
                    var previousDelta = Offset ?? PointF.Create(0, 0);
                    var relativeDeltaX = absoluteDeltaX - previousDelta.X;
                    var relativeDeltaY = absoluteDeltaY - previousDelta.Y;

                    previousDelta.X = absoluteDeltaX;
                    previousDelta.Y = absoluteDeltaY;
                    Offset = previousDelta;

                    AddTranslate(CurrentShape, relativeDeltaX, relativeDeltaY);

                    break;

                case MovementHandle.None:
                    throw new ArgumentOutOfRangeException(nameof(Handle), "Cannot be none at this point.");
                default:
                    throw new ArgumentOutOfRangeException(nameof(Handle));
            }
        }

        protected override void DrawCurrentShape(IRenderer renderer, ISvgDrawingCanvas ws)
        {
            renderer.Graphics.Save();

            var radius = (float)MaxPointerDistance / 4 / ws.ZoomFactor;
            var halfRadius = radius / 2;
            
            var rect = CurrentShape.GetBoundingBox();
            var points = rect.GetPoints();
            
            renderer.FillCircle(points[0].X - halfRadius, points[0].Y - halfRadius, radius, BlueBrush);
            renderer.FillCircle(points[1].X - halfRadius, points[1].Y - halfRadius, radius, BlueBrush);
            renderer.FillCircle(points[2].X - halfRadius, points[2].Y - halfRadius, radius, BlueBrush);
            renderer.FillCircle(points[3].X - halfRadius, points[3].Y - halfRadius, radius, BlueBrush);

            renderer.Graphics.Restore();
        }

        public override string GetDescription()
        {
            return LocalizationService.GetString("Svg.Editor.RectangleTool.Description");
        }

        protected override SvgRectangle CreateShape(PointF relativeStart)
        {
            var ellipse = new SvgRectangle
            {
                Stroke = new SvgColourServer(Color.Create(0, 0, 0)),
                Fill = SvgPaintServer.None,
                StrokeWidth = new SvgUnit(SvgUnitType.Pixel, 5),
                X = new SvgUnit(SvgUnitType.Pixel, relativeStart.X),
                Y = new SvgUnit(SvgUnitType.Pixel, relativeStart.Y),
                CustomAttributes = { { ConstraintsCustomAttributeKey, NoFillConstraint } }
            };

            return ellipse;
        }
    }
}
