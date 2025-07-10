using System.Collections.Generic;
using System.Threading.Tasks;
using Svg.Editor.Tools;

namespace Svg.Editor.Interfaces
{
    public interface ITextInputService
    {
        Task<TextTool.TextProperties> GetUserInput(
            string title,
            string textValue = "",
            IEnumerable<string> textSizeOptions = null,
            int textSizeSelected = 0,
            int maxTextLength = -1);
    }
}