using System;
using System.Collections.Generic;
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
using Mojio.Events;
using AFLib;

//Ambiguities
using Fragment = Android.App.Fragment;

namespace AutoFences
{
    [Activity (Icon = "@drawable/ic_logo")]
    public class NavigationDrawerActivity : Activity, FragmentAdapter.OnItemClickListener
    {
        private DrawerLayout mDrawerLayout;
        private RecyclerView mDrawerList;
        private ActionBarDrawerToggle mDrawerToggle;

        private string mDrawerTitle;
        private String[] navDrawerTitles = new string[3];

        //used for refreshing main trip page
        private static Boolean firstStart = true;
        private static  List<TripData> list;
        private static  String outputString = "";
        private static  String lastTime = "";
        private static  int tripIndex;
        private static  double fuelEcon;

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
            if (savedInstanceState == null){
                //first launch
                selectItem (0);
                SetTitle("Trips");

            }

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
            if(position == 0) SetTitle("Alerts");
            if(position == 1) SetTitle("Settings");
            if(position == 2) SetTitle("About");
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

        private void SetTitle (string title)
        {
            this.Title = title;
            this.ActionBar.Title = title;
        }

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

            }

            public static Fragment NewInstance ()
            {
                Fragment fragment = new DisplayFragment ();
                fragment.RetainInstance = true;
                return fragment;
            }

            public async void getTripData (View view)
            {

                //TODO use global preference instead
                var numTripsToDisplay = 20;

                var prefs = Application.Context.GetSharedPreferences ("settings", FileCreationMode.Private);
                var prefEditor = prefs.Edit ();

                if (!MojioConnectionHelper.isClientLoggedIn ()) {
                    await MojioConnectionHelper.setupMojioConnection (prefs);
                }


                if(firstStart){
                    firstStart = false;
                    Globals.client.PageSize = 500; //Gets 15 results
                    MojioResponse<Results<Trip>> response = await Globals.client.GetAsync<Trip> ();
                    Results<Trip> result = response.Data;

                    //var results = view.FindViewById<TextView> (Resource.Id.tripResults);

                    tripIndex = 0;
                    fuelEcon = 0.0;


                    tripIndex--;

                    list = new List<TripData> ();
                    //iterate over each trip to create TripData for each existing trip.

                    foreach (Trip trip in result.Data) {
                        try {
                            fuelEcon += (double) trip.FuelEfficiency;
                            tripIndex++;
                            TripData td = new TripData (trip.StartTime, trip.EndTime, trip.MaxSpeed.Value.ToString (), trip.EndLocation.Lat.ToString (), trip.EndLocation.Lng.ToString (),
                                              trip.FuelEfficiency.ToString (), trip.FuelLevel.ToString (), trip.StartLocation.Lat.ToString (), trip.StartLocation.Lng.ToString ());
                            //add new trip to beginning of list, so they are in most recent first order
                            list.Insert (0, td);
                        } catch (Exception e) {
                            Console.WriteLine ("Exception:" + e);
                        }
                    }
                }
                prefEditor.PutString ("speed", "disabled");

                int i = 1;
                var firstTrip = true;
                var tripsDisplayed = 0;
                // programmatically create a view widget for each trip
                LinearLayout linlay = view.FindViewById<LinearLayout> (Resource.Id.linearLayout28);

                // remove all old children
                Console.Write ("NUMCHILDREN: " + linlay.ChildCount);

                for(int j = linlay.ChildCount-1; j >= 0; j--){
                    linlay.RemoveViewAt (j);

                }

                //places all the UI elements
                foreach (TripData td in list) {
                    if(tripsDisplayed > numTripsToDisplay){
                        break;
                    } else {
                        tripsDisplayed++;
                    }

                    // get most recent time
                    if(firstTrip){
                        lastTime = td.startDate + " @ " + td.startTime;
                        firstTrip = false;
                    } else {
                        // drive divider first for every one except for first card
                        var space = new Space (Application.Context){
                            LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, 10)
                        };
                        var divider = new LinearLayout (Application.Context){
                            LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, 1)
                        };

                        divider.SetBackgroundColor (Resources.GetColor(17170432)); //darker grey

                        //add divider then space for padding
                        linlay.AddView (divider);
                        linlay.AddView (space);
                    }


