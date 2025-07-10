using Acr.UserDialogs;
using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using Android.Widget;
using Plugin.CurrentActivity;
using Svg.Editor.Forms;
using Svg.Editor.Sample.Forms;
using Xamarin.Forms.Platform.Android;

namespace Sample.Forms.Droid
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);

            Xamarin.Forms.Forms.Init(this, savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            UserDialogs.Init(() => (Activity)this);
            SvgEditorForms.Init();
            CrossCurrentActivity.Current.Init(this, savedInstanceState);

            LoadApplication(new App());
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}