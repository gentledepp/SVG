using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Labs.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using Svg.Editor.Avalon.Forms.Dialog.Views;

namespace Svg.Editor.Avalon.Forms.Dialog;

public class UserInteractionService : IUserInteraction
{
    public async Task<bool> ConfirmAsync(string message, string? title = null, string okButton = "OK", string cancelButton = "Cancel", bool cancellable = false)
    {
        return await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var tb = new TextBlock
            {
                Text = message,
                Focusable = true,
                IsTabStop = true
            };
            var d = new ContentDialog()
            {
                Title = title,
                // Can be anything though (It is a simple content control)
                Content = tb,
                IsSecondaryButtonEnabled = false,
                PrimaryButtonText = okButton,
                CloseButtonText = cancellable ? cancelButton : null,
            };
            d.AttachKeyboardControl(cancellable);

            var r = await d.ShowAsync();
            return r == ContentDialogResult.Primary;
        });
    }
    public Task AlertAsync(string message, string? title = null, string okButton = "OK")
    {
        return Dispatcher.UIThread.InvokeAsync(() =>
        {
            var tb = new TextBlock
            {
                Text = message,
                Focusable = true,
                IsTabStop = true
            };
            var d = new ContentDialog()
            {
                Title = title,
                Content = tb,
                IsPrimaryButtonEnabled = false,
                IsSecondaryButtonEnabled = false,
                CloseButtonText = okButton,
            };
            d.AttachKeyboardControl(true);
            return d.ShowAsync();
        });
    }
    public async Task<ConfirmThreeButtonsResponse> ConfirmThreeButtonsAsync(string message, string? title = null, string positive = "Yes", string negative = "No",
        string neutral = "Maybe", bool cancellable = true)
    {
        return await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var tb = new TextBlock
            {
                Text = message,
                Focusable = true,
                IsTabStop = true
            };
            var d = new ContentDialog()
            {
                Title = title,
                Content = tb,
                IsSecondaryButtonEnabled = true,
                PrimaryButtonText = positive,
                SecondaryButtonText = negative,
                CloseButtonText = cancellable ? neutral : null,
            };
            d.AttachKeyboardControl(cancellable);
            var r = await d.ShowAsync();
            return r switch
            {
                ContentDialogResult.None => ConfirmThreeButtonsResponse.Neutral,
                ContentDialogResult.Primary => ConfirmThreeButtonsResponse.Positive,
                ContentDialogResult.Secondary => ConfirmThreeButtonsResponse.Negative,
                _ => throw new ArgumentOutOfRangeException()
            };
        });
    }
    public Task<string?> PromptAsync(string message, string? title = null, string okButton = "OK", string cancelButton = "Cancel", bool cancellable = true)
    {
        return Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var d = new ContentDialog()
            {
                Title = title,
                // Can be anything though (It is a simple content control)
                Content = message,
                IsSecondaryButtonEnabled = false,
                PrimaryButtonText = okButton,
                CloseButtonText = cancellable ? cancelButton : null,
            };
            var vm = new InputDialogResultViewModel();
            vm.CanCancel = cancellable;
            vm.Initialize(d);

            d.Content = new InputDialogContent { DataContext = vm };

            var r = await d.ShowAsync();

            if (r == ContentDialogResult.Primary)
                return vm.GetResult();
            return default(string);
        });
    }
    public Task<InputResponse> InputAsync(string message, string? title = null, string okButton = "OK",
        string cancelButton = "Cancel", string? initialText = null, string? placeholder = null, bool cancellable = true)
    {
        return Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var d = new ContentDialog()
            {
                Title = title,
                // Can be anything though (It is a simple content control)
                Content = message,
                IsSecondaryButtonEnabled = false,
                PrimaryButtonText = okButton,
                CloseButtonText = cancellable ? cancelButton : null,
            };
            var vm = new InputDialogResultViewModel();
            vm.UserInput = initialText;
            vm.WatermarkText = placeholder;
            vm.CanCancel = cancellable;
            vm.Initialize(d);

            d.Content = new InputDialogContent { DataContext = vm };

            var r = await d.ShowAsync();

            if (r == ContentDialogResult.Primary)
                return new InputResponse(true, vm.GetResult());
            return new InputResponse(false, null);
        });
    }

    public Task<Color> ColorPickerAsync(string? title = null, string okButton = "OK",
        string cancelButton = "Cancel", string? initialText = null, string? placeholder = null, bool cancellable = true)
    {
        return Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var d = new ContentDialog()
            {
                Title = title,
                // Can be anything though (It is a simple content control)
                IsSecondaryButtonEnabled = false,
                PrimaryButtonText = okButton,
                CloseButtonText = cancellable ? cancelButton : null,
            };
            var vm = new ColorPickerDialogViewModel();

            vm.Initialize(d);

            d.Content = new ColorPickerDialog() { DataContext = vm };

            var r = await d.ShowAsync();

            if (r == ContentDialogResult.Primary)
                return vm.GetResult();
            return Colors.Black;
        });
    }

    public Task<string> ActionSheetAsync(string message, IEnumerable<string> options, string? title = null, string cancelButton = "Cancel", CancellationToken? cancelToken = null, bool cancellable = false)
    {
        return Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var d = new ContentDialog()
            {
                Title = title,
                // Can be anything though (It is a simple content control)
                Content = message,
                IsSecondaryButtonEnabled = false,
                IsPrimaryButtonEnabled = false,
                PrimaryButtonText = "",
                CloseButtonText = cancellable ? cancelButton : null,
            };
            var vm = new ActionSheetDialogResultViewModel(options);
            vm.Initialize(d);
            vm.CanCancel = cancellable;
            d.Content = new ActionSheetDialogContent() { DataContext = vm };

            if (cancelToken.HasValue)
            {
                if (cancelToken.Value.IsCancellationRequested)
                    return default(string);
                cancelToken.Value.Register(() => Dispatcher.UIThread.Invoke(() => vm.Cancel()));
            }

            var r = await d.ShowAsync();

            if (r == ContentDialogResult.Primary)
                return (string)vm.GetResult();
            return default(string);
        });
    }


    #region Progressdialogs

    private ContentDialog? _currentDialog = null;

    public void Dismiss()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            if (_currentDialog is not null)
                _currentDialog.Hide();
        });
    }


    #endregion

    /* Ignored deliberately by Alex: Do we really need these "input dialogs" anymore now that Avalonia Date/Time picker _work_?
    public Task<DateTime?> PromptDateAsync(DateTime? selectedDate)
    {
        throw new NotImplementedException();
    }

    public Task<TimeSpan?> PromptTimeAsync(TimeSpan? selectedTime)
    {
        throw new NotImplementedException();
    }*/

}

