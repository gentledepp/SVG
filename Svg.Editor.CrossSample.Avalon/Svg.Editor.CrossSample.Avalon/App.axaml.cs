using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using Svg.Editor.Avalon.Forms;
using Svg.Editor.Avalon.Forms.ToolBar;

namespace Svg.Editor.CrossSample.Avalon;

public partial class App : Application
{
    public override void Initialize()
    {
        SvgEditorForms.Init(this);
        AvaloniaXamlLoader.Load(this);
    }

    public override async void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView();
        }

        Application.Current.RequestedThemeVariant = ThemeVariant.Dark;

        base.OnFrameworkInitializationCompleted();
    }
}