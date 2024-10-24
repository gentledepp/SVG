using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Styling;
using Svg.Editor.Avalon.Forms.Services;
using Svg.Interfaces;


namespace Svg.Editor.CrossSample.Avalon;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        FormsPickImageService.MainWindow = this;
        SvgEngine.RegisterSingleton<IFileSystem>(() => new UwpFileSystem());

    }
}
public class UwpFileSystem : FileSystem
{
    public override string GetDefaultStoragePath()
    {
        return "./";
    }
}