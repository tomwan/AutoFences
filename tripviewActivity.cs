using System.Text;
using System;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Android.Locations;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Mojio;
using Mojio.Client;
using Mojio.Events;

namespace AutoFences
{
    [Activity (Label = "Trip Status")]            
    public class tripviewActivity : Activity, ILocationListener
    {
        private GoogleMap _map;
        private MapFragment _mapFragment;
        private LocationManager locMgr;
        private LatLng endmarkerLocation; 
        private LatLng startmarkerLocation;


        protected override void OnCreate (Bundle bundle){
            base.OnCreate (bundle);
            SetContentView (Resource.Layout.tripStatus);

            TextView end = FindViewById<TextView> (Resource.Id.endTime);
            TextView speed = FindViewById<TextView> (Resource.Id.maxSpeed);
            TextView fuelEff = FindViewById<TextView> (Resource.Id.FuelEff);
            TextView fuelLev = FindViewById<TextView> (Resource.Id.FuelLevel);

            // Get bundle from NavigationDrawerActivity
            Bundle extras = Intent.GetBundleExtra ("extras");
            String endlat = extras.GetString ("endlat") ?? "Latitude not available";     
            String endlng = extras.GetString ("endlng") ?? "Longitude not available";
            String startlat = extras.GetString ("startlat") ?? "Latitude not available";     
            String startlng = extras.GetString ("startlng") ?? "Longitude not available";
            String fuelEfficiency = extras.GetString ("fuelEfficiency") ?? "Fuel Efficiency not available";     
            String fuelLevel = extras.GetString ("fuelLevel") ?? "Fuel Level not available";
            String startTime = extras.GetString ("startTime") ?? "Start time not available";
            String startDate = extras.GetString ("startDate") ?? "Start date not available";
            String maxSpeed = extras.GetString ("maxSpeed") ?? "Max speed not available";
            String endDateTime = extras.GetString("endTime") ?? "Time Not Available";

            end.Text = endDateTime;
            speed.Text = "Maximum Speed: " + maxSpeed;
            fuelEff.Text = "Fuel Efficiency: " + fuelEfficiency +" L/100km";
            fuelLev.Text = "Fuel Level: %" + fuelLevel;

            endmarkerLocation = new LatLng (Convert.ToDouble(endlat), Convert.ToDouble(endlng));
            startmarkerLocation = new LatLng (Convert.ToDouble(startlat), Convert.ToDouble(startlng));
            InitMapFragment ();

        }

        protected override void OnResume() {
            base.OnResume();
            SetMarker ();
        }

        private void InitMapFragment()
        {
            _mapFragment = FragmentManager.FindFragmentByTag("map") as MapFragment;
            if (_mapFragment == null)
            {
                GoogleMapOptions mapOptions = new GoogleMapOptions()
                    .InvokeMapType(GoogleMap.MapTypeNormal)
                    .InvokeZoomControlsEnabled(false)
                    .InvokeCompassEnabled(true);

                FragmentTransaction fragTx = FragmentManager.BeginTransaction();
                _mapFragment = MapFragment.NewInstance(mapOptions);
                fragTx.Add(Resource.Id.map, _mapFragment, "map");
                fragTx.Commit();

            }
        }

        private void SetMarker()
        {
            if (_map == null)
            {
                _map = _mapFragment.Map;
            }
            if (_map != null) {
                MarkerOptions marker1 = new MarkerOptions();
                marker1.SetPosition(startmarkerLocation);
                marker1.SetTitle("Start Location");
                _map.AddMarker(marker1);

                MarkerOptions marker2 = new MarkerOptions();
                marker2.SetPosition(endmarkerLocation);
                marker2.SetTitle("End Location");
                _map.AddMarker(marker2);
                // We create an instance of CameraUpdate, and move the map to it.
                CameraUpdate cameraUpdate = CameraUpdateFactory.NewLatLngZoom(startmarkerLocation, 15);
                _map.MoveCamera(cameraUpdate);

            }
        }
        public void OnProviderEnabled (string provider)
        {
            //
        }

        public void OnProviderDisabled (string provider)
        {

        }
        public void OnStatusChanged (string provider, Availability status, Bundle extras)
        {

        }
        public void OnLocationChanged (Android.Locations.Location location)
        {
        }

    }
}