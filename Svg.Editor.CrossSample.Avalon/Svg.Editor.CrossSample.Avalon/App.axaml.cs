using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Svg.Editor.Avalon.Forms;
using Svg.Editor.Avalon.Forms.ToolBar;

namespace Svg.Editor.CrossSample.Avalon;

public partial class App : Application
{
    public override void Initialize()
    {
        SvgEditorForms.Init();
        AvaloniaXamlLoader.Load(this);
    }

    public override async void OnFrameworkInitializationCompleted()
    {
        DataTemplates.Add(new MenuItemHeaderTemplate());


        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView();
        }

        base.OnFrameworkInitializationCompleted();
    }
}