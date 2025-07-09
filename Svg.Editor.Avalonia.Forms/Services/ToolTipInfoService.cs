using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Labs.Controls;
using Avalonia.Threading;
using Svg.Editor.Interfaces;

namespace Svg.Editor.Avalon.Forms.Services;

public class ToolTipInfoService : IToolTipInfoService
{
    private ContentDialog? _d;
    public async Task ShowToolTip(string text)
    {
        if(_d != null)
            return;
        
        if (text == null)
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
        CloseToolTip();
    }

    public void CloseToolTip()
    {
        if (_d != null)
        {
            _d.Hide();
            _d = null;
        }
    }
}