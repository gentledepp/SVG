namespace Svg.Editor.Avalon.Forms.Dialog.Views;

public abstract class ContentDialogResultViewModelBase<TResult> : ContentDialogViewModelBase
{
    internal abstract TResult GetResult();
}