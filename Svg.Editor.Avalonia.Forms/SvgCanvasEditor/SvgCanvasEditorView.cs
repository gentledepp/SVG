using Svg.Editor.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Svg.Editor.Avalon.Forms
{
    public partial class SvgCanvasEditorView : CustomControl
    {

        public ISvgDrawingCanvas DrawingCanvas
        {
            get { return DataContext as ISvgDrawingCanvas; }
            set
            {
                DataContext = value;
            }
        }
    }
}
