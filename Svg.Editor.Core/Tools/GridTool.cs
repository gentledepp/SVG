using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Svg.Editor.Events;
using Svg.Editor.Extensions;
using Svg.Editor.Interfaces;
using Svg.Editor.UndoRedo;
using Svg.Interfaces;
using Svg.Transforms;

namespace Svg.Editor.Tools
{
    public class GridTool : UndoableToolBase
    {
        #region Private fields

        private const double Gamma = 90f;
        private const float FloatError = 0.01f;

        private float StepSizeY
        {
            get
            {
                object stepSizeY;
                if (!Properties.TryGetValue(StepSizeYKey, out stepSizeY))
                    stepSizeY = 20.0f;
                return Convert.ToSingle(stepSizeY);
            }
        }

        private float StepSizeX { get; }

        private double Alpha
        {
            get
            {
                object alpha;
                if (!Properties.TryGetValue(AlphaAngleKey, out alpha))
                    alpha = 30.0f;
                return Convert.ToSingle(alpha);
            }
        }

        private Pen _pen;

        //private Pen _pen2;
        private Brush _brush;
        //private Brush _brush2;

        private bool _isSnappingInProgress;
        private bool _areElementsMoved;
        private PointF _generalTranslation;

        #endregion

        #region Private properties

        private Dictionary<double, double> CachedDiagonals { get; } = new Dictionary<double, double>();

        private Brush Brush => _brush ?? (_brush = SvgEngine.Factory.CreateSolidBrush(SvgEngine.Factory.CreateColorFromArgb(255, 210, 210, 210)));
        //private Brush Brush2 => _brush2 ?? (_brush2 = Engine.Factory.CreateSolidBrush(Engine.Factory.CreateColorFromArgb(180, 0, 0, 0)));
        private Pen Pen => _pen ?? (_pen = SvgEngine.Factory.CreatePen(Brush, 1));
        //private Pen Pen2 => _pen2 ?? (_pen2 = Engine.Factory.CreatePen(Brush2, 1));

        #endregion

        #region Public properties

        public string IconGridOn { get; set; } = "ic_grid_on.svg";
        public string IconGridOff { get; set; } = "ic_grid_off.svg";

        public const string IsSnappingEnabledKey = "issnappingenabled";
        public const string StepSizeYKey = "stepsizey";
        public const string AlphaAngleKey = "alpha";

        public bool IsSnappingEnabled
        {
            get
            {
                object isSnappingEnabled;
                if (!Properties.TryGetValue(IsSnappingEnabledKey, out isSnappingEnabled))
                    isSnappingEnabled = true;
                return (bool) isSnappingEnabled;
            }
            set { Properties[IsSnappingEnabledKey] = value; }
        }

        public override int InputOrder => 100; // must be before movetool!

        public bool IsVisible { get; set; } = true;
        
        #endregion

        public GridTool(IDictionary<string, object> properties, IUndoRedoService undoRedoService)
            : base("Grid", properties, undoRedoService)
        {
            // using triangle calculation to determine the x and y steps based on stepsize (y) and angle (alpha)
            // http://www.arndt-bruenner.de/mathe/scripts/Dreiecksberechnung.htm
            /*
                                                  XXX+
                                               XXX   |
                                             XX      |
                                          XXX  B = ? |
                                        XXX          |
                                      XXX            |
                           c = ?   XXX               |
                                 XXX                 |
                               XXX                   |  a = 20
                             XXX                     |
                           XXX                       |
                         XXX                         |
                       XXX                           |
                     XXX    A = 27.3          G = 90 |
                    XX-------------------------------+
                              b = ?
             * */
            var a = StepSizeY / 2;
            var beta = 180f - (Alpha + Gamma);
            var b = a * SinDegree(beta) / SinDegree(Alpha);
            //var c = a * SinDegree(Gamma) / SinDegree(Alpha);
            StepSizeX = (float) b * 2;

            ToolType = ToolType.Modify;
        }

        #region Overrides

        public override async Task Initialize(ISvgDrawingCanvas ws)
        {
            await base.Initialize(ws);

            // add tool commands
            Commands = new List<IToolCommand>
            {
                new ToggleGridCommand(ws, this, "Toggle Grid"),
                new ToggleSnappingCommand(ws, this, "Toggle Snapping")
            };

            // initialize with callbacks
            WatchDocument(ws.Document);
        }

