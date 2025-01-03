using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using iCL.Modules.UserInteraction;
using Svg.Editor.Tools;

namespace Svg.Editor.CrossSample.Avalon.Services
{
    public class StrokeStyleOptionsInputService : IStrokeStyleOptionsInputService
    {
        public async Task<StrokeStyleTool.StrokeStyleOptions> GetUserInput(string title, IEnumerable<string> strokeDashOptions, int strokeDashSelected, IEnumerable<string> strokeWidthOptions,
            int strokeWidthSelected)
        {
            var dashes = strokeDashOptions.ToArray();
            var dash = await UserInteractionServiceExt.UserInteractionInst.ActionSheetAsync(title, dashes);

            if (dash == null || dash == "cancel")
                return null;
            var widths = strokeWidthOptions.ToArray();
            var width = await UserInteractionServiceExt.UserInteractionInst.ActionSheetAsync(title, widths);
            if (width == null || width == "cancel")
                return null;
            return new StrokeStyleTool.StrokeStyleOptions() { StrokeDashIndex = Array.IndexOf(dashes, dash), StrokeWidthIndex = Array.IndexOf(widths, width) };
        }
    }
}
