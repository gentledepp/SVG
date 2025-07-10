using Acr.UserDialogs;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Plugin.CurrentActivity;
using Svg.Editor.Forms;

namespace Svg.Editor.Sample.Forms.Droid
{
    [Activity(Label = "Svg.Editor.Sample.Forms", Icon = "@drawable/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            Xamarin.Forms.Forms.Init(this, bundle);
            UserDialogs.Init(() => (Activity) this);
            SvgEditorForms.Init();
	        CrossCurrentActivity.Current.Init(this, bundle);

            LoadApplication(new App());
        }

	    public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Android.Content.PM.Permission[] grantResults)
	    {
		    Plugin.Permissions.PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);
	    }
    }
}

