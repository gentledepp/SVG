using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Svg.Editor.Avalonia.Forms;

namespace Svg.Editor.Sample.Avalon;

public partial class App : Application
{
    public override void Initialize()
    {
        SvgEditorForms.Init();
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow()
            {
                DataContext = new MainViewModel()
            };
        }


        base.OnFrameworkInitializationCompleted();
    }

}