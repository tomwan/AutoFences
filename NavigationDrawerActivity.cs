using System;

using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Support.V4.App;
using Android.Support.V4.View;
using Android.Support.V4.Widget;
using Android.Support.V7.Widget;
using Mojio.Client;
using Mojio;

//Ambiguities
using Fragment = Android.App.Fragment;

namespace AutoFences
{
    [Activity (Label = "@string/app_name", Icon = "@drawable/icon", NoHistory = true)]
    public class NavigationDrawerActivity : Activity, FragmentAdapter.OnItemClickListener
    {
        private DrawerLayout mDrawerLayout;
        private RecyclerView mDrawerList;
        private ActionBarDrawerToggle mDrawerToggle;

        private string mDrawerTitle;
        private String[] navDrawerTitles = new string[3];

        protected override void OnCreate (Bundle savedInstanceState)
        {

            base.OnCreate (savedInstanceState);
            SetContentView (Resource.Layout.activity_navigation_drawer);

            mDrawerTitle = this.Title;
            navDrawerTitles [0] = this.Resources.GetString (Resource.String.select_device);
            navDrawerTitles [1] = this.Resources.GetString (Resource.String.settings);
            navDrawerTitles [2] = this.Resources.GetString (Resource.String.help);
            mDrawerLayout = FindViewById<DrawerLayout> (Resource.Id.drawer_layout);
            mDrawerList = FindViewById<RecyclerView> (Resource.Id.left_drawer);

            // set a custom shadow that overlays the main content when the drawer opens
            mDrawerLayout.SetDrawerShadow (Resource.Drawable.drawer_shadow, GravityCompat.Start);
            // improve performance by indicating the list if fixed size.
            mDrawerList.HasFixedSize = true;
            mDrawerList.SetLayoutManager (new LinearLayoutManager (this));

            // set up the drawer's list view with items and click listener
            mDrawerList.SetAdapter (new FragmentAdapter (navDrawerTitles, this));
            // enable ActionBar app icon to behave as action to toggle nav drawer
            this.ActionBar.SetDisplayHomeAsUpEnabled (true);
            this.ActionBar.SetHomeButtonEnabled (true);

            // ActionBarDrawerToggle ties together the the proper interactions
            // between the sliding drawer and the action bar app icon

            mDrawerToggle = new MyActionBarDrawerToggle (this, mDrawerLayout,
                Resource.Drawable.ic_drawer, 
                Resource.String.drawer_open, 
                Resource.String.drawer_close);

            mDrawerLayout.SetDrawerListener (mDrawerToggle);
            if (savedInstanceState == null) //first launch
                selectItem (0);
        }

        internal class MyActionBarDrawerToggle : ActionBarDrawerToggle
        {
            NavigationDrawerActivity owner;

            public MyActionBarDrawerToggle (NavigationDrawerActivity activity, DrawerLayout layout, int imgRes, int openRes, int closeRes)
                : base (activity, layout, imgRes, openRes, closeRes)
            {
                owner = activity;
            }

            public override void OnDrawerClosed (View drawerView)
            {
                owner.ActionBar.Title = owner.Title;
                owner.InvalidateOptionsMenu ();
            }

            public override void OnDrawerOpened (View drawerView)
            {
                owner.ActionBar.Title = owner.mDrawerTitle;
                owner.InvalidateOptionsMenu ();
            }
        }

        public override bool OnCreateOptionsMenu (IMenu menu)
        {
            // Inflate the menu; this adds items to the action bar if it is present.
            this.MenuInflater.Inflate (Resource.Menu.navigation_drawer, menu);
            return true;
        }

        /* Called whenever we call invalidateOptionsMenu() */
        public override bool OnPrepareOptionsMenu (IMenu menu)
        {
            // If the nav drawer is open, hide action items related to the content view
            bool drawerOpen = mDrawerLayout.IsDrawerOpen (mDrawerList);
            //menu.FindItem (Resource.Id.action_websearch).SetVisible (!drawerOpen);
            return base.OnPrepareOptionsMenu (menu);
        }

        public override bool OnOptionsItemSelected (IMenuItem item)
        {
            // The action bar home/up action should open or close the drawer.
            // ActionBarDrawerToggle will take care of this.
            if (mDrawerToggle.OnOptionsItemSelected (item)) {
                return true;
            }
            return base.OnOptionsItemSelected (item);

        }

        /* The click listener for RecyclerView in the navigation drawer */
        public void OnClick (View view, int position)
        {
            selectItem (position);
        }

        private void selectItem (int position)
        {
            // update the main content by replacing fragments
            var fragment = DisplayFragment.NewInstance ();
            if (position == 0) {
                fragment = DisplayFragment.NewInstance ();
            } else if (position == 1) {
                fragment = SettingsFragment.NewInstance ();
            } else if (position == 2) {
                fragment = HelpFragment.NewInstance ();
            }

            var fragmentManager = this.FragmentManager;
            var ft = fragmentManager.BeginTransaction ();
            ft.Replace (Resource.Id.content_frame, fragment);
            ft.Commit ();

            // update selected item title, then close the drawer
            mDrawerLayout.CloseDrawer (mDrawerList);
        }