        public override async Task OnPreDraw(IRenderer renderer, ISvgDrawingCanvas ws)
        {
            await base.OnPreDraw(renderer, ws);

            if (!IsVisible) return;

            // draw gridlines
            DrawGridLines(renderer, ws);

            // draw debug stuff
            //var canvasx = -ws.RelativeTranslate.X;
            //var canvasy = -ws.RelativeTranslate.Y;
            //renderer.DrawCircle(canvasx, canvasy, 50, Pen); // point should remain in top left corner on screen
            //renderer.DrawCircle(0, 0, 20, Pen2); // point on canvas - should move along
            const float originLength = 50f;
            renderer.DrawLine(0f, -originLength, 0f, originLength, Pen);
            renderer.DrawLine(-originLength, 0f, originLength, 0f, Pen);
        }

        public override void OnDocumentChanged(SvgDocument oldDocument, SvgDocument newDocument)
        {
            // add watch for element snapping
            WatchDocument(newDocument);
            UnWatchDocument(oldDocument);
        }

        public override Task OnUserInput(UserInputEvent @event, ISvgDrawingCanvas ws)
        {
            var me = @event as MoveEvent;
            if (me != null)
            {
                _areElementsMoved = true;
                _generalTranslation = null;
            }
            else
            {
                _generalTranslation = null;
                _areElementsMoved = false;
            }
            return Task.FromResult(true);
        }

        public override void Dispose()
        {
            base.Dispose();

            _pen?.Dispose();
            //_pen2?.Dispose();
            _brush?.Dispose();
            //_brush2?.Dispose();
        }

        #endregion

        #region GridLines

        private void DrawGridLines(IRenderer renderer, ISvgDrawingCanvas ws)
        {
            var screenTopLeft = ws.ScreenToCanvas(0, 0);

            var relativeCanvasTranslationX = screenTopLeft.X % StepSizeX;
            var relativeCanvasTranslationY = screenTopLeft.Y % StepSizeY;

            var height = renderer.Height / ws.ZoomFactor;
            var yPosition = height - height % StepSizeY + StepSizeY * 2;
            var stepSize = (int) Math.Round(StepSizeY, 0);

            var x = screenTopLeft.X - relativeCanvasTranslationX - (StepSizeX * 2);
            // subtract 2x stepsize so gridlines always start from "out of sight" and lines do not start from a visible x-border
            var y = screenTopLeft.Y - relativeCanvasTranslationY;

            // cache these expensive calculations for performance
            var cachedDiagonalKey = renderer.Width + renderer.Height;
            double diagonal;
            if (!CachedDiagonals.TryGetValue(cachedDiagonalKey, out diagonal))
            {
                diagonal = Math.Sqrt(Math.Pow(renderer.Width, 2) + Math.Pow(renderer.Height, 2));
                CachedDiagonals[cachedDiagonalKey] = diagonal;
            }
            var lineLength = diagonal / ws.ZoomFactor + stepSize * 8;

            for (var i = y - yPosition; i <= y + yPosition; i += stepSize)
            {
                DrawLineLeftToBottom(renderer, i, x, lineLength); /* \ */
            }

            for (var i = y; i <= y + 2 * yPosition; i += stepSize)
            {
                DrawLineLeftToTop(renderer, i, x, lineLength); /* / */
            }
        }

        // line looks like this -> /
        private void DrawLineLeftToTop(IRenderer renderer, float y, float canvasX, double lineLength)
        {
            var startX = canvasX;
            var startY = y;
            var stopX = (float) (lineLength * Math.Cos(Alpha * (Math.PI / 180))) + canvasX;
            var stopY = y - (float) (lineLength * Math.Sin(Alpha * (Math.PI / 180)));


            renderer.DrawLine(
                startX,
                startY,
                stopX,
                stopY,
                Pen);
        }

        // line looks like this -> \
        private void DrawLineLeftToBottom(IRenderer renderer, float y, float canvasX, double lineLength)
        {
            var startX = canvasX;
            var startY = y;
            var endX = (float) (lineLength * Math.Cos(Alpha * (Math.PI / 180))) + canvasX;
            var endY = y + (float) (lineLength * Math.Sin(Alpha * (Math.PI / 180)));

            renderer.DrawLine(
                startX,
                startY,
                endX,
                endY,
                Pen);
        }

