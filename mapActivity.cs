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
    [Activity (Label = "mapActivity")]            
    public class mapActivity : Activity, ILocationListener
    {
        private GoogleMap _map;
        private MapFragment _mapFragment;
        private LocationManager locMgr;
        private LatLng currentLocation = new LatLng(49.2677, -123.2564);

        protected override void OnCreate (Bundle bundle)
        {
            base.OnCreate (bundle);
            SetContentView (Resource.Layout.mapLayout);
            InitMapFragment();
            SetupMapIfNeeded();
            SetupZoomInButton ();
            SetupZoomOutButton();

        }

        protected override void OnResume()
        {
            base.OnResume();
            SetupMapIfNeeded();

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

        private void SetupMapIfNeeded()
        {
            if (_map == null)
            {
                _map = _mapFragment.Map;
                if (_map != null)
                {
                    MarkerOptions marker1 = new MarkerOptions();
                    marker1.SetPosition(currentLocation);
                    _map.AddMarker(marker1);

                    // We create an instance of CameraUpdate, and move the map to it.
                    CameraUpdate cameraUpdate = CameraUpdateFactory.NewLatLngZoom(currentLocation, 15);
                    _map.MoveCamera(cameraUpdate);
                }
            }
        }

        private void SetupZoomInButton()
        {
            Button zoomInButton = FindViewById<Button>(Resource.Id.zoomInButton);
            zoomInButton.Click += (sender, e) => { _map.AnimateCamera(CameraUpdateFactory.ZoomIn()); };
        }

        private void SetupZoomOutButton()
        {
            Button zoomOutButton = FindViewById<Button>(Resource.Id.zoomOutButton);
            zoomOutButton.Click += (sender, e) => { _map.AnimateCamera(CameraUpdateFactory.ZoomOut()); };
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
