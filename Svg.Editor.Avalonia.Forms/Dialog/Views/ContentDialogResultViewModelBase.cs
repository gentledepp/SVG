namespace Svg.Editor.Avalon.Forms.Dialog.Views;

public abstract class ContentDialogResultViewModelBase<TResult> : ContentDialogViewModelBase
{
    public abstract TResult GetResult();
}