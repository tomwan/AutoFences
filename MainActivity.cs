using System;

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
        protected override void OnCreate (Bundle bundle)
        {
            base.OnCreate (bundle); // Always call the superclass.

            SetContentView (Resource.Layout.Main);

			Guid appID = new Guid (); // Insert appID and secretKey
			Guid secretKey = new Guid();

			MojioClient client = new MojioClient(MojioClient.Live);

			client.PageSize = 15;

			client.BeginAsync (appID, secretKey); // Begin Async auth

			EditText email = FindViewById<EditText> (Resource.Id.email);
			EditText password = FindViewById<EditText> (Resource.Id.password);
			Button confirmButton = FindViewById<Button> (Resource.Id.confirmButton);

			confirmButton.Click += (o, e) => {
				client.SetUserAsync(email.Text, password.Text);
			};
        }
	}
}