using Android.App;
using Android.Content.PM;
using Android.OS;
using Plugin.CurrentActivity;
using Svg.Editor.Forms;

namespace Svg.Editor.Sample.App
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            SvgEditorForms.Init();
            CrossCurrentActivity.Current.Init(this, savedInstanceState);
            base.OnCreate(savedInstanceState);

        }
    }
}