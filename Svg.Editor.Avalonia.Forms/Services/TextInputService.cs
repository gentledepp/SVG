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
        private readonly IUserInteraction _userInteractionService;

        public TextInputService()
        {
            _userInteractionService = SvgEngine.Resolve<IUserInteraction>();
        }
        public async Task<TextTool.TextProperties> GetUserInput(string title, string textValue = "",
            IEnumerable<string> textSizeOptions = null, int textSizeSelected = 0, int maxTextLength = -1)
        {

            var sizeOptions = textSizeOptions?.ToList() ?? new List<string>();

            var result = await _userInteractionService.InputWithOptionsAsync(
                "Text edit",
                sizeOptions,
                title,
                "Ok",
                "Cancel",
                textValue,
                placeholder: "Enter text",
                selectedIndex: textSizeSelected);

            if (!result.Ok)
            {
                return new TextTool.TextProperties
                {
                    FontSizeIndex = textSizeSelected,
                    LineHeight = 12f,
                    Text = textValue
                };
            }

            var text = result.Text ?? string.Empty;
            if (maxTextLength != -1 && text.Length > maxTextLength)
                text = text.Substring(0, maxTextLength);

            var sizeIndex = result.SelectedIndex >= 0 ? result.SelectedIndex : textSizeSelected;

            return new TextTool.TextProperties
            {
                FontSizeIndex = sizeIndex,
                LineHeight = 12f,
                Text = text
            };
        }
    }
}
