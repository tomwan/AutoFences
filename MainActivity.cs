using System;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Mojio;
using Mojio.Client;


namespace AutoFences
{
    [Activity (Label = "AutoFences", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        protected async override void OnCreate (Bundle bundle)
        {
            base.OnCreate (bundle); // Always call the superclass.

            SetContentView (Resource.Layout.Main);

            // TODO: Move the following code to the splash screen. This activity should only be for login.
            var prefs = Application.Context.GetSharedPreferences ("settings", FileCreationMode.Private); // These two lines should be duplicated when moved.
            var prefEditor = prefs.Edit(); //Please tidy up comments after move.

            MojioClient client = Globals.client;

            Guid appID = new Guid (Configurations.appID);
            Guid secretKey = new Guid (Configurations.secretKey);

            try {
                await client.BeginAsync (appID, secretKey);
            } catch (UnauthorizedAccessException uae) {
                Toast.MakeText (this, uae.Message, ToastLength.Short).Show ();
            }
            Toast.MakeText (this, "Connection OK.", ToastLength.Short).Show ();

            if ((prefs.GetString ("email", null)) != null && (prefs.GetString("password",null)) != null) { //Check if credentials have already been entered. If so, log in.
                try {
                    await client.SetUserAsync (prefs.GetString ("email", null), prefs.GetString ("password", null)); // Logs the user in.
                } catch (Exception exception) {
                    Toast.MakeText (this, exception.Message, ToastLength.Short).Show ();
                }

                if (client.IsLoggedIn ()) {
                    StartActivity (typeof(NavigationDrawerActivity));
                    Finish ();
                }
            }
            // See above.

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
                        await client.SetUserAsync (email.Text, password.Text); // Logs the user in.
                    } catch (Exception exception) {
                        Toast.MakeText (this, exception.Message, ToastLength.Short).Show ();
                    }

                    if (client.IsLoggedIn ()) {
                        prefEditor.PutString("email", email.Text);
                        prefEditor.PutString("password",password.Text);
                        prefEditor.Apply();
                        StartActivity (typeof(NavigationDrawerActivity));
                        Finish();
                    } else {
                        Toast.MakeText (this, "The credentials provided are invalid.", ToastLength.Short).Show ();
                    }
                }
            };
        }
    }
}