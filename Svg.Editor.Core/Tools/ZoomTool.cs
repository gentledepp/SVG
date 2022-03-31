using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Svg.Editor.Events;
using Svg.Editor.Interfaces;
using Svg.Interfaces;

namespace Svg.Editor.Tools
{
    public sealed class ZoomTool : ToolBase
    {
        public const string MinScaleKey = "minscale";
        public const string MaxScaleKey = "maxscale";
        public const string ZoomX1CommandKey = "zoomx1";
        public const string ZoomX2CommandKey = "zoomx2";
        public const string ZoomInCommandKey = "zoomin";
        public const string ZoomOutCommandKey = "zoomout";

        #region Private fields and properties
        private bool _focused;
        private float CurrentFocusX { get; set; }
        private float CurrentFocusY { get; set; }
        //private Brush _purpleBrush;
        //private Pen _purplePen;
        //private Brush _orangeBrush;
        //private Pen _orangePen;
        //private Brush PurpleBrush => _purpleBrush ?? (_purpleBrush = Engine.Factory.CreateSolidBrush(Engine.Factory.CreateColorFromArgb(255, 210, 80, 210)));
        //private Pen PurplePen => _purplePen ?? (_purplePen = Engine.Factory.CreatePen(PurpleBrush, 5));
        //private Brush OrangeBrush => _orangeBrush ?? (_orangeBrush = Engine.Factory.CreateSolidBrush(Engine.Factory.CreateColorFromArgb(255, 220, 160, 60)));
        //private Pen OrangePen => _orangePen ?? (_orangePen = Engine.Factory.CreatePen(OrangeBrush, 5));

        #endregion

        public ZoomTool(IDictionary<string, object> properties) : base("Zoom", properties)
        {
            IconName = "ic_zoom.svg";
            ToolType = ToolType.View;
        }

        #region Public properties

        public float MinScale
        {
            get
            {
                object minScale;
                if (!Properties.TryGetValue(MinScaleKey, out minScale))
                    minScale = .5f;
                return Convert.ToSingle(minScale);
            }
            set { Properties[MinScaleKey] = value; }
        }

        public float MaxScale
        {
            get
            {
                object maxScale;
                if (!Properties.TryGetValue(MaxScaleKey, out maxScale))
                    maxScale = 5f;
                return Convert.ToSingle(maxScale);
            }
            set { Properties[MaxScaleKey] = value; }
        }

        public bool ZoomX1CommandEnabled
        {
            get
            {
                object zoomX1;
                return Properties.TryGetValue(ZoomX1CommandKey, out zoomX1) && Convert.ToBoolean(zoomX1);
            }
            set { Properties[ZoomX1CommandKey] = value; }
        }

        public bool ZoomX2CommandEnabled
        {
            get
            {
                object zoomX2;
                return Properties.TryGetValue(ZoomX2CommandKey, out zoomX2) && Convert.ToBoolean(zoomX2);
            }
            set { Properties[ZoomX2CommandKey] = value; }
        }

        public bool ZoomInCommandEnabled
        {
            get
            {
                object zoomIn;
                return Properties.TryGetValue(ZoomInCommandKey, out zoomIn) && Convert.ToBoolean(zoomIn);
            }
            set { Properties[ZoomInCommandKey] = value; }
        }

        public bool ZoomOutCommandEnabled
        {
            get
            {
                object zoomOut;
                return Properties.TryGetValue(ZoomOutCommandKey, out zoomOut) && Convert.ToBoolean(zoomOut);
            }
            set { Properties[ZoomOutCommandKey] = value; }
        }

        #endregion


        #region Overides

        public override bool IsActive => Canvas?.ZoomEnabled ?? false;

