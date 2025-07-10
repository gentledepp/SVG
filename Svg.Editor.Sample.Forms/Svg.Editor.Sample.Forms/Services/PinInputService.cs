using Acr.UserDialogs;
using Svg.Editor.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Svg.Editor.Sample.Forms.Services
{
	public class PinInputService : IPinInputService
    {
        public async Task<PinTool.PinSize> GetUserInput(IEnumerable<string> pinSizeOptions, int oldSizeIndex = 1)
        {
            var defaultResult = (PinTool.PinSize)oldSizeIndex;

            var sizeResult = await UserDialogs.Instance.ActionSheetAsync("Select pin size", "Cancel", null, null, pinSizeOptions.ToArray());

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
