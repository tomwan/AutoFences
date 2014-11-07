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

			Guid appID = new Guid("a2394021-c43c-4991-b0e7-04b8cf0107cd");
			Guid secretKey = new Guid("66c05a5b-32c5-4cd2-a9ac-b29e996b06c1");

			MojioClient client = new MojioClient(
				appID, 
				secretKey,
				MojioClient.Sandbox
			);

			client.PageSize = 15;

			EditText email = FindViewById<EditText> (Resource.Id.email);
			EditText password = FindViewById<EditText> (Resource.Id.password);
			Button confirmButton = FindViewById<Button> (Resource.Id.confirmButton);

			confirmButton.Click += (o, e) => {
				client.SetUser(email.Text, password.Text);
			};



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