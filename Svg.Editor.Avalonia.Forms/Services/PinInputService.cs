using Avalonia.Controls;
using Svg.Editor.Avalon.Forms.Dialog;
using Svg.Editor.Interfaces;
using Svg.Editor.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Svg.Editor.Avalon.Forms.Services
{
    public class PinInputService : IPinInputService
    {
        private readonly IUserInteraction _userInteractionService;


        public PinInputService()
        {
            _userInteractionService = SvgEngine.Resolve<IUserInteraction>();
        }
        public async Task<PinTool.PinSize> GetUserInput(IEnumerable<string> pinSizeOptions, int oldSizeIndex = 1)
        {
            var defaultResult = (PinTool.PinSize)oldSizeIndex;

            var sizeResult = await _userInteractionService.ActionSheetAsync("Select pin size", pinSizeOptions.ToArray());
            PinTool.PinSize.Medium.ToString();

            if (sizeResult == "Cancel")
            {
                return defaultResult;
            }

            PinTool.PinSize result;
            if (!Enum.TryParse(sizeResult, out result))
            {
                result = defaultResult;
            }

            return result;
        }
    }
}
