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
	public static class Globals	{
		public static MojioClient client = new MojioClient(MojioClient.Live);
	}

    [Activity (Label = "AutoFences", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        protected async override void OnCreate (Bundle bundle)
        {
            base.OnCreate (bundle); // Always call the superclass.

            SetContentView (Resource.Layout.Main);

			Guid appID = new Guid (Configuration.appID); 
			Guid secretKey = new Guid(Configuration.secretKey);

			try {
				await Globals.client.BeginAsync (appID, secretKey);
			} catch (UnauthorizedAccessException uae) {
				Toast.MakeText (this, uae.Message, ToastLength.Short).Show (); 
			}
			Toast.MakeText (this, "Connection OK.", ToastLength.Short).Show ();

			EditText email = FindViewById<EditText> (Resource.Id.email);
			EditText password = FindViewById<EditText> (Resource.Id.password);
			Button logInButton = FindViewById<Button> (Resource.Id.loginbutton);

			logInButton.Click += async (o, e) => {
				if (string.IsNullOrEmpty(email.Text)) {
					Toast.MakeText (this, "Please enter a valid username.", ToastLength.Short).Show (); 
				} else if (string.IsNullOrEmpty(password.Text)) {
					Toast.MakeText (this, "Please enter a valid password.", ToastLength.Short).Show (); 
				} else {
					try {
						await Globals.client.SetUserAsync(email.Text, password.Text); // Logs the user in.
					} catch (Exception exception) {
						Toast.MakeText (this, exception.Message, ToastLength.Short).Show (); 
					}

					if(Globals.client.IsLoggedIn()) {
						StartActivity(typeof(DisplayActivity));
					} else {
						Toast.MakeText (this, "The credentials provided are invalid.", ToastLength.Short).Show ();
					}
				}
			};
		}
	}
}