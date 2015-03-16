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


namespace AutoFences
{
    public class SignalRHelper
    {
        //TODO: Get rid of these constant variables after settings page is implemented
        private const int StartTime = 8;
        private const int EndTime = 24;

        private static Boolean alertedChronoFencing = false, alertedOutsideGeoFence = false;

        public async static Task SignalRSetup (MojioClient client)
        {
            Guid appID = new Guid (Configurations.appID);
            Guid secretKey = new Guid (Configurations.secretKey);
            Guid vehicleID = new Guid (Configurations.vehicleID);

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

            // Setting up Geographical Spherical Fence
            var center = new Location {
                Lat = 49.25,
                Lng = 123.1
            };

            var radius = 25;  // radius in km

            // Create a new observer
            var observer = new GeoFenceObserver (vehicleID, center, radius);
            var result = await Globals.client.CreateAsync (observer);

            // Subscript SignalR to the observer
            client.Observe (result.Data);

            // Register the Event Callback Handler for when a fence is entered or exited.
            client.ObserveHandler += (entity) => {
                var vehicle = entity as Vehicle;
                //if (vehicle) {
                //Will write  something
                //}
            };
        }

        public static void SignalRCleanup (MojioClient client) {
            Guid vehicleID = new Guid (Configurations.vehicleID);
            EventType[] types = new EventType[] {
                EventType.IgnitionOn,
                EventType.IgnitionOff,
                EventType.Speed,
                EventType.FenceEntered,
                EventType.FenceExited
            };
            client.Unsubscribe<Vehicle> (vehicleID, types);
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
                //TODO: retrieve preset times from settings.
                TimeSpan startTime = new TimeSpan (StartTime, 0, 0);//temporarily hardcoded.
                TimeSpan endTime = new TimeSpan (EndTime, 0, 0);
                TimeSpan nowTime = DateTime.Now.TimeOfDay;

                if (!((nowTime > startTime) && (nowTime < endTime))) {
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
            } else if (events.EventType == EventType.Speed) {
                Console.WriteLine ("Speeding!");
                Notification.Builder builder = new Notification.Builder (Application.Context)
                    .SetContentTitle ("Mojio Notification")
                    .SetContentText ("Vehicle Speeding!")
                    .SetSmallIcon (Resource.Drawable.ic_logo);

                NotificationManager notificationManager = Application.Context.GetSystemService (Context.NotificationService) as NotificationManager;
                builder.SetDefaults (NotificationDefaults.Sound | NotificationDefaults.Vibrate);
                Notification notification = builder.Build();

                // Publish the notification:
                const int notificationId = 1;
                notificationManager.Notify (notificationId, notification);
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
                //TODO: retrieve preset times from settings.
                TimeSpan startTime = new TimeSpan (StartTime, 0, 0); //temporarily hardcoded.
                TimeSpan endTime = new TimeSpan (EndTime, 0, 0); //temporarily hardcoded.
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