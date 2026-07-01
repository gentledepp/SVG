using Avalonia.Media;

namespace Svg.Editor.Avalon.Forms.Dialog.Views;

public class ColorPickerDialogViewModel : ContentDialogResultViewModelBase<Color>
{

    private Color _color;
    public Color SelectedColor
    {
        get => _color;
        set => _color = value;
    }

    public ColorPickerDialogViewModel()
    {
    }

    public override Color GetResult()
    {
        return SelectedColor;
    }
}