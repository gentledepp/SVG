using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Styling;
using Svg.Editor.Sample.Avalon.Services;


namespace Svg.Editor.CrossSample.Avalon;

public partial class MainWindow : Window
{

    public MainWindow()
    {
        InitializeComponent();

        StrokeStyleOptionsInputService.GetWindow = () => this;
    }

}