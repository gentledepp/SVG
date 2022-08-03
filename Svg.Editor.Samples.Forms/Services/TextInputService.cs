using Svg.Editor.Interfaces;
using Svg.Editor.Tools;

namespace Svg.Editor.Samples.Forms.Services
{
    public class TextInputService : ITextInputService
    {
        private readonly ContentPage _page;
        public TextInputService(ContentPage contentPage)
        {
            _page = contentPage;
        }
        public async Task<TextTool.TextProperties> GetUserInput(string title, string textValue = "",
            IEnumerable<string> textSizeOptions = null, int textSizeSelected = 0, int maxTextLength = -1)
        {
            var result = await _page.DisplayPromptAsync("Text edit", title, "Ok", "Cancel", textValue ?? "Enter text...");
            if(maxTextLength != -1)
			{
                while(result.Length > 2)
				{
                    result = await _page.DisplayPromptAsync("Text edit", title, "Ok", "Cancel", textValue ?? "Enter text...");
                }
			}
            var defaultResult = new TextTool.TextProperties
            {
                FontSizeIndex = textSizeSelected,
                LineHeight = 12f,
                Text = textValue
            };

            var text = result;
            if (text == "Cancel")
            {
                return defaultResult;
            }

            int sizeIndex = textSizeSelected;
            if (textSizeOptions != null)
            {
                var sizeResult = await _page.DisplayActionSheet("Font size", "Cancel", null, textSizeOptions.ToArray());

                if (sizeResult == "Cancel")
                {
                    return defaultResult;
                }


                sizeIndex = textSizeOptions.ToList().IndexOf(sizeResult);
                sizeIndex = sizeIndex >= 0 ? sizeIndex : textSizeSelected;
            }

            return new TextTool.TextProperties {FontSizeIndex = sizeIndex, LineHeight = 12f, Text = text};
        }
    }
}
