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
using Mojio.Client;
using Mojio;

namespace AutoFences
{
	[Activity (Label = "DisplayActivity")]			
	public class DisplayActivity : Activity
	{
		protected async override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			SetContentView (Resource.Layout.Display);

			MojioClient client = Globals.client;

			client.PageSize = 15; //Gets 15 results
			MojioResponse<Results<Trip>> response = await client.GetAsync<Trip>();
			Results<Trip> result = response.Data;

			var results = FindViewById<TextView> (Resource.Id.tripResults);
			int tripIndex = 1;
			String outputString = "";

			// Iterate over each trip
			foreach( Trip trip in result.Data ) {
				outputString += string.Format ("Trip {0}:", tripIndex) + System.Environment.NewLine + "Start time: " + trip.StartTime.ToString() 
					+ System.Environment.NewLine + "End time: " + trip.EndTime.ToString() + System.Environment.NewLine + "Longitude: " 
					+ trip.EndLocation.Lng.ToString() + System.Environment.NewLine + "Latitude: " + trip.EndLocation.Lat.ToString() 
					+ System.Environment.NewLine + "Max Speed: " + trip.MaxSpeed.Value.ToString() + " km/h"
					+ System.Environment.NewLine + System.Environment.NewLine;
				tripIndex++;
			}
			results.Text = outputString;
		}
	}
}