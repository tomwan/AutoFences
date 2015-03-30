using System;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Systems;
using Mojio;
using Mojio.Client;
using Mojio.Events;
using Android.Gms.Maps.Model;


namespace AutoFences
{
    public class SignalRHelper
    {
        //TODO: Get rid of these constant variables after settings page is implemented
        private const int StartTime = 8;
        private const int EndTime = 24;

        private static Boolean alertedChronoFencing = false, alertedOutsideGeoFence = false;
        private static ISharedPreferences savedPref;

        public async static Task SignalRSetup (MojioClient client, ISharedPreferences prefs)
        {
            Guid appID = new Guid (AFLib.Configurations.appID);
            Guid secretKey = new Guid (AFLib.Configurations.secretKey);
            Guid vehicleID = new Guid (AFLib.Configurations.vehicleID);
            savedPref = prefs;

            //--------------------Subscribing to SignalR Events--------------------------//
            EventType[] types = new EventType[] {
                EventType.IgnitionOn,
                EventType.IgnitionOff,
                EventType.Speed,
                EventType.FenceEntered,
                EventType.FenceExited,
                EventType.TripStatus
            };
            client.EventHandler += ReceiveEvent;  //Call event handler
            await client.Subscribe<Vehicle> (vehicleID, types);  //Subscribe to Mojio events with ID
            Console.WriteLine ("Subscription to Mojio SignalR events sucessful!");

            //Setting up Geographical Spherical Fence
            var center = new Location {
                Lat = Convert.ToDouble(prefs.GetString("geofencinglatitude", null)),
                Lng = Convert.ToDouble(prefs.GetString("geofencinglongitude", null))
            };

            var radius = prefs.GetInt ("geofencingradius", 0);  // radius in km

            // Create a new observer
            if (center.Lat != null && center.Lng != null && radius > 0) {
                var observer = new GeoFenceObserver (vehicleID, center, radius);
                var result = await client.CreateAsync (observer);

                // Subscript SignalR to the observer
                client.Observe (result.Data);

                // Register the Event Callback Handler for when a fence is entered or exited.
                client.ObserveHandler += (entity) => {
                    var vehicle = entity as Vehicle;
                    Notification.Builder builder = new Notification.Builder (Application.Context)
                        .SetContentTitle ("Mojio GeoFencing Alert")
                        .SetContentText ("Vehicle has crossed the GeoFence.")
                        .SetSmallIcon (Resource.Drawable.ic_logo);
                };
            }
        }

        public static void SignalRCleanup (MojioClient client) {
            Guid vehicleID = new Guid (AFLib.Configurations.vehicleID);
            EventType[] types = new EventType[] {
                EventType.IgnitionOn,
                EventType.IgnitionOff,
                EventType.Speed,
                EventType.FenceEntered,
                EventType.FenceExited
            };
            client.Unsubscribe<Vehicle> (vehicleID, types);
        }

        public async static Task updateGeoFencing (LatLng newLocation, int radius) {
            Guid vehicleID = new Guid (AFLib.Configurations.vehicleID);
            var center = new Location {
                Lat = newLocation.Latitude,
                Lng = newLocation.Longitude
            };
            
            var observer = new GeoFenceObserver (vehicleID, center, radius);
            var result = await AFLib.Globals.client.CreateAsync (observer);

            // Subscript SignalR to the observer
            AFLib.Globals.client.Observe (result.Data);

            // Register the Event Callback Handler for when a fence is entered or exited.
            AFLib.Globals.client.ObserveHandler += (entity) => {
                var vehicle = entity as Vehicle;
                Notification.Builder builder = new Notification.Builder (Application.Context)
                    .SetContentTitle ("Mojio GeoFencing Alert")
                    .SetContentText ("Vehicle has crossed the GeoFence.")
                    .SetSmallIcon (Resource.Drawable.ic_logo);
            };
        }

