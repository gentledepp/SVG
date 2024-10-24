using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Svg.Editor.Avalon.Forms.Dialog;
using Svg.Editor.Interfaces;
using Svg.Editor.Tools;

namespace Svg.Editor.Avalon.Forms.Services
{
    public class TextInputService : ITextInputService
    {
        public async Task<TextTool.TextProperties> GetUserInput(string title, string textValue = "",
            IEnumerable<string> textSizeOptions = null, int textSizeSelected = 0, int maxTextLength = -1)
        {
            var result = await UserInteractionServiceExt.UserInteractionInst.InputAsync("Text edit", title, "Ok", "Cancel", textValue ?? "Enter text...");
            if (maxTextLength != -1)
            {
                while (result.Text.Length > 2)
                {
                    result = await UserInteractionServiceExt.UserInteractionInst.InputAsync("Text edit", title, "Ok", "Cancel", textValue ?? "Enter text...");
                }
            }
            var defaultResult = new TextTool.TextProperties
            {
                FontSizeIndex = textSizeSelected,
                LineHeight = 12f,
                Text = textValue
            };

            var text = result.Text;
            if (text == "Cancel")
            {
                return defaultResult;
            }

            int sizeIndex = textSizeSelected;
            if (textSizeOptions != null)
            {
                var sizeResult = await UserInteractionServiceExt.UserInteractionInst.ActionSheetAsync("Font size", textSizeOptions.ToArray());

                if (sizeResult == "Cancel")
                {
                    return defaultResult;
                }


                sizeIndex = textSizeOptions.ToList().IndexOf(sizeResult);
                sizeIndex = sizeIndex >= 0 ? sizeIndex : textSizeSelected;
            }

            return new TextTool.TextProperties
            {
                FontSizeIndex = sizeIndex,
                LineHeight = 12f,
                Text = text
            };
        }
    }
}
