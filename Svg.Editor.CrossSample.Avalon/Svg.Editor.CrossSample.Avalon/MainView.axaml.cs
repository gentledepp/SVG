using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace Svg.Editor.CrossSample.Avalon;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
    }

    private bool _isDarkMode = false;

    public void ClickHandler(object sender, RoutedEventArgs args)
    {
        _isDarkMode = !_isDarkMode;

        var theme = _isDarkMode
            ? ThemeVariant.Dark
            : ThemeVariant.Light;

        Application.Current.RequestedThemeVariant = theme;
    }
}