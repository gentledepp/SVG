using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Svg.Editor.Extensions;
using Svg.Editor.Gestures;
using Svg.Editor.Interfaces;
using Svg.Interfaces;
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

        private bool _isActive;
        private Brush _brush;
        private Pen _pen;
        private Brush _circleBrush;
        private Brush PolyLineBrush => _brush ?? (_brush = SvgEngine.Factory.CreateSolidBrush(SvgEngine.Factory.CreateColorFromArgb(125, 80, 210, 210)));
        private Pen PolyLinePen => _pen ?? (_pen = SvgEngine.Factory.CreatePen(PolyLineBrush, 5));
        private Brush PolyCircleBrush => _circleBrush ?? (_circleBrush = SvgEngine.Factory.CreateSolidBrush(SvgEngine.Factory.CreateColorFromArgb(255, 80, 210, 210)));

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
            
            //Closing the polygon
            if (Points.Count > 0 && IsClickingOnFirstPoint(point))
            {
                if (!IsValidPolgyon(Points))
                {
                    Reset();
                }
                else
                {
                    DrawPolygon();
                    Reset();
                    Canvas.ActiveTool = Canvas.Tools.First(tool => tool.Name == "Select");
                }
            }
            else
            {
                Points.Add(point);
            }

            Canvas.FireInvalidateCanvas();
        }

        public override Task OnDraw(IRenderer renderer, ISvgDrawingCanvas ws)
        {
            // we draw the selection rectangle
            if (Points.Count != 0)
            {
                renderer.Graphics.Save();
                PolyLinePen.StrokeWidth = 5 / Canvas.ZoomFactor;

                for (int i = 0; i < Points.Count-1; i++)
                {
                    var f = Points[i];
                    var t = Points[i + 1];
                    renderer.DrawLine(f.X, f.Y, t.X, t.Y, PolyLinePen);
                }

                var firstPoint = Points[0];
                var radius = 5f/Canvas.ZoomFactor;
                renderer.FillCircle(firstPoint.X-(radius/2), firstPoint.Y-(radius/2), radius, PolyCircleBrush);
            }

            return Task.CompletedTask;
        }

        private bool IsClickingOnFirstPoint(PointF tapPoint)
        {
            var point = Points.First();

            return point.X >= tapPoint.X - 10 / Canvas.ZoomFactor &&
                   point.X <= tapPoint.X + 10 / Canvas.ZoomFactor &&
                   point.Y >= tapPoint.Y - 10 / Canvas.ZoomFactor &&
                   point.Y <= tapPoint.Y + 10 / Canvas.ZoomFactor;
        }


        public override string GetDescription()
        {
            return LocalizationService.GetString("Svg.Editor.PolygonTool.Description");
        }

        private void DrawPolygon()
        {
            CreatePolygonOverride(Points);
        }

        protected virtual bool IsValidPolgyon(IList<PointF> points)
        {
            if (points.Count < 3)
                return false;

            return true;
        }

        protected virtual void CreatePolygonOverride(IEnumerable<PointF> points)
        {
            var collectionPoints = new SvgPointCollection();
            collectionPoints.AddRange(points.SelectMany(p => new SvgUnit[] { p.X, p.Y }));

            var polygon = new SvgPolygon()
            {
                Points = collectionPoints,
                StrokeWidth = new SvgUnit(SvgUnitType.Pixel, DefaultStrokeWidth),
                Fill = SvgColourServer.None,
                Stroke = new SvgColourServer(Color.Create(0,0,0))
            };
            // we do not want the polygon to be filled
            polygon.AddConstraints(NoFillConstraint);

            Canvas.Document.Children.Add(polygon);
        }

        public override void Reset()
        {
            Points.Clear();
        }

        public override void Dispose()
        {
            PolyCircleBrush.Dispose();
            PolyLineBrush.Dispose();
            base.Dispose();
        }
    }
}