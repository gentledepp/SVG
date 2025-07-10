using Svg.Interfaces;

namespace Svg.Pathing
{
    public sealed class SvgQuadraticCurveSegment : SvgPathSegment
    {
        private PointF _controlPoint;

        public PointF ControlPoint
        {
            get { return _controlPoint; }
            set { _controlPoint = value; }
        }

        private PointF FirstControlPoint
        {
            get
            {
                float x1 = Start.X + (ControlPoint.X - Start.X) * 2 / 3;
                float y1 = Start.Y + (ControlPoint.Y - Start.Y) * 2 / 3;

                return PointF.Create(x1, y1);
            }
        }

        private PointF SecondControlPoint
        {
            get
            {
                float x2 = ControlPoint.X + (End.X - ControlPoint.X) / 3;
                float y2 = ControlPoint.Y + (End.Y - ControlPoint.Y) / 3;

                return PointF.Create(x2, y2);
            }
        }

        public SvgQuadraticCurveSegment(PointF start, PointF controlPoint, PointF end)
        {
            Start = start;
            _controlPoint = controlPoint;
            End = end;
        }

        public override void AddToPath(GraphicsPath graphicsPath)
        {
            graphicsPath.AddQuad(Start, ControlPoint, End);
        }
        
        public override string ToString()
		{
			return "Q" + ControlPoint.ToSvgString() + " " + End.ToSvgString();
		}

    }
}
