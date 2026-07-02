using System;
using System.Collections.Generic;
using System.Text;

namespace Svg.Editor.Avalon.Forms.Dialog.Views;

public record TextOptionsResult(string? Text, string? SelectedOption, int SelectedIndex);

public class TextOptionsDialogResultViewModel : ContentDialogResultViewModelBase<TextOptionsResult>
{
    private string? _userInput;
    public string? UserInput
    {
        get => _userInput;
        set => SetField(ref _userInput, value);
    }

    private string? _watermarkText;
    public string? WatermarkText
    {
        get => _watermarkText;
        set => SetField(ref _watermarkText, value);
    }

    private IReadOnlyList<string> _options = Array.Empty<string>();
    public IReadOnlyList<string> Options
    {
        get => _options;
        set => SetField(ref _options, value);
    }

    private int _selectedIndex;
    public int SelectedIndex
    {
        get => _selectedIndex;
        set => SetField(ref _selectedIndex, value);
    }

    public override TextOptionsResult? GetResult()
    {
        var selected = SelectedIndex >= 0 && SelectedIndex < Options.Count
            ? Options[SelectedIndex]
            : null;

        return new TextOptionsResult(UserInput, selected, SelectedIndex);
    }
}