                    ImageView mapButton = new ImageView (Application.Context);
                    mapButton.SetImageResource (Resource.Drawable.mapButton);
                    mapButton.SetAdjustViewBounds (true);
                    mapButton.Click += delegate {                      
                        var tripviewActivity = new Intent (Activity, typeof(tripviewActivity));
                        // Create bundle to send to tripViewActivity
                        Bundle extras = new Bundle();
                        extras.PutString ("endlat", (td.endlocationlat));
                        extras.PutString("endlng", (td.endlocationlng));
                        extras.PutString ("startlat", (td.startlocationlat));
                        extras.PutString("startlng", (td.startlocationlng));
                        extras.PutString ("fuelEfficiency", (td.fuelEfficiency));
                        extras.PutString("fuelLevel", (td.fuelLevel));
                        extras.PutString ("startTime", (td.startTime));
                        extras.PutString("startDate",(td.startDate));
                        extras.PutString("maxSpeed", (td.maxSpeed));
                        extras.PutString("endTime", (td.endDateTime));
                        tripviewActivity.PutExtra("extras", extras);                     
                        StartActivity (tripviewActivity);
                    };
                    linlay.AddView (mapButton);

                    TextView tv = new TextView (Application.Context);
                    tv.Text = td.startDate + " @ " + td.startTime;
                    tv.TextSize = 30;
                    //tv.Elevation = 4;
                    tv.SetPadding (5, 5, 5, 5);
                    //tv.SetBackgroundColor (Android.Graphics.Color.ParseColor("#BBDEFB"));
                    tv.SetTextColor (Resources.GetColor(Resource.Color.black_text));
                    linlay.AddView (tv);
                    i++;

                    LinearLayout innerll1 = new LinearLayout (Application.Context);
                    innerll1.Orientation = Android.Widget.Orientation.Horizontal;
                    innerll1.Id = i + 5000;
                    //innerll1.Elevation = 4;
                    innerll1.SetPadding (5, 5, 5, 5);
                    //innerll1.SetBackgroundColor (Android.Graphics.Color.ParseColor("#BBDEFB"));

                    ImageView iv1 = new ImageView (Application.Context);
                    iv1.SetImageResource (Resource.Drawable.stopwatch);
                    iv1.SetMaxHeight (50);
                    iv1.SetColorFilter (Resources.GetColor(Resource.Color.accent));
                    iv1.SetAdjustViewBounds (true);
                    innerll1.AddView (iv1);

                    TextView tv1 = new TextView (Application.Context);
                    tv1.Text = "   " + td.tripLength;
                    tv1.TextSize = 20;
                    tv1.SetTextColor (Resources.GetColor(Resource.Color.secondary_text));
                    innerll1.AddView (tv1);
                    linlay.AddView (innerll1);

                    LinearLayout innerll2 = new LinearLayout (Application.Context);
                    innerll2.Orientation = Android.Widget.Orientation.Horizontal;
                    //innerll2.Elevation = 4;
                    innerll2.SetPadding (5, 5, 5, 5);
                    //innerll2.SetBackgroundColor (Android.Graphics.Color.ParseColor("#BBDEFB"));

                    ImageView iv2 = new ImageView (Application.Context);
                    iv2.SetImageResource (Resource.Drawable.speedometer);
                    iv2.SetMaxHeight (50);
                    iv2.SetColorFilter (Resources.GetColor(Resource.Color.accent));
                    iv2.SetAdjustViewBounds (true);
                    innerll2.AddView (iv2);

                    TextView tv2 = new TextView (Application.Context);
                    tv2.Text = "   " + td.maxSpeed;
                    tv2.TextSize = 20;
                    //tv2.Elevation = 4;
                    tv2.SetTextColor (Resources.GetColor(Resource.Color.secondary_text));
                    innerll2.AddView (tv2);
                    linlay.AddView (innerll2);

