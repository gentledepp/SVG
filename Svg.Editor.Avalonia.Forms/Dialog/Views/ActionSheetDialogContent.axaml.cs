using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace Svg.Editor.Avalon.Forms.Dialog.Views;

public partial class ActionSheetDialogContent : UserControl
{
    public ActionSheetDialogContent()
    {
        InitializeComponent();
    }

    private ActionSheetDialogResultViewModel ResultViewModel => (ActionSheetDialogResultViewModel)DataContext!;

    #region HandleClickingItem

    /// <summary>Used when the control is not (yet) attached to a visual root.</summary>
    private static readonly Size FallbackTapSize = new(10, 10);

    private IPointer? _pressedPointer;
    private Control? _pressedItemVisual;
    private Point _pressRootPoint;
    private bool _tapCancelled;

    private void Item_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        // Reset first: a press that we do not track must not leave state from a previous
        // gesture behind, otherwise a later release could be matched against a stale press.
        _pressedPointer = null;
        _pressedItemVisual = null;
        _tapCancelled = false;

        if (!e.GetCurrentPoint(null).Properties.IsLeftButtonPressed)
            return;

        _pressedPointer = e.Pointer;
        _pressedItemVisual = sender as Control;
        // Root (TopLevel) coordinates on purpose: measuring relative to the item would be
        // useless, because while the ListBox scrolls the item travels with the finger and
        // the relative delta stays at ~zero.
        _pressRootPoint = e.GetPosition(null);
    }

    private void Item_OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_tapCancelled || _pressedPointer is null || !ReferenceEquals(e.Pointer, _pressedPointer))
            return;

        if (ExceedsTapSlop(e.GetPosition(null), e.Pointer.Type))
            _tapCancelled = true;
    }

    /// <summary>
    /// Raised when a gesture recognizer (ScrollGestureRecognizer) steals the pointer,
    /// i.e. the user started scrolling rather than tapping.
    /// </summary>
    private void Item_OnPointerCaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        if (_pressedPointer is not null && ReferenceEquals(e.Pointer, _pressedPointer))
            _tapCancelled = true;
    }

    private void Item_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        var cancelled = _tapCancelled;
        var pressedPointer = _pressedPointer;
        var pressedItemVisual = _pressedItemVisual;

        _tapCancelled = false;
        _pressedPointer = null;
        _pressedItemVisual = null;

        if (cancelled
            || pressedPointer is null
            || !ReferenceEquals(e.Pointer, pressedPointer)
            || !ReferenceEquals(sender, pressedItemVisual)
            || e.InitialPressMouseButton != MouseButton.Left)
            return;

        if (ExceedsTapSlop(e.GetPosition(null), e.Pointer.Type))
            return;

        if (sender is not Control { DataContext: ActionSheetItem item })
            return;

        // Resolve the tapped item ourselves instead of relying on ListBox.SelectedItem:
        // for touch and pen the ListBox only updates the selection *later* in the same
        // bubbling route (it selects on PointerReleased, not PointerPressed), so reading
        // SelectedItem here could yield the previously selected item.
        ResultViewModel.SelectedItem = item;
        ResultViewModel.AcceptPrimary();
    }

    private bool ExceedsTapSlop(Point currentRootPoint, PointerType pointerType)
    {
        // TopLevel.PlatformSettings is private in Avalonia 12; VisualExtensions is the public way in.
        var slop = this.GetPlatformSettings()?.GetTapSize(pointerType) ?? FallbackTapSize;

        return Math.Abs(currentRootPoint.X - _pressRootPoint.X) > slop.Width
            || Math.Abs(currentRootPoint.Y - _pressRootPoint.Y) > slop.Height;
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