        public override async Task Initialize(ISvgDrawingCanvas ws)
        {
            await base.Initialize(ws);

            Commands = new[]
            {
                new ToolCommand(this, "Show all", x =>
                {
                    var worldBounds = Canvas.Document.CalculateDocumentBounds();
                    if (worldBounds.IsEmpty)
                    {
                        Canvas.ZoomFactor = 1;
                        Canvas.ZoomFocus = PointF.Create(0, 0);
                        Canvas.Translate = PointF.Create(0, 0);
                        Canvas.FireInvalidateCanvas();
                        return;
                    }
                    Canvas.ZoomFactor = Math.Min(Canvas.ScreenWidth / worldBounds.Width,
                        Canvas.ScreenHeight / worldBounds.Height);
                    Canvas.ZoomFocus = PointF.Create(0, 0);
                    var offsetX = -worldBounds.Left * Canvas.ZoomFactor;
                    var marginX = (Canvas.ScreenWidth - worldBounds.Width * Canvas.ZoomFactor) / 2;
                    var offsetY = -worldBounds.Top*Canvas.ZoomFactor;
                    var marginY = (Canvas.ScreenHeight - worldBounds.Height * Canvas.ZoomFactor) / 2;
                    Canvas.Translate = PointF.Create(offsetX + marginX, offsetY + marginY);
                    Canvas.FireInvalidateCanvas();
                }, iconName:"ic_aspect_ratio.svg", sortFunc:x => 1450),
                new ToolCommand(this, "Zoom in +", x =>
                {
                    var f = Canvas.ZoomFactor + 0.25f;
                    Canvas.ZoomFactor = Math.Max(MinScale, Math.Min(f, MaxScale));
                    Canvas.FireInvalidateCanvas();
                }, o => ZoomInCommandEnabled, iconName:"ic_zoom_in.svg", sortFunc:x => 1500),
                new ToolCommand(this, "Zoom out -", x =>
                {
                    var f = Canvas.ZoomFactor - 0.25f;
                    Canvas.ZoomFactor = Math.Max(MinScale, Math.Min(f, MaxScale));
                    Canvas.FireInvalidateCanvas();
                }, o => ZoomOutCommandEnabled, iconName:"ic_zoom_out.svg", sortFunc:x => 1550),
                new ToolCommand(this, "100 %", x =>
                {
                    Canvas.ZoomFactor = Math.Max(MinScale, Math.Min(1, MaxScale));
                    Canvas.FireInvalidateCanvas();
                }, o => ZoomX1CommandEnabled, iconName:"ic_zoom_100.svg", sortFunc:x => 1600),
                new ToolCommand(this, "200 %", x =>
                {
                    Canvas.ZoomFactor = Math.Max(MinScale, Math.Min(2, MaxScale));
                    Canvas.FireInvalidateCanvas();
                }, o => ZoomX2CommandEnabled, iconName:"ic_zoom_200.svg", sortFunc:x => 1650)
            };
        }

