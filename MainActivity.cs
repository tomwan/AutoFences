using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

namespace AutoFences
{
    [Activity (Label = "AutoFences", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        protected override void OnCreate (Bundle bundle)
        {
            base.OnCreate (bundle); // Always the superclass.

            SetContentView (Resource.Layout.Main);

        }

        //        protected override void OnResume ()
        //        {
        //            base.OnResume (); //Always call the superclass
        //
        //        }
        //
        //        protected override void OnPause ()
        //        {
        //            base.OnPause (); //Always call the superclass
        //        }
    }
}


