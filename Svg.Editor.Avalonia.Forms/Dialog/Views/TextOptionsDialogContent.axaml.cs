using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;

namespace Svg.Editor.Avalon.Forms;

public partial class TextOptionsDialogContent : UserControl
{
    public TextOptionsDialogContent()
    {
        InitializeComponent();
    }

    private void InputField_OnAttachedToVisualTree(object sender, VisualTreeAttachmentEventArgs e)
    {
        if (sender is InputElement inputElement)
        {
            Dispatcher.UIThread.Post(() =>
            {
                inputElement.Focus(NavigationMethod.Unspecified, KeyModifiers.None);
            });
        }
    }
}