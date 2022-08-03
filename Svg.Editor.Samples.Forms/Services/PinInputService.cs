using Svg.Editor.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Svg.Editor.Samples.Forms.Services
{
	public class PinInputService : IPinInputService
    {

        private readonly ContentPage _page;
        public PinInputService(ContentPage contentPage)
        {
            _page = contentPage;
        }

        public async Task<PinTool.PinSize> GetUserInput(IEnumerable<string> pinSizeOptions, int oldSizeIndex = 1)
        {
            var defaultResult = (PinTool.PinSize)oldSizeIndex;

            var sizeResult = await _page.DisplayActionSheet("Select pin size", "Cancel", null, pinSizeOptions.ToArray());

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
