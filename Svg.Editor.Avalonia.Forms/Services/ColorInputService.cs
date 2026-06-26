using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Svg.Editor.Avalon.Forms.Dialog;

namespace Svg.Editor.Avalon.Forms.Services
{
    public class ColorInputService : Editor.Tools.IColorInputService
    {
        private readonly IUserInteraction _userInteractionService;

        public ColorInputService()
        {
            _userInteractionService = SvgEngine.Resolve<IUserInteraction>();
        }
        public async Task<string> GetHexaColorFromUserInput(string title)
        {
            var result = await _userInteractionService.ColorPickerAsync(title);

            if (result == null)
                return "#000000";

            var argb = result.ToUInt32();
            var hex = $"#{argb.ToString("x8", CultureInfo.InvariantCulture)}";
            return hex;
        }
    }
}