internal static class ContentDialogExtensions
{
    internal static void AttachKeyboardControl(this ContentDialog dialog, bool cancelable, Key primary = Key.Enter, Key secondary = Key.None, Key close = Key.Escape)
    {
        if (dialog == null) throw new ArgumentNullException(nameof(dialog));

        if (!(dialog.Content is InputElement d))
            throw new InvalidOperationException("Content must be InputElement");

        d.Focusable = true;
        d.IsTabStop = true;

        d.KeyDown += OnKeyDown;
        d.AttachedToVisualTree += OnAttachedToVisualTree;
        dialog.Closed += OnDialogClosed;
        dialog.Closing += OnDialogClosing;


        void OnDialogClosing(object? sender, ContentDialogClosingEventArgs e)
        {
            if (e.Result == ContentDialogResult.None && !cancelable)
                e.Cancel = true;
        }
        void OnKeyDown(object? _, KeyEventArgs args)
        {
            if (args.Key == primary) dialog.Hide(ContentDialogResult.Primary);
            if (args.Key == secondary) dialog.Hide(ContentDialogResult.Secondary);
            if (args.Key == close) dialog.Hide();
        }
        void OnAttachedToVisualTree(object? element, VisualTreeAttachmentEventArgs args)
        {
            if (element is InputElement i) i.Focus();
        }
        void OnDialogClosed(object? sender, ContentDialogClosedEventArgs e)
        {
            d.KeyDown -= OnKeyDown;
            d.AttachedToVisualTree -= OnAttachedToVisualTree;
            dialog.Closing -= OnDialogClosing;
            dialog.Closed -= OnDialogClosed;
        }

    }
}