        #endregion

        #region Snapping

        /// <summary>
        /// Subscribes to all visual elements "Add/RemoveChild" handlers and their "transformCollection changed" event
        /// </summary>
        /// <param name="document"></param>
        private void WatchDocument(SvgDocument document)
        {
            if (document == null)
                return;

            document.ChildAdded -= OnChildAdded;
            document.ChildAdded += OnChildAdded;

            foreach (var child in document.Children.OfType<SvgVisualElement>())
            {
                Subscribe(child);
            }
        }

        private void UnWatchDocument(SvgDocument document)
        {
            if (document == null)
                return;

            document.ChildAdded -= OnChildAdded;

            foreach (var child in document.Children.OfType<SvgVisualElement>())
            {
                Unsubscribe(child);
            }
        }

        private void Subscribe(SvgElement child)
        {
            if (!(child is SvgVisualElement))
                return;

            child.ChildAdded -= OnChildAdded;
            child.ChildAdded += OnChildAdded;
            child.ChildRemoved -= OnChildRemoved;
            child.ChildRemoved += OnChildRemoved;
            child.AttributeChanged -= OnAttributeChanged;
            child.AttributeChanged += OnAttributeChanged;
        }

        private void Unsubscribe(SvgElement child)
        {
            if (!(child is SvgVisualElement))
                return;

            child.ChildAdded -= OnChildAdded;
            child.ChildRemoved -= OnChildRemoved;
            child.AttributeChanged -= OnAttributeChanged;
        }

        private void OnChildAdded(object sender, ChildAddedEventArgs e)
        {
            Subscribe(e.NewChild);
            SnapElementToGrid(e.NewChild);
        }

        private void OnChildRemoved(object sender, ChildRemovedEventArgs e)
        {
            Unsubscribe(e.RemovedChild);
        }

        private void OnAttributeChanged(object sender, AttributeEventArgs e)
        {
            // if snapping is currently in progress, just skip (otherwise we might cause stackoverflowexception!
            if (_isSnappingInProgress || !IsSnappingEnabled)
                return;

            if (string.Equals(e.Attribute, "transform"))
            {
                var element = (SvgElement) sender;

                // if transform was changed and rotation has been added, skip snapping
                var oldRotation = (e.OldValue as SvgTransformCollection)?.GetMatrix()?.RotationDegrees;
                var newRotation = (e.Value as SvgTransformCollection)?.GetMatrix()?.RotationDegrees;
                if (oldRotation != newRotation)
                    return;

                // otherwise we need to reevaluate the translate of that particular element
                SnapElementToGrid(element);

                return;
            }

            var line = sender as SvgLine;
            if (line != null && Regex.IsMatch(e.Attribute, @"^y[12]$"))
            {
                try
                {
                    _isSnappingInProgress = true;

                    float absoluteDeltaX, absoluteDeltaY;

                    // Get start and end points from line in canvas space
                    var points = line.GetTransformedLinePoints();

                    // Matrix for transforming the calculated delta back to line space
                    var m = line.Transforms.GetMatrix();
                    m.Invert();

                    switch (e.Attribute)
                    {
                        case "y1":
                            SnapPointToGrid(points[0].X, points[0].Y, out absoluteDeltaX, out absoluteDeltaY);
                            points[0].X += absoluteDeltaX;
                            points[0].Y += absoluteDeltaY;
                            m.TransformPoints(points);
                            var formerLineStartX = line.StartX.Clone();
                            var formerLineStartY = line.StartY.Clone();
                            UndoRedoService.ExecuteCommand(new UndoableActionCommand("Snap line start to grid", o =>
                            {
                                line.StartX = points[0].X;
                                line.StartY = points[0].Y;
                            }, o =>
                            {
                                line.StartX = formerLineStartX;
                                line.StartY = formerLineStartY;
                            }), hasOwnUndoRedoScope: false);
                            break;
                        case "y2":
                            SnapPointToGrid(points[1].X, points[1].Y, out absoluteDeltaX, out absoluteDeltaY);
                            points[1].X += absoluteDeltaX;
                            points[1].Y += absoluteDeltaY;
                            m.TransformPoints(points);
                            var formerLineEndX = line.EndX.Clone();
                            var formerLineEndY = line.EndY.Clone();
                            UndoRedoService.ExecuteCommand(new UndoableActionCommand("Snap line end to grid", o =>
                            {
                                line.EndX = points[1].X;
                                line.EndY = points[1].Y;
                            }, o =>
                            {
                                line.EndX = formerLineEndX;
                                line.EndY = formerLineEndY;
                            }), hasOwnUndoRedoScope: false);
                            break;
                    }
                }
                finally
                {
                    _isSnappingInProgress = false;
                }

            }
        }

