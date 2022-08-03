using System.Threading.Tasks;
using Svg.Editor.Gestures;
using Svg.Editor.Interfaces;
using Svg.Editor.Tools;

namespace Svg.Editor.Samples.Forms.Tools
{
	public class AddItemTool : ToolBase
	{
		public AddItemTool() : base("Add item", null)
		{
		}

        public override async Task Initialize(ISvgDrawingCanvas ws)
        {
            await base.Initialize(ws);

            IsActive = false;
        }

		protected override async Task OnLongPress(LongPressGesture longPress)
		{
			await base.OnLongPress(longPress);

			if(!IsActive) return;

            await Canvas.AddItemInScreenCenter(new SvgCircle { Radius = 10 });
		}
	}
}