        //      private void SetTitle (string title)
        //      {
        //          this.Title = title;
        //          this.ActionBar.Title = title;
        //      }

        protected override void OnTitleChanged (Java.Lang.ICharSequence title, Android.Graphics.Color color)
        {
            //base.OnTitleChanged (title, color);
            //this.ActionBar.Title = title.ToString ();
        }

        /**
         * When using the ActionBarDrawerToggle, you must call it during
         * onPostCreate() and onConfigurationChanged()...
        */

        protected override void OnPostCreate (Bundle savedInstanceState)
        {
            base.OnPostCreate (savedInstanceState);
            // Sync the toggle state after onRestoreInstanceState has occurred.
            mDrawerToggle.SyncState ();
        }

        public override void OnConfigurationChanged (Configuration newConfig)
        {
            base.OnConfigurationChanged (newConfig);
            // Pass any configuration change to the drawer toggls
            mDrawerToggle.OnConfigurationChanged (newConfig);
        }

        /**
         * Fragment that appears in the "content_frame", shows the display fragment
         */
        internal class DisplayFragment : Fragment
        {

            public DisplayFragment ()
            {
                // Empty constructor required for fragment subclasses
            }

            public static Fragment NewInstance ()
            {
                Fragment fragment = new DisplayFragment ();
                return fragment;
            }

            public async void getTripData (View view)
            {
                var prefs = Application.Context.GetSharedPreferences ("settings", FileCreationMode.Private);
                var prefEditor = prefs.Edit();

                if (MojioConnectionHelper.isClientLoggedIn()) {
                    await MojioConnectionHelper.setupMojioConnection (prefs);
                }
                Globals.client.PageSize = 15; //Gets 15 results
                MojioResponse<Results<Trip>> response = await Globals.client.GetAsync<Trip> ();
                Results<Trip> result = response.Data;

                var results = view.FindViewById<TextView> (Resource.Id.tripResults);
                
                int tripIndex = 1;
                String outputString = "";

                // Iterate over each trip
                foreach (Trip trip in result.Data) {
                    outputString += string.Format ("Trip {0}:", tripIndex) + System.Environment.NewLine + "Start time: " + trip.StartTime.ToString ()
                    + System.Environment.NewLine + "End time: " + trip.EndTime.ToString () + System.Environment.NewLine + "Longitude: "
                    + trip.EndLocation.Lng.ToString () + System.Environment.NewLine + "Latitude: " + trip.EndLocation.Lat.ToString ()
                    + System.Environment.NewLine + "Max Speed: " + trip.MaxSpeed.Value.ToString () + " km/h"
                    + System.Environment.NewLine + System.Environment.NewLine;
                    tripIndex++;
                }
                //Toast.MakeText (this, "async task worked", ToastLength.Short).Show ();
                results.Text = outputString;
            }

            public override View OnCreateView (LayoutInflater inflater, ViewGroup container,
                                      Bundle savedInstanceState)
            {
                View rootView = inflater.Inflate (Resource.Layout.Display, container, false);
                getTripData (rootView);
                return rootView;
            }
        }

        /**
         * Fragment that appears in the "content_frame", shows the display fragment
         */
        internal class SettingsFragment : Fragment
        {
            public SettingsFragment ()
            {
                // Empty constructor required for fragment subclasses
            }

            public static Fragment NewInstance ()
            {
                Fragment fragment = new SettingsFragment ();
                return fragment;
            }

            public override View OnCreateView (LayoutInflater inflater, ViewGroup container,
                                               Bundle savedInstanceState)
            {
                View rootView = inflater.Inflate (Resource.Layout.Settings, container, false);
                var ht = rootView.FindViewById<TextView> (Resource.Id.settingsText);
                ht.Text = rootView.Resources.GetString (Resource.String.settings_placeholder);
                return rootView;
            }
        }

        /**
         * Fragment that appears in the "content_frame", shows the display fragment
         */
        internal class HelpFragment : Fragment
        {
            public HelpFragment ()
            {
                // Empty constructor required for fragment subclasses
            }

            public static Fragment NewInstance ()
            {
                Fragment fragment = new HelpFragment ();
                return fragment;
            }

            public override View OnCreateView (LayoutInflater inflater, ViewGroup container,
                                               Bundle savedInstanceState)
            {
                View rootView = inflater.Inflate (Resource.Layout.Help, container, false);
                var ht = rootView.FindViewById<TextView> (Resource.Id.helpText);
                ht.Text = rootView.Resources.GetString (Resource.String.help_placeholder);
                return rootView;
            }
        }
    }
}