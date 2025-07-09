using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;

namespace Svg.Editor.Avalon.Forms.Dialog.Views;

public partial class ActionSheetDialogContent : UserControl
{
    public ActionSheetDialogContent()
    {
        InitializeComponent();
    }

    private ActionSheetDialogResultViewModel ResultViewModel => (ActionSheetDialogResultViewModel)DataContext!;

    private void SelectingItemsControl_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (ResultViewModel is null)
            throw new InvalidOperationException("DataContext was null");

        ResultViewModel.AcceptPrimary();
    }

    #region HandleClickingItem

    private object? _previouslyClickedOnItem = null;

    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        _previouslyClickedOnItem = sender;
    }

    private void InputElement_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (sender == _previouslyClickedOnItem)
        {
            ResultViewModel.AcceptPrimary();
            _previouslyClickedOnItem = null;
        }
    }

    #endregion

    #region HandlePressingEnterr on item

    private void InputElement_OnKeyUp(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            ResultViewModel.AcceptPrimary();
        }
    }

    #endregion

    #region Handle keyboard navigation by focusing the listbox

    private void Visual_OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        // We will set the focus into our listbox just after it got attached to the visual tree.
        if (sender is ListBox listBox)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                listBox.Focus(NavigationMethod.Pointer, KeyModifiers.None);

            });
        }
    }

    #endregion

    private void InputElement_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            ResultViewModel.AcceptPrimary();
        if (e.Key == Key.Escape)
            ResultViewModel.Cancel();
    }
}