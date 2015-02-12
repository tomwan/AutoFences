
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace AutoFences
{
    [Activity (Label = "AutoFences", MainLauncher = true, Icon = "@drawable/ic_logo", NoHistory=true)]
    public class Splash : Activity
    {
        protected override async void OnCreate (Bundle bundle)
        {
            base.OnCreate (bundle);

            SetContentView (Resource.Layout.Splash);

            var prefs = Application.Context.GetSharedPreferences ("settings", FileCreationMode.Private);

            if (await MojioConnectionHelper.setupMojioConnection (prefs)) {
                Toast.MakeText (this, "Log In successful.", ToastLength.Short).Show ();
                StartActivity (typeof(NavigationDrawerActivity));
                Finish ();
            } else {
                StartActivity (typeof(MainActivity));
                Finish ();
            }
        }
    }
}

