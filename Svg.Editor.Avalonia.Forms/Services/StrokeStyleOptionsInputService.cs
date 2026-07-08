using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Svg.Editor.Avalon.Forms.Dialog;
using Svg.Editor.Interfaces;
using Svg.Editor.Tools;

namespace Svg.Editor.Avalon.Forms.Services
{
    public class StrokeStyleOptionsInputService : IStrokeStyleOptionsInputService
    {
        private readonly ILocalizationService _localizationService;
        private readonly IUserInteraction _userInteractionService;

        public StrokeStyleOptionsInputService() 
        {
            _localizationService = SvgEngine.Resolve<ILocalizationService>();
            _userInteractionService = SvgEngine.Resolve<IUserInteraction>();

        }

        public async Task<StrokeStyleTool.StrokeStyleOptions> GetUserInput(string title, IEnumerable<string> strokeDashOptions, int strokeDashSelected, IEnumerable<string> strokeWidthOptions,
            int strokeWidthSelected)
        {
            var dashes = strokeDashOptions.ToArray();
            var dash = await _userInteractionService.ActionSheetAsync(title, dashes, cancelButton: _localizationService.GetString("Svg.Editor.Global.Cancel"), cancellable: true, selectedIndex: strokeDashSelected);

            if (dash == null)
                return null;
            var widths = strokeWidthOptions.ToArray();
            var width = await _userInteractionService.ActionSheetAsync(title, widths, cancelButton: _localizationService.GetString("Svg.Editor.Global.Cancel"), cancellable: true, selectedIndex: strokeWidthSelected);
            if (width == null)
                return null;
            return new StrokeStyleTool.StrokeStyleOptions() { StrokeDashIndex = Array.IndexOf(dashes, dash), StrokeWidthIndex = Array.IndexOf(widths, width) };
        }
    }
}
