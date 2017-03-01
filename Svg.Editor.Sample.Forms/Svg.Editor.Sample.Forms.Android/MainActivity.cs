﻿using Android.App;
using Android.Content.PM;
using Android.OS;

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

            Editor.Init();

            LoadApplication(new App());
        }
    }
}

