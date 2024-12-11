
using Svg.Editor.Avalon.Forms;
using System;

namespace Svg.Editor.CrossSample.Avalon
{
    public class SvgEditorView : SvgCanvasEditorView
    {
        public SvgEditorView()
        {
            if (OperatingSystem.IsWindows())
            {
                Console.WriteLine("IsWindowstrue");
            }
        }
    }
}
