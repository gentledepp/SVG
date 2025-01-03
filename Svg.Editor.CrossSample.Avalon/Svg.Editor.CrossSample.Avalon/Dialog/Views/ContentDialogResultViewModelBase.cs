namespace Svg.Editor.CrossSample.Avalon.Dialog.Views;

public abstract class ContentDialogResultViewModelBase<TResult> : ContentDialogViewModelBase
{
    internal abstract TResult GetResult();
}