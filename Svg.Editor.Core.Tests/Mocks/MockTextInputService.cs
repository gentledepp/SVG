using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Svg.Editor.Interfaces;
using Svg.Editor.Tools;

namespace Svg.Editor.Core.Test.Mocks
{
    internal class MockTextInputService : ITextInputService
    {
        public Func<string, string, TextTool.TextProperties> F { get; set; } =
            (x, y) => new TextTool.TextProperties() { Text = y, FontSizeIndex = 0 };

        public Task<TextTool.TextProperties> GetUserInput(string title, string textValue, IEnumerable<string> textSizeOptions, int textSizeSelectedint, int maxTextLength)
        {
            return Task.FromResult(F(title, textValue));
        }
    }
}