        private PointF GetSnappingPoint(SvgElement element)
        {
            var line = element as SvgLine;
            if (line != null)
            {
                return line.GetTransformedLinePoints()[0];
            }

            return (element as SvgVisualElement)?.GetBoundingBox().Location ?? PointF.Create(0, 0);
        }

        private void SnapElementToGrid(SvgElement element)
        {
            if (!IsSnappingEnabled)
                return;

            var ve = element as SvgVisualElement;
            if (ve == null || ve.HasConstraints(NoSnappingConstraint))
                return;

            try
            {
                _isSnappingInProgress = true;

                // snap to grid:
                // get absolute point
                var snap = GetSnappingPoint(element);

                float absoluteDeltaX, absoluteDeltaY;

                // for the first moved element, store the translation and translate all other elements by the same translation
                // so if multiple elements are moved, their position relative to each other stays the same
                if (_areElementsMoved && _generalTranslation != null)
                {
                    absoluteDeltaX = _generalTranslation.X;
                    absoluteDeltaY = _generalTranslation.Y;
                }
                else
                {
                    SnapPointToGrid(snap.X, snap.Y, out absoluteDeltaX, out absoluteDeltaY);

                    if (_generalTranslation == null)
                    {
                        _generalTranslation = PointF.Create(absoluteDeltaX, absoluteDeltaY);
                    }
                }

                var formerMx = ve.Transforms.GetMatrix().Clone();
                var mx = ve.CreateTranslation(absoluteDeltaX, absoluteDeltaY);

                UndoRedoService.ExecuteCommand(new UndoableActionCommand("Snap element to grid", o =>
                {
                    ve.SetTransformationMatrix(mx);
                }, o =>
                {
                    ve.SetTransformationMatrix(formerMx);
                }), hasOwnUndoRedoScope: false);

            }
            finally
            {
                _isSnappingInProgress = false;
            }
        }

