using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iCL.Modules.UserInteraction;

namespace Svg.Editor.CrossSample.Avalon.Services
{
    public class ColorInputService : Svg.Editor.Tools.IColorInputService
    {
        public async Task<int> GetIndexFromUserInput(string title, string[] items, string[] colors, int defaultIndex = 0)
        {
            var result = await UserInteractionServiceExt.UserInteractionInst.ActionSheetAsync(title, items);

            if (result == null)
                return -1;

            var index = items.ToList().IndexOf(result);
            if (index < 0)
                return defaultIndex;

            return index;
        }
    }
}
