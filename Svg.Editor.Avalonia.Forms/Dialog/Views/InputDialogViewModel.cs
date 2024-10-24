namespace Svg.Editor.Avalon.Forms.Dialog.Views;

public class InputDialogResultViewModel : ContentDialogResultViewModelBase<string>
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


    internal override string? GetResult()
    {
        return UserInput;
    }
}