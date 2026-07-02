using System;
using System.Collections.Generic;
using System.Text;
using Avalonia.Media;

namespace Svg.Editor.Avalon.Forms.Services
{
    public interface IColorPickerState
    {
        Color LastPickedColor { get; set; }
    }

    public class ColorPickerState : IColorPickerState
    {
        public Color LastPickedColor { get; set; } = Colors.Black;
    }

}
