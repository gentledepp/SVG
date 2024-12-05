using Avalonia.Controls;
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using Svg.Editor.Sample.Avalon.Dialog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Svg.Editor.Sample.Avalon.Services
{
    public class ColorInputService : Tools.IColorInputService
    {
        public async Task<int> GetIndexFromUserInput(string title, string[] items, string[] colors, int defaultIndex = 0)
        {
            var result = await ActionSheetDialog.ShowActionSheet(new Window() ,title, "cancel", items);

            if (result == null)
                return -1;

            var index = items.ToList().IndexOf(result);
            if (index < 0)
                return defaultIndex;

            return index;
        }
    }
}
