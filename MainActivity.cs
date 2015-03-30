using System;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

namespace AutoFences
{
    [Activity (Label = "AutoFences", Icon = "@drawable/ic_logo", NoHistory=true)]
    public class MainActivity : Activity
    {
        protected async override void OnCreate (Bundle bundle)
        {
            base.OnCreate (bundle); // Always call the superclass.

            SetContentView (Resource.Layout.Main);

            var prefs = Application.Context.GetSharedPreferences ("settings", FileCreationMode.Private);
            var prefEditor = prefs.Edit();

            EditText email = FindViewById<EditText> (Resource.Id.email);
            EditText password = FindViewById<EditText> (Resource.Id.password);
            Button logInButton = FindViewById<Button> (Resource.Id.loginbutton);

            logInButton.Click += async (o, e) => {
                if (string.IsNullOrEmpty (email.Text)) {
                    Toast.MakeText (this, "Please enter a valid username or email.", ToastLength.Short).Show ();
                } else if (string.IsNullOrEmpty (password.Text)) {
                    Toast.MakeText (this, "Please enter a valid password.", ToastLength.Short).Show ();
                } else {
                    try {
                        if (await AFLib.MojioConnectionHelper.setupMojioConnectionFirstTime (email.Text, password.Text, prefEditor)) {
                            StartService(new Intent(this, typeof(AutoFencesService)));
                            StartActivity (typeof(NavigationDrawerActivity));
                            Finish ();
                        } else {
                            Toast.MakeText (this, "The credentials provided are invalid.", ToastLength.Short).Show ();
                        }
                    } catch (Exception exception) {
                        Toast.MakeText (this, exception.Message, ToastLength.Short).Show ();
                    }
                }
            };
        }
    }
}