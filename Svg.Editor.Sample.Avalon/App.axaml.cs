using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Svg.Editor.Avalon.Forms;
using Svg.Interfaces;

namespace Svg.Editor.Sample.Avalon;

public partial class App : Application
{
    public override void Initialize()
    {
        SvgEditorForms.Init();
        SvgEngine.RegisterSingleton<IFileSystem>(() => new UwpFileSystem());
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



    public class UwpFileSystem : FileSystem
{
    public override string GetDefaultStoragePath()
    {
            string appDataPath = /*"C:\\Users\\zepr2\\AppData\\Local\\Packages\\TestSvgEditor";*/
            
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            
            
            Directory.CreateDirectory(appDataPath);
            return System.Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        }
    }

}