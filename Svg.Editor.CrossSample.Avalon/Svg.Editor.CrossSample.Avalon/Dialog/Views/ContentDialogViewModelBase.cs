using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Labs.Controls;

namespace Svg.Editor.CrossSample.Avalon.Dialog.Views;

public abstract class ContentDialogViewModelBase : INotifyPropertyChanged
{
    private Avalonia.Labs.Controls.ContentDialog? _dialog;
    private object? _content;

    public bool CanCancel { get; set; } = true;
    
    internal void Initialize(Avalonia.Labs.Controls.ContentDialog? dialog)
    { 
        if (dialog is null)
        {
            throw new ArgumentNullException(nameof(dialog));
        }

        _dialog = dialog;
        Content = dialog.Content;
        dialog.Closing += DialogOnClosing;
        dialog.Closed += DialogOnClosed;
    }

    private void DialogOnClosing(object? sender, ContentDialogClosingEventArgs e)
    {
        if(!CanClose(e.Result))
            e.Cancel = true;
    }

    public object? Content
    {
        get => _content;
        set => SetField(ref _content, value);
    }

    private void DialogOnClosed(object? sender, ContentDialogClosedEventArgs args)
    {
        _dialog.Closed -= DialogOnClosed;
        _dialog.Closing -= DialogOnClosing;
    }

    /// <summary>
    /// Override this to guard your dialog from closing if the user still needs to do something (e.g. select an item from the actionsheet)
    /// </summary>
    /// <returns></returns>
    protected virtual bool CanClose(ContentDialogResult result)
    {
        if (result == ContentDialogResult.None && !CanCancel)
            return false;

        return true;
    }
    
    public void AcceptPrimary()
    {
        _dialog.Hide(ContentDialogResult.Primary);
    }

    public void AcceptSecondary()
    {   
        _dialog.Hide(ContentDialogResult.Secondary);
    }
    
    public void Cancel()
    {
        _dialog.Hide();
    }
    
    #region PropertyChanged by JetBrains Rider
    
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
    
    #endregion
}