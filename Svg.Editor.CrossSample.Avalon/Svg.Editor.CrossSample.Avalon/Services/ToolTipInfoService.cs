using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Labs.Controls;
using Avalonia.Threading;
using iCL.Modules.UserInteraction;
using Svg.Editor.Interfaces;

namespace Svg.Editor.CrossSample.Avalon.Services;

public class ToolTipInfoService : IToolTipInfoService
{
    private ContentDialog? _d;
    public async Task ShowToolTip(string text)
    {
        if(text == null)
            return;

        var tb = new TextBlock
        {
            Text = text,
            Focusable = true,
            IsTabStop = true
        };
        _d = new ContentDialog()
        {
            Content = tb,
            IsPrimaryButtonEnabled = false,
            IsSecondaryButtonEnabled = false,
        };
        _d.PointerPressed +=  CloseToolTip;
        await _d.ShowAsync();
    }

    private void CloseToolTip(object? sender, PointerPressedEventArgs e)
    {
        if (_d != null)
        {
            _d.Hide();
        }
    }

    public void CloseToolTip()
    {
        if (_d != null)
        {
            _d.Hide();
        }
    }
}