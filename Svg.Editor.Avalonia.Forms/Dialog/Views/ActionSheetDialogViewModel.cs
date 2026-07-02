using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Labs.Controls;

namespace Svg.Editor.Avalon.Forms.Dialog.Views;

public class ActionSheetDialogResultViewModel : ContentDialogResultViewModelBase<object?>
{
    private ActionSheetItem? _selectedItem;

    public ActionSheetItem? SelectedItem
    {
        get => _selectedItem;
        set => SetField(ref _selectedItem, value);
    }

    public ActionSheetDialogResultViewModel(IEnumerable<string> items)
    {
        Items = new ObservableCollection<ActionSheetItem>(items.Select(i => new ActionSheetItem(i, i)));
        SelectedItem = Items.FirstOrDefault();
    }

    public ActionSheetDialogResultViewModel(IEnumerable<KeyValuePair<string, object?>> items)
    {
        Items = new ObservableCollection<ActionSheetItem>(items.Select(i => new ActionSheetItem(i.Key, i.Value)));
        SelectedItem = Items.FirstOrDefault();
    }

    public ObservableCollection<ActionSheetItem> Items { get; }

    // Can only close if 
    protected override bool CanClose(ContentDialogResult result)
    {
        // closing with "ok" is only allowed if an item is selected
        if (result is ContentDialogResult.Primary)
            return SelectedItem is not null;

        return base.CanClose(result);
    }

    public override object? GetResult()
    {
        return SelectedItem?.Value;
    }
}

public class ActionSheetItem(string title, object? value)
{
    public string Title { get; } = title;
    public object? Value { get; } = value;
}