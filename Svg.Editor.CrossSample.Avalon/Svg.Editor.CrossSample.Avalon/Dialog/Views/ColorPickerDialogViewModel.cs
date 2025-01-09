using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Svg.Editor.CrossSample.Avalon.Dialog.Views;

public class ColorPickerDialogViewModel : ContentDialogResultViewModelBase<Color>
{
    public Color SelectedColor
    {
        get;
        set;
    }

    public ColorPickerDialogViewModel()
    {
    }

    internal override Color GetResult()
    {
        return SelectedColor;
    }
}