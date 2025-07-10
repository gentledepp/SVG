using System;
using System.Collections.Generic;
using Svg.Editor.Gestures;
using Svg.Editor.Interfaces;
using Svg.Editor.UndoRedo;
using Svg.Interfaces;

namespace Svg.Editor.Tools
{
	public class EllipseTool : ShapeToolBase<SvgEllipse>
    {
        public override int InputOrder => 300;

        public EllipseTool(IDictionary<string, object> properties, IUndoRedoService undoRedoService) : base("Ellipse", properties, undoRedoService)
        {
            IconName = "ic_panorama_fish_eye.svg";
        }

	    protected override void ApplyGesture(DragGesture drag, PointF canvasEnd)
	    {
			// capture _currentEllipse for use in lambda
		    var capturedCurrentEllipse = CurrentShape;

		    // capture variables for use in lambda
		    var formerCenterX = CurrentShape.CenterX;
		    var formerCenterY = CurrentShape.CenterY;
		    var formerRadiusX = CurrentShape.RadiusX;
		    var formerRadiusY = CurrentShape.RadiusY;

		    // transform point with inverse matrix because we want the real canvas position
		    var matrix = CurrentShape.Transforms.GetMatrix();
		    matrix.Invert();
		    matrix.TransformPoints(new[] {canvasEnd});

		    // the rectangle that is drawn over the two points, which will contain the ellipse
		    var drawnRectangle = RectangleF.FromPoints(new[] {canvasEnd, AnchorPosition});

		    // resize/move ellipse depending on where the pointer was put down
		    switch (Handle)
		    {
			    case MovementHandle.TopRight:
			    case MovementHandle.BottomLeft:
			    case MovementHandle.BottomRight:
			    case MovementHandle.TopLeft:

				    UndoRedoService.ExecuteCommand(new UndoableActionCommand("Resize ellipse topleft", o =>
				    {
					    var center = PointF.Create(drawnRectangle.X + drawnRectangle.Width / 2,
						    drawnRectangle.Y + drawnRectangle.Height / 2);
					    var radius = SizeF.Create(drawnRectangle.Width / 2, drawnRectangle.Height / 2);
					    capturedCurrentEllipse.CenterX = new SvgUnit(SvgUnitType.Pixel, center.X);
					    capturedCurrentEllipse.CenterY = new SvgUnit(SvgUnitType.Pixel, center.Y);
					    capturedCurrentEllipse.RadiusX = new SvgUnit(SvgUnitType.Pixel, radius.Width);
					    capturedCurrentEllipse.RadiusY = new SvgUnit(SvgUnitType.Pixel, radius.Height);
				    }, o =>
				    {
					    capturedCurrentEllipse.CenterX = formerCenterX;
					    capturedCurrentEllipse.CenterY = formerCenterY;
					    capturedCurrentEllipse.RadiusX = formerRadiusX;
					    capturedCurrentEllipse.RadiusY = formerRadiusY;
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

		    var radius = (float) MaxPointerDistance / 4 / ws.ZoomFactor;
		    var halfRadius = radius / 2;
		    var points = CurrentShape.GetTransformedPoints();
		    renderer.FillCircle(points[0].X - halfRadius, points[0].Y - halfRadius, radius, BlueBrush);
		    renderer.FillCircle(points[1].X - halfRadius, points[1].Y - halfRadius, radius, BlueBrush);
		    renderer.FillCircle(points[2].X - halfRadius, points[2].Y - halfRadius, radius, BlueBrush);
		    renderer.FillCircle(points[3].X - halfRadius, points[3].Y - halfRadius, radius, BlueBrush);

		    renderer.Graphics.Restore();
	    }

	    protected override SvgEllipse CreateShape(PointF relativeStart)
        {
            var ellipse = new SvgEllipse
            {
                Stroke = new SvgColourServer(Color.Create(0, 0, 0)),
                Fill = SvgPaintServer.None,
                StrokeWidth = new SvgUnit(SvgUnitType.Pixel, 5),
                CenterX = new SvgUnit(SvgUnitType.Pixel, relativeStart.X),
                CenterY = new SvgUnit(SvgUnitType.Pixel, relativeStart.Y),
                CustomAttributes = { { ConstraintsCustomAttributeKey, NoFillConstraint } }
            };

            return ellipse;
        }
    }
}