        public override Task OnUserInput(UserInputEvent @event, ISvgDrawingCanvas ws)
        {
            var b = ws.Document.Bounds;
            if (b.Width == 0 && b.Height == 0)
            {
                MinScale = 0.5f;
            }
            else
            {
                var minZoomWidth = (ws.ScreenWidth / b.Width);
                var minZoomHeight = (ws.ScreenHeight / b.Height);

                MinScale = Math.Max(minZoomHeight, minZoomWidth);
            }

            if (!IsActive)
                return Task.FromResult(true);

            var se = @event as ScaleEvent;
            if (se == null)
                return Task.FromResult(true);

            switch (se.Status)
            {
                case ScaleStatus.Scaling:
                    CurrentFocusX = se.FocusX;
                    CurrentFocusY = se.FocusY;
                    var zoomFactor = GetBoundedZoomFactor(se, ws);
                    // jusst set focal point if not focused and zoom factor actually changed
                    if (!_focused && Math.Abs(zoomFactor - ws.ZoomFactor) > 0.01f)
                    {
                        /*
                         * A zoom with a focal point is a mix of scaling and translation. When the user zooms in, we will place the focal point on the canvas
                         * coordinates that relate to the screen coordinates of the gesture and scale the canvas around that point. Scaling around the focal
                         * point means effectively stretching all what's left of the focus to the left, all what's above to the top and so on. All what is
                         * stretched over the bounds of the screen will be cut out by the renderer. If the focus was in the center of the screen, the objects
                         * on the sides will exit when zooming in and enter when zooming out again, while objects in the middle stay centered. If the focus is
                         * in the top left, the objects on the far right and bottom sides will exit when zooming in and enter when zooming out, and so on.
                         * 
                         * Let's assume the user wants to zoom to an object in the top left of the screen, and then zoom out from the screen center again. The
                         * first zoom is executed just as described, the canvas is stretched to the right and bottom sides while the top left remains on screen.
                         * Now, the focal point will change to the screen center, which means the scaling will happen around that point. Just changing the focus
                         * will result in a jump to the new focus and the user losing sight of the objects in the top left, but we want the objects in the top
                         * left to stay where they are until we actually change the zoom factor. To acheive this, we need to save this position by adding a
                         * translation to the canvas. This translation has to cope for the jump between the two focal points, so we take the offset between them,
                         * relative to the zoom factor we had at the previous zoom, and translate the canvas to the opposite. The process is explained graphically
                         * with a canvas of the size 4*3.
                         * 
                         * 
                         *     P..... previous focus
                         *     F..... focal point
                         *     p..... previous zoom factor
                         *     z..... zoom facor
                         *     
                         *       a)   F = ?|?                   b)    F = 0|0                                      c)      F = 4|0
                         *            z = 1                           z = 2                                                z = 2
                         *     
                         *       0                   4         0                                       8        -4                  0                    4
                         *     0 +-------------------+       0 F-------------------+-------------------+       0 +------------------+--------------------F 
                         *       |                   |         |                   |                   |         |                  |                    | 
                         *       |                   |         |                   |                   |         |                  |                    | 
                         *       |                   |         |                   |                   |         |                  |                    | 
                         *       |                   |         |                   |                   |         |                  |                    | 
                         *       |                   |         |                   |                   |         |                  |                    | 
                         *     3 +-------------------+         +-------------------+                   |         |                  +--------------------+ 
                         *                                     |                                       |         |                                       |
                         *                                     |                                       |         |                                       |
                         *                                     |                                       |         |                                       |
                         *                                     |                                       |         |                                       |
                         *                                   6 +---------------------------------------+       6 +---------------------------------------+
                         *     
                         * 
                         * If the zoom factor was 1, we don't need to translate anything (a). If it was 2, the canvas was doubled in size and translated to a
                         * proportion of the focus. If the focus was 0|0, we don't need to translate either (b). But if the focus was set to 4|0 (c), we need to
                         * translate -4 on the x axis to save this translation.
                         * 
                         * 
                         * 
                         *        d)    F = 4|0                                                          e)      F = 0|0
                         *              z = 3                                                                    z = 3
                         *              P = 0|0                                                                  P = 4|0
                         *              p = 2                                                                    p = 2
                         *                                                                        
                         *      -2         0                   4                             10       -6                             0                   4         6
                         *     0 +---------P-------------------F-----------------------------+       0 +-----------------------------F-------------------P---------+
                         *       |         |                   |                             |         |                             |                   |         |
                         *       |         |                   |                             |         |                             |                   |         |
                         *       |         |                   |                             |         |                             |                   |         |
                         *       |         |                   |                             |         |                             |                   |         |
                         *       |         |                   |                             |         |                             |                   |         |
                         *       |         +-------------------+                             |         |                             +-------------------+         |
                         *       |                                                           |         |                                                           |
                         *       |                                                           |         |                                                           |
                         *       |                                                           |         |                                                           |
                         *       |                                                           |         |                                                           |                         
                         *       |                                                           |         |                                                           |
                         *       |                                                           |         |                                                           |
                         *       |                                                           |         |                                                           |
                         *       |                                                           |         |                                                           |
                         *       |                                                           |         |                                                           |
                         *       |                                                           |         |                                                           |
                         *     6 +-----------------------------------------------------------+       6 +-----------------------------------------------------------+
                         * 
                         * 
                         */
                        var canvasFocus = ws.ScreenToCanvas(CurrentFocusX, CurrentFocusY);
                        // save translate from previous zoom
                        ws.Translate -= (ws.ZoomFocus - canvasFocus) * (ws.ZoomFactor - 1);
                        ws.ZoomFocus = canvasFocus;
                        _focused = true;
                    }
                    // Don't let the object get too small or too large.
                    ws.ZoomFactor = zoomFactor;
                    ws.FireInvalidateCanvas();
                    break;
                case ScaleStatus.Start:
                    _focused = false;
                    break;
            }

            return Task.FromResult(true);
        }

        //public override async Task OnDraw(IRenderer renderer, SvgDrawingCanvas ws)
        //{
        //    await base.OnDraw(renderer, ws);

        //    renderer.Graphics.Save();

        //    var canvasCurrentFocus = ws.ScreenToCanvas(CurrentFocusX, CurrentFocusY);
        //    renderer.DrawCircle(canvasCurrentFocus.X, canvasCurrentFocus.Y, 18, PurplePen);
        //    renderer.DrawCircle(ws.ZoomFocus.X, ws.ZoomFocus.Y, 22, OrangePen);

        //    renderer.Graphics.Restore();
        //}

        //public override async Task OnDraw(IRenderer renderer, SvgDrawingCanvas ws)
        //{
        //    await base.OnDraw(renderer, ws);

        //    renderer.Graphics.Save();

        //    renderer.DrawRectangle(ws.Document.CalculateDocumentBounds(), PurplePen);

        //    renderer.Graphics.Restore();
        //}

        #endregion

        #region Private helpers

        private float GetBoundedZoomFactor(ScaleEvent se, ISvgDrawingCanvas ws)
        {
            var newZoomFactor = ws.ZoomFactor * se.ScaleFactor;

            return Math.Max(MinScale, Math.Min(newZoomFactor, MaxScale));
        }

        #endregion
    }
}
