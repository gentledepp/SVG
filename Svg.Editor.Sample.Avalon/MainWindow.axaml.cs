using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Styling;


namespace Svg.Editor.Sample.Avalon;

public partial class MainWindow : Window
{
    private bool _isDarkMode = false;

    public MainWindow()
    {
        InitializeComponent();
    }

    public void ClickHandler(object sender, RoutedEventArgs args)
    {
        _isDarkMode = !_isDarkMode;

        var theme = _isDarkMode
            ? ThemeVariant.Dark
            : ThemeVariant.Light;

        Application.Current.RequestedThemeVariant = theme;
    }
}