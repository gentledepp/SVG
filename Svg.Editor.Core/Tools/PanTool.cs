using System.Collections.Generic;
using System.Threading.Tasks;
using Svg.Editor.Events;
using Svg.Editor.Interfaces;

namespace Svg.Editor.Tools
{
    public class PanTool : ToolBase
    {
        public PanTool(IDictionary<string, object> properties)
            : base("Pan", properties)
        {
            IconName = "ic_pan_tool.svg";
            ToolType = ToolType.View;
        }

        public override Task Initialize(ISvgDrawingCanvas ws)
        {
            IsActive = true;
            return base.Initialize(ws);
        }

        public override Task OnUserInput(UserInputEvent @event, ISvgDrawingCanvas ws)
        {
            if (!IsActive)
                return Task.FromResult(true);

            var ev = @event as MoveEvent;

            if (ev == null || ev.PointerCount != 2)
                return Task.FromResult(true);

            ws.Translate.X += ev.RelativeDelta.X;
            ws.Translate.Y += ev.RelativeDelta.Y;
            ws.FireInvalidateCanvas();

            ws.FireInvalidateCanvas();

            return Task.FromResult(true);
        }
    }
}
