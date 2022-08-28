using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Svg.Editor.Gestures;
using Svg.Editor.Interfaces;
using Svg.Pathing;
using PointF = Svg.Interfaces.PointF;

namespace Svg.Editor.Tools
{
    public class PolygonTool : UndoableToolBase, ISupportMoving
    {
        public PolygonTool(IDictionary<string, object> properties, IUndoRedoService undoRedoService) : base("Polygon",
            properties, undoRedoService)
        {
            IconName = "ic_polygon.svg";
            ToolUsage = ToolUsage.Explicit;
            ToolType = ToolType.Create;
        }

        private const string PolygonTempAttributeKey = "data-polytemp";

        private bool _isActive;

        public override bool IsActive
        {
            get => _isActive;
            set
            {
                _isActive = value;
                if (!value)
                    Reset();
            }
        }

        public override async Task Initialize(ISvgDrawingCanvas ws)
        {
            await base.Initialize(ws);

            IsActive = false;
        }

        public const string DefaultStrokeWidthKey = "defaultstrokewidth";

        public int DefaultStrokeWidth
        {
            get
            {
                object defaultStrokeWidth;
                return Properties.TryGetValue(DefaultStrokeWidthKey, out defaultStrokeWidth)
                    ? Convert.ToInt32(defaultStrokeWidth)
                    : 2;
            }
            set => Properties[DefaultStrokeWidthKey] = value;
        }
        public IList<PointF> Points { get; } = new List<PointF>();
        protected override async Task OnTap(TapGesture tap)
        {
            await base.OnTap(tap);

            var point = Canvas.ScreenToCanvas(tap.Position);

            if (!IsActive) return;

            Canvas.SelectedElements.Clear();

            if (Points.Count > 0)
            {
                //Closing the polygon
                if (IsClickingOnFirstPoint(point))
                {
                    var ls = new SvgLineSegment(Points.Last(), Points.First());
                    
                    DrawPolygon();
                    Reset();
                    Canvas.ActiveTool = Canvas.Tools.First(tool => tool.Name == "Select");
                }
                else
                {
                    DrawVector(Points.Last(), point);
                }
            }
            else
            {
                DrawPoint(point);
                Points.Add(point);
            }

            Canvas.FireInvalidateCanvas();
        }

        private bool IsClickingOnFirstPoint(PointF tapPoint)
        {
            var point = Points.First();

            return point.X >= tapPoint.X - 10 / Canvas.ZoomFactor &&
                   point.X <= tapPoint.X + 10 / Canvas.ZoomFactor &&
                   point.Y >= tapPoint.Y - 10 / Canvas.ZoomFactor &&
                   point.Y <= tapPoint.Y + 10 / Canvas.ZoomFactor;
        }

        private void DrawPoint(PointF point)
        {
            var circle = new SvgCircle()
            {
                CenterX = point.X,
                CenterY = point.Y,
                Radius = 5,
                FillOpacity = 0.75f,
                StrokeOpacity = 0.75f
            };
            circle.CustomAttributes[PolygonTempAttributeKey] = "";

            Canvas.Document.Children.Add(circle);
        }

        private void DrawVector(PointF start, PointF end)
        {
            Points.Add(end);

            var line = new SvgLine()
            {
                StartX = start.X,
                EndX = end.X,
                StartY = start.Y,
                EndY = end.Y,
                StrokeOpacity = 0.5f
            };
            line.CustomAttributes[PolygonTempAttributeKey] = "";

            Canvas.Document.Children.Add(line);
        }

        private void DrawPolygon()
        {
            var points = Points.SelectMany(p => new SvgUnit[] { p.X, p.Y });
            var collectionPoints = new SvgPointCollection();
            collectionPoints.AddRange(points);

            var polygon = new SvgPolygon()
            {
                Points = collectionPoints,
                StrokeWidth = new SvgUnit(SvgUnitType.Pixel, DefaultStrokeWidth),
                FillOpacity = 0
            };

            Canvas.Document.Children.Add(polygon);
        }

        public override void Reset()
        {
            var toDelete = Canvas.Document.Children.Where(c => c.CustomAttributes.ContainsKey(PolygonTempAttributeKey))
                .ToList();

            toDelete.ForEach(item => Canvas.Document.Children.Remove(item));

            Points.Clear();

        }
    }
}