using Android.App;
using Android.Content.PM;
using Avalonia.Android;

namespace Svg.Editor.CrossSample.Avalon.Android
{
    [Activity(
        Label = "Svg.Editor.CrossSample.Avalon.Android",
        Theme = "@style/MyTheme.NoActionBar",
        Icon = "@drawable/icon",
        MainLauncher = true,
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
    public class MainActivity : AvaloniaMainActivity
    {
    }
}