                    Space spc = new Space (Application.Context);
                    spc.SetMinimumHeight (14);
                    linlay.AddView (spc);


                }

                var fuelEfficiecny = view.FindViewById<TextView> (Resource.Id.fuelUsage);
                var lastTripTime = view.FindViewById<TextView> (Resource.Id.lastTripTime);
                var fe = Math.Round((fuelEcon / tripIndex),1);
                fuelEfficiecny.Text = "   " + fe + " L/100km";
                lastTripTime.Text = "   " + lastTime;

            }

            public override View OnCreateView (LayoutInflater inflater, ViewGroup container,
                Bundle savedInstanceState)
            {
                View rootView = inflater.Inflate (Resource.Layout.Display, container, false);


                getTripData (rootView);

                var refresher = rootView.FindViewById<SwipeRefreshLayout> (Resource.Id.refresher);
                refresher.SetColorScheme (Resource.Color.primary_dark,
                    Resource.Color.accent,
                    Resource.Color.primary,
                    Resource.Color.primary_dark);
                refresher.Refresh += async delegate {

                    firstStart = true;
                    Toast.MakeText (Application.Context, "Refreshing...", ToastLength.Short).Show ();
                    getTripData(rootView);
                    refresher.Refreshing = false;
                };

                Button launchMap = rootView.FindViewById<Button> (Resource.Id.MapButton);

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


                var save = rootView.FindViewById<Button> (Resource.Id.saveTime);
                var launchMap = rootView.FindViewById<Button> (Resource.Id.MapButton);
                var startTime = rootView.FindViewById<EditText> (Resource.Id.startTime);
                var endTime = rootView.FindViewById<EditText> (Resource.Id.endTime);
                var togglebutton = rootView.FindViewById<ToggleButton>(Resource.Id.toggleButton);

                var prefs = Application.Context.GetSharedPreferences ("settings", FileCreationMode.Private);
                var prefEditor = prefs.Edit();

                

                if (!prefs.GetBoolean ("speed", true)) {
                    togglebutton.Checked = false;
                }

                togglebutton.Click += (o, e) => {
                    if(togglebutton.Checked == false) {
                        prefEditor.PutBoolean("speed", false);
                        prefEditor.Apply();
                    } else {
                        prefEditor.PutBoolean("speed", true);
                        prefEditor.Apply();
                    }
                };

                if (!(prefs.GetInt ("startHour", 25) == 25)) {
                    startTime.Text = Convert.ToDateTime(prefs.GetInt ("startHour", 0) + ":" + (prefs.GetInt ("startMinute", 0))).ToString("HH:mm");
                    endTime.Text = Convert.ToDateTime(prefs.GetInt ("endHour", 0) + ":" + (prefs.GetInt ("endMinute", 0))).ToString("HH:mm");
                }

                //save the chronofence
                save.Click += delegate {
                    string[] startTimeArray = startTime.Text.Split(':');
                    string[] endTimeArray = endTime.Text.Split(':');
                    if(startTimeArray.Length == 2 && endTimeArray.Length == 2) {
                        DateTime startTimeCompare = Convert.ToDateTime(startTime.Text);
                        DateTime endTimeCompare = Convert.ToDateTime(endTime.Text);

                        if(DateTime.Compare(startTimeCompare, endTimeCompare) < 0) { //Make sure start time is less than end time
                            if(Convert.ToInt32(startTimeArray[0]) < 24 && Convert.ToInt32(startTimeArray[1]) < 60 &&
                                Convert.ToInt32(endTimeArray[0]) < 24 && Convert.ToInt32(endTimeArray[1]) < 60) { // Make sure all the numbers make sense
                                prefEditor.PutInt("startHour", Convert.ToInt32(startTimeArray[0]));
                                prefEditor.PutInt("startMinute",Convert.ToInt32(startTimeArray[1]));
                                prefEditor.PutInt("endHour", Convert.ToInt32(endTimeArray[0]));
                                prefEditor.PutInt("endMinute", Convert.ToInt32(endTimeArray[1]));
                                prefEditor.Apply();
                            }
                        }
                    }
                    //TODO tommy, link this
                    //TODO should probably be a check here to see if input is valid [should be hh:mm]
                    Console.WriteLine ("Chronofence starts at {0} PM and ends at {1}AM", startTime.Text, endTime.Text);
                    Toast.MakeText (Application.Context, "Saved...", ToastLength.Short).Show ();
                };

                launchMap.Click += delegate {
                    //StartActivity (typeof(mapActivity));
                    StartActivity(new Intent(Activity, typeof(mapActivity)));
                };
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




                var mojioHelp = rootView.FindViewById<Button> (Resource.Id.useMojio);
                mojioHelp.Text = "Moj.io web page";
                mojioHelp.Click += delegate {
                    var uri = Android.Net.Uri.Parse ("https://www.moj.io/#howitworks");
                    var intent = new Intent (Intent.ActionView, uri); 
                    StartActivity (intent);     
                };
                return rootView;
            }
        }
    }
}