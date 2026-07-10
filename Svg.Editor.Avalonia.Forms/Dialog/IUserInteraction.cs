using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media;

namespace Svg.Editor.Avalon.Forms.Dialog;

public record InputWithOptionsResponse(bool Ok, string? Text, string? SelectedOption, int SelectedIndex);

public interface IUserInteraction
{
    Task<bool> ConfirmAsync(string message, string? title = null, string okButton = "OK", string cancelButton = "Cancel", bool cancellable = false);

    Task AlertAsync(string message, string? title = null, string okButton = "OK");

    public Task<Color> ColorPickerAsync(string? title = null, string okButton = "OK",
        string cancelButton = "Cancel", string? initialText = null, string? placeholder = null,
        bool cancellable = true);
    Task<string?> PromptAsync(string message, string? title = null, string okButton = "OK", string cancelButton = "Cancel", bool cancellable = true);

    Task<InputResponse> InputAsync(
        string message,
        string? title = null,
        string okButton = "OK",
        string cancelButton = "Cancel",
        string? initialText = null,
        string? placeholder = null,
        bool cancellable = true);

    Task<ConfirmThreeButtonsResponse> ConfirmThreeButtonsAsync(
        string message,
        string? title = null,
        string positive = "Yes",
        string negative = "No",
        string neutral = "Cancel",
        bool cancellable = true);

    Task<string> ActionSheetAsync(string message, IEnumerable<string> options, string? title = null, string cancelButton = "Cancel", CancellationToken? cancelToken = null, bool cancellable = false, int selectedIndex = -1);

    /* Ignored deliberately by Alex: Do we really need these "input dialogs" anymore now that Avalonia Date/Time picker _work_?
    Task<DateTime?> PromptDateAsync(DateTime? selectedDate);

    Task<TimeSpan?> PromptTimeAsync(TimeSpan? selectedTime);
    */
   
    Task<InputWithOptionsResponse> InputWithOptionsAsync(
    string message,
    IEnumerable<string> options,
    string? title = null,
    string okButton = "OK",
    string cancelButton = "Cancel",
    string? initialText = null,
    string? placeholder = null,
    int selectedIndex = 0,
    bool cancellable = true);

    void Dismiss();
}