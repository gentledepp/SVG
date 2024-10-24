using Avalonia.Media;

namespace Svg.Editor.Avalon.Forms.Dialog.Views;

public class ColorPickerDialogViewModel : ContentDialogResultViewModelBase<Color>
{

    private Color _color;
    public Color SelectedColor
    {
        get
        {
            return _color;
        }
        set
        {
            _color = value;
        }
    }

    public ColorPickerDialogViewModel()
    {
    }

    internal override Color GetResult()
    {
        return SelectedColor;
    }
}