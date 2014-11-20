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

			Guid appID = new Guid (""); // Insert public key
			Guid secretKey = new Guid("");// Insert Secret Key

			MojioClient client = new MojioClient(MojioClient.Live); //Despite the naming, Live and Sandbox are both MojioClient.Live; Horrible naming is horrible.

			try {
				await client.BeginAsync (appID, secretKey);
			} catch (UnauthorizedAccessException uae) {
				Toast.MakeText (this, uae.Message, ToastLength.Short).Show (); 
			}
			Toast.MakeText (this, "Connection OK.", ToastLength.Short).Show ();

			EditText email = FindViewById<EditText> (Resource.Id.email);
			EditText password = FindViewById<EditText> (Resource.Id.password);
			Button logInButton = FindViewById<Button> (Resource.Id.loginbutton);

			logInButton.Click += async (o, e) => {
				try {
					await client.SetUserAsync(email.Text, password.Text);// Logs the user in.
				} catch (Exception exception) {
					Toast.MakeText (this, exception.Message, ToastLength.Short).Show (); 
				}

				if(client.IsLoggedIn()) {
					Toast.MakeText (this, "Log in successful.", ToastLength.Short).Show ();
				} else {
					Toast.MakeText (this, "The credentials provided are invalid.", ToastLength.Short).Show ();
				}


				//Temporary Prototype 1 Code; Move elsewhere after. 
				//Currently shows the list of end times from oldest to newest.
				client.PageSize = 15; //Gets 15 results
				MojioResponse<Results<Trip>> response = await client.GetAsync<Trip>();
				Results<Trip> result = response.Data;

				// Iterate over each trip
				foreach( Trip trip in result.Data )
				{
					Toast.MakeText (this, trip.EndTime.ToString(), ToastLength.Short).Show ();
				}
			};
        }
	}
}