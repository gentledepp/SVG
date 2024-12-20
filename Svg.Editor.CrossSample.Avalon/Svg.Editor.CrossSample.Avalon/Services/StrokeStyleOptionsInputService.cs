using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Svg.Editor.Sample.Avalon.Dialog;
using Svg.Editor.Tools;

namespace Svg.Editor.Sample.Avalon.Services
{
    public class StrokeStyleOptionsInputService : IStrokeStyleOptionsInputService
    {
        public static Func<Window> GetWindow { get;set; } = () => new Window();
        public async Task<StrokeStyleTool.StrokeStyleOptions> GetUserInput(string title, IEnumerable<string> strokeDashOptions, int strokeDashSelected, IEnumerable<string> strokeWidthOptions,
            int strokeWidthSelected)
        {
            var dashes = strokeDashOptions.ToArray();
            var dash = await ActionSheetDialog.ShowActionSheet(GetWindow(), title, "cancel", dashes);

            if (dash == null || dash == "cancel")
                return null;
            var widths = strokeWidthOptions.ToArray();
            var width = await ActionSheetDialog.ShowActionSheet(GetWindow(), title, "cancel", widths);
            if (width == null || width == "cancel")
                return null;
            return new StrokeStyleTool.StrokeStyleOptions() { StrokeDashIndex = Array.IndexOf(dashes, dash), StrokeWidthIndex = Array.IndexOf(widths, width) };
        }
    }
}