        private void SnapPointToGrid(float x, float y, out float absoluteDeltaX, out float absoluteDeltaY)
        {
            // determine next intersection of gridlines
            // so we determine which point P1, P2 is the nearest one
            // afterwards, see if the intermediary points (Px, Pz) are even closer 
            // and in that case transition to those.

            // so when we have the ankle points (P1, P2), determine if an intermediate point (Px, Pz) is even nearer
            /*
                                                    Px
                                                                                              Px = P1 + Vx
                                                 XXX+XX
                                              XXXX  | XXXX                                    Px = P1 + (P1.x + b)
                                            XXX     |    XXXX                                           (P1.y - a)
                                          XX   B = ?|       XXX
                                       XXX          |         XXX
                                     XXX            |            XXX
                     c = ?         XXX              |               XXX
                                XXX                 |                 XXX
                              XXX                   |  a = 20            XXX
                            XXX                     |                      XXX
                          XXX                       |                         XXX
                        XXX                         |                           XX
                      XXX                    G = 90 |                            XXX
                    XXX    A = 27.3                 |                              XXX
                P1 XX---------------------------------------------------------------+    P2
                   XXX           b = ?              |                             XX
                      XX                            |                           XXX
                       XXX                          |                         XXX
                          XXX                       |                       XXX
                            XXX                     |                     XXX
                              XXX                   |                  XXXX
                                XXX                 |                XXX
                                  XXXX              |              XXX
                                     XX             |            XXX
                                      XXX           |          XXX
                                        XXX         |       XXXX
                                          XXX       |     XXX
                                            XXX     |   XXX
                                              XXX  ++ XXX
                                                 XXX XX

                                                   Pz

             * 
             * */

            var halfStepSizeX = StepSizeX / 2;
            var halfStepSizeY = StepSizeY / 2;

            var diffX = x % StepSizeX;
            var diffY = y % StepSizeY;

            // if x is already snapped, just correct the floating point error by substracting StepSizeX
            if (Math.Abs(Math.Abs(diffX) - StepSizeX) < FloatError)
            {
                diffX -= StepSizeX * Math.Sign(diffX);
            }

            // if y is already snapped, just correct the floating point error by substracting StepSizeY
            if (Math.Abs(Math.Abs(diffY) - StepSizeY) < FloatError)
            {
                diffY -= StepSizeY * Math.Sign(diffY);
            }

            float deltaX = 0, deltaY = 0;

            // see if intermediary point is even nearer but also take Y coordinate into consideration!!
            if (diffX > halfStepSizeX - FloatError)
            {
                // transition to intermediary point
                deltaX = halfStepSizeX;
                deltaY = (Math.Abs(diffY) > halfStepSizeY - FloatError ? halfStepSizeY : -halfStepSizeY) * (Math.Abs(diffY) > FloatError ? Math.Sign(diffY) : 1);
            }
            else if (diffX < -halfStepSizeX + FloatError)
            {
                deltaX = -halfStepSizeX;
                deltaY = (Math.Abs(diffY) > halfStepSizeY - FloatError ? halfStepSizeY : -halfStepSizeY) * (Math.Abs(diffY) > FloatError ? Math.Sign(diffY) : 1);
            }
            else if (diffY > halfStepSizeY - FloatError)
            {
                deltaY = halfStepSizeY;
                deltaX = (Math.Abs(diffX) > halfStepSizeX - FloatError ? halfStepSizeX : -halfStepSizeX) * (Math.Abs(diffX) > FloatError ? Math.Sign(diffX) : 1);
            }
            else if (diffY < -halfStepSizeY + FloatError)
            {
                deltaY = -halfStepSizeY;
                deltaX = (Math.Abs(diffX) > halfStepSizeX - FloatError ? halfStepSizeX : -halfStepSizeX) * (Math.Abs(diffX) > FloatError ? Math.Sign(diffX) : 1);
            }

            absoluteDeltaX = deltaX - diffX;
            absoluteDeltaY = deltaY - diffY;
        }

        #endregion

        #region Private helpers

        private static double SinDegree(double value)
        {
            return RadianToDegree(Math.Sin(DegreeToRadian(value)));
        }
        private static double DegreeToRadian(double angle)
        {
            return Math.PI * angle / 180.0;
        }
        private static double RadianToDegree(double angle)
        {
            return angle * (180.0 / Math.PI);
        }

        #endregion

        #region Inner types

        private class ToggleGridCommand : ToolCommand
        {
            private readonly ISvgDrawingCanvas _canvas;

            public ToggleGridCommand(ISvgDrawingCanvas canvas, GridTool tool, string name)
                : base(tool, name, (o) => { }, iconName: tool.IconGridOff, sortFunc: (tc) => 2000)
            {
                _canvas = canvas;
            }

            private new GridTool Tool => (GridTool) base.Tool;

            public override string GroupIconName => Tool.IsVisible ? Tool.IconGridOn : Tool.IconGridOff;

            public override void Execute(object parameter)
            {
                var t = Tool;
                t.IsVisible = !t.IsVisible;
                IconName = t.IsVisible ? t.IconGridOff : t.IconGridOn;
                _canvas.FireInvalidateCanvas();
                _canvas.FireToolCommandsChanged();
            }
        }

        private class ToggleSnappingCommand : ToolCommand
        {
            private readonly ISvgDrawingCanvas _canvas;

            public ToggleSnappingCommand(ISvgDrawingCanvas canvas, GridTool tool, string name)
                : base(tool, name, (o) => { }, iconName: tool.IconGridOff, sortFunc: (tc) => 2000)
            {
                _canvas = canvas;
            }
            private new GridTool Tool => (GridTool)base.Tool;

            public override string GroupIconName => Tool.IsVisible ? Tool.IconGridOn : Tool.IconGridOff;

            public override void Execute(object parameter)
            {
                var t = (GridTool) Tool;
                t.IsSnappingEnabled = !t.IsSnappingEnabled;
                IconName = t.IsSnappingEnabled ? t.IconGridOff : t.IconGridOn;
                _canvas.FireToolCommandsChanged();
            }
        }

        #endregion
    }
}