        //TODO: Remove Console.WriteLine()
        public static void ReceiveEvent (Event events)
        {
            DateTime now = DateTime.Now.ToLocalTime();
            string currentTime = (string.Format ("{0}", now));

            if (events.EventType == EventType.IgnitionOff) {
                Console.WriteLine ("Ignition Off!");
            } else if (events.EventType == EventType.IgnitionOn) {
                Console.WriteLine ("Ignition On!");
                if (!(savedPref.GetInt ("startHour", 25) == 25)) {
                    TimeSpan startTime = new TimeSpan (savedPref.GetInt ("startHour", 0), savedPref.GetInt ("startMinute", 0), 0);
                    TimeSpan endTime = new TimeSpan (savedPref.GetInt ("endHour", 0), savedPref.GetInt ("endMinute", 0), 0);
                    TimeSpan nowTime = DateTime.Now.TimeOfDay;

                    if (!((nowTime > startTime) && (nowTime < endTime))) {
                        Console.WriteLine (startTime.Hours + ":" + startTime.Minutes + " " + endTime.Hours + ":" + endTime.Minutes);
                        Notification.Builder builder = new Notification.Builder (Application.Context)
                    .SetContentTitle ("Mojio ChronoFencing Alert")
                    .SetContentText ("Vehicle has been started outside of the preset timespan!")
                    .SetSmallIcon (Resource.Drawable.ic_logo);

                        NotificationManager notificationManager = Application.Context.GetSystemService (Context.NotificationService) as NotificationManager;
                        builder.SetDefaults (NotificationDefaults.Sound | NotificationDefaults.Vibrate);
                        Notification notification = builder.Build ();

                        // Publish the notification:
                        const int notificationId = 3;
                        notificationManager.Notify (notificationId, notification);
                    } else {
                        alertedChronoFencing = false;
                    }
                }
            } else if (events.EventType == EventType.Speed) {
                Console.WriteLine ("Speeding!");
                if (savedPref.GetBoolean ("speed", true)) {
                    Notification.Builder builder = new Notification.Builder (Application.Context)
                    .SetContentTitle ("Mojio Notification")
                    .SetContentText ("Vehicle Speeding!")
                    .SetSmallIcon (Resource.Drawable.ic_logo);

                    NotificationManager notificationManager = Application.Context.GetSystemService (Context.NotificationService) as NotificationManager;
                    builder.SetDefaults (NotificationDefaults.Sound | NotificationDefaults.Vibrate);
                    Notification notification = builder.Build ();

                    // Publish the notification:
                    const int notificationId = 1;
                    notificationManager.Notify (notificationId, notification);
                }
            } else if (events.EventType == EventType.FenceEntered) {
                Console.WriteLine ("Fence Entered!");
                if (alertedOutsideGeoFence) { //If previously outside of fence
                    Notification.Builder builder = new Notification.Builder (Application.Context)
                        .SetContentTitle ("Mojio GeoFencing Alert")
                        .SetContentText ("Vehicle has returned to the preset geofence perimeter")
                        .SetSmallIcon (Resource.Drawable.ic_logo);

                    NotificationManager notificationManager = Application.Context.GetSystemService (Context.NotificationService) as NotificationManager;
                    builder.SetDefaults (NotificationDefaults.Sound | NotificationDefaults.Vibrate);
                    Notification notification = builder.Build();

                    // Publish the notification:
                    const int notificationId = 4;
                    notificationManager.Notify (notificationId, notification);
                    alertedOutsideGeoFence = false;
                }
            } else if (events.EventType == EventType.FenceExited) {
                Console.WriteLine ("Fence Exited!");
                Notification.Builder builder = new Notification.Builder (Application.Context)
                    .SetContentTitle ("Mojio GeoFencing Alert")
                    .SetContentText ("Vehicle has left the preset geofence perimeter")
                    .SetSmallIcon (Resource.Drawable.ic_logo);

                NotificationManager notificationManager = Application.Context.GetSystemService (Context.NotificationService) as NotificationManager;
                builder.SetDefaults (NotificationDefaults.Sound | NotificationDefaults.Vibrate);
                Notification notification = builder.Build();

                // Publish the notification:
                const int notificationId = 2;
                notificationManager.Notify (notificationId, notification);
                alertedOutsideGeoFence = true;
            } else if (events.EventType == EventType.TripStatus) { //If running
                if (!(savedPref.GetInt ("startHour", 25) == 25)) {
                    TimeSpan startTime = new TimeSpan (savedPref.GetInt ("startHour", 0), savedPref.GetInt ("startMinute", 0), 0);
                    TimeSpan endTime = new TimeSpan (savedPref.GetInt ("endHour", 0), savedPref.GetInt ("endMinute", 0), 0);
                    TimeSpan nowTime = DateTime.Now.TimeOfDay;

                    if (!((nowTime > startTime) && (nowTime < endTime))) { //Not in timespan
                        Notification.Builder builder = new Notification.Builder (Application.Context)
                    .SetContentTitle ("Mojio ChronoFencing Alert")
                    .SetContentText ("Vehicle is in motion outside of the preset timespan!")
                    .SetSmallIcon (Resource.Drawable.ic_logo);

                        NotificationManager notificationManager = Application.Context.GetSystemService (Context.NotificationService) as NotificationManager;
                        builder.SetDefaults (NotificationDefaults.Sound | NotificationDefaults.Vibrate);
                        Notification notification = builder.Build ();
                        notification.Flags |= Android.App.NotificationFlags.OnlyAlertOnce;
                        // Publish the notification:
                        const int notificationId = 3;
                        notificationManager.Notify (notificationId, notification);
                        alertedChronoFencing = true;
                    } else { // In timespan
                        if (alertedChronoFencing) {
                            Notification.Builder builder = new Notification.Builder (Application.Context)
                            .SetContentTitle ("Mojio ChronoFencing Alert")
                            .SetContentText ("Vehicle is now inside of the preset timespan!")
                            .SetSmallIcon (Resource.Drawable.ic_logo);

                            NotificationManager notificationManager = Application.Context.GetSystemService (Context.NotificationService) as NotificationManager;
                            builder.SetDefaults (NotificationDefaults.Sound | NotificationDefaults.Vibrate);
                            Notification notification = builder.Build ();

                            // Publish the notification:
                            const int notificationId = 3;
                            notificationManager.Notify (notificationId, notification);
                        }
                        alertedChronoFencing = false;
                    }
                }
            }
        }
    }
}