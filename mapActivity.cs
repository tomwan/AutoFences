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

namespace AutoFences
{
    [Activity (Label = "Set Map")]            
    public class mapActivity : Activity, ILocationListener
    {
        private GoogleMap _map;
        private int radius;
        private MapFragment _mapFragment;
        private LocationManager locMgr;
        private LatLng currentLocation;
        private LatLng newLocation;
        SeekBar seekBar;
        private Circle _circle;

        protected override void OnCreate (Bundle bundle){
            base.OnCreate (bundle);
            SetContentView (Resource.Layout.mapLayout);           

            seekBar = FindViewById<SeekBar>(Resource.Id.seekBarRadius);
            Button confirmRadius = FindViewById<Button> (Resource.Id.setRadius);

            newLocation = GetCurrentLocation ();
            InitMapFragment();
           
            confirmRadius.Click += delegate {
                // Radius has been set, save to user prefs and use for fenceing.
                Console.WriteLine ("Location {0}, Radius is: {1}", newLocation, radius);
            };
        }

        protected override void OnResume()
        {
            base.OnResume();
            SetupMapIfNeeded();
            seekBar.ProgressChanged += new EventHandler<SeekBar.ProgressChangedEventArgs>(seekBarProgressChanged);
            _map.MapClick += new EventHandler<GoogleMap.MapClickEventArgs> (updatemap);
        }

        void updatemap(object sender, GoogleMap.MapClickEventArgs e) {
            newLocation = e.Point;
            updateMap ();

        }

        void seekBarProgressChanged(object sender, SeekBar.ProgressChangedEventArgs e) {
            radius = e.Progress;
            updateMap ();
        }

        void updateMap () {
            _map.Clear ();
            MarkerOptions marker1 = new MarkerOptions();
            marker1.SetPosition(newLocation);
            _map.AddMarker(marker1);
        
            CircleOptions circleOptions = new CircleOptions ();
            circleOptions.InvokeCenter (newLocation);
            circleOptions.InvokeRadius (radius);
            circleOptions.InvokeFillColor (Convert.ToInt32 ("0x3000ffff", 16));
            circleOptions.InvokeStrokeColor (Convert.ToInt32 ("0x3000ffff", 16));
            _circle = _map.AddCircle (circleOptions);
            
             
        }
           

        /*public override void OnBackPressed(){
            StartActivity (typeof(NavigationDrawerActivity));
            //StartActivity(new Intent(Activity, typeof(NavigationDrawerActivity)));
            Finish ();
        }*/
       
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

        private LatLng GetCurrentLocation() { 

            Criteria locationCriteria = new Criteria();
            locationCriteria.Accuracy = Accuracy.Coarse;
            locationCriteria.PowerRequirement = Power.Medium;

            var service = (LocationManager)GetSystemService(LocationService); 
            var provider = service.GetBestProvider(locationCriteria, true); 
            var location = service.GetLastKnownLocation(provider); 

            if (provider != null) {
                service.RequestLocationUpdates (provider, 2000, 1, this);
            } else {
                Toast.MakeText (this, "No location providers available", ToastLength.Short).Show ();
            }

            return new LatLng(location.Latitude, location.Longitude);          
        }

        private void SetupMapIfNeeded()
        {
            if (_map == null)
            {
                _map = _mapFragment.Map;
                if (_map != null) {
                    MarkerOptions marker1 = new MarkerOptions();
                    marker1.SetPosition(newLocation);
                    _map.AddMarker(marker1);

                    // We create an instance of CameraUpdate, and move the map to it.
                    CameraUpdate cameraUpdate = CameraUpdateFactory.NewLatLngZoom(newLocation, 15);
                    _map.MoveCamera(cameraUpdate);
                   
                }
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
