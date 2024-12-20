using Avalonia.Controls;
using Svg.Editor.Sample.Avalon.Dialog;
using Svg.Editor.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Svg.Editor.Sample.Avalon.Services
{
    public class PinInputService : IPinInputService
    {
        public async Task<PinTool.PinSize> GetUserInput(IEnumerable<string> pinSizeOptions, int oldSizeIndex = 1)
        {
            var defaultResult = (PinTool.PinSize)oldSizeIndex;

            var sizeResult = await ActionSheetDialog.ShowActionSheet(StrokeStyleOptionsInputService.GetWindow(), "Select pin size", "cancel", pinSizeOptions.ToArray()); PinTool.PinSize.Medium.ToString(); 

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
