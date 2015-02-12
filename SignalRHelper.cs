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
        public async static Task SignalRSetup ()
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
                EventType.FenceExited
            };
            Globals.client.EventHandler += ReceiveEvent;  //Call event handler
            await Globals.client.Subscribe<Vehicle> (vehicleID, types);  //Subscribe to Mojio events with ID
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
            Globals.client.Observe (result.Data);

            // Register the Event Callback Handler for when a fence is entered or exited.
            Globals.client.ObserveHandler += (entity) => {
                var vehicle = entity as Vehicle;
                //if (vehicle) {
                //Will write  something
                //}
            };
        }

        public static void SignalRCleanup () {
            Guid vehicleID = new Guid (Configurations.vehicleID);
            EventType[] types = new EventType[] {
                EventType.IgnitionOn,
                EventType.IgnitionOff,
                EventType.Speed,
                EventType.FenceEntered,
                EventType.FenceExited
            };
            Globals.client.Unsubscribe<Vehicle> (vehicleID, types);
        }

        public static void ReceiveEvent (Event events)
        {
            if (events.EventType == EventType.IgnitionOff) {
                Console.WriteLine ("Ignition Off!");
                Notification.Builder builder = new Notification.Builder (Application.Context)
                    .SetContentTitle ("Mojio Notification")
                    .SetContentText ("Vehicle Ignition is Off!")
                    .SetSmallIcon (Resource.Drawable.Icon);

                // Get the notification manager:
                NotificationManager notificationManager = Application.Context.GetSystemService (Context.NotificationService) as NotificationManager;
                builder.SetDefaults (NotificationDefaults.Sound | NotificationDefaults.Vibrate);
                Notification notification = builder.Build();

                // Publish the notification:
                const int notificationId = 0;
                notificationManager.Notify (notificationId, notification);

            } else if (events.EventType == EventType.IgnitionOn) {
                Console.WriteLine ("Ignition On!");
                Notification.Builder builder = new Notification.Builder (Application.Context)
                    .SetContentTitle ("Mojio Notification")
                    .SetContentText ("Vehicle Ignition is On!")
                    .SetSmallIcon (Resource.Drawable.Icon);

                NotificationManager notificationManager = Application.Context.GetSystemService (Context.NotificationService) as NotificationManager;
                builder.SetDefaults (NotificationDefaults.Sound | NotificationDefaults.Vibrate);
                Notification notification = builder.Build();

                // Publish the notification:
                const int notificationId = 0;
                notificationManager.Notify (notificationId, notification);
            } else if (events.EventType == EventType.Speed) {
                Console.WriteLine ("Speed !");
                Notification.Builder builder = new Notification.Builder (Application.Context)
                    .SetContentTitle ("Mojio Notification")
                    .SetContentText ("Vehicle Speeding!")
                    .SetSmallIcon (Resource.Drawable.Icon);

                NotificationManager notificationManager = Application.Context.GetSystemService (Context.NotificationService) as NotificationManager;
                builder.SetDefaults (NotificationDefaults.Sound | NotificationDefaults.Vibrate);
                Notification notification = builder.Build();

                // Publish the notification:
                const int notificationId = 0;
                notificationManager.Notify (notificationId, notification);
            } else if (events.EventType == EventType.FenceEntered) {
                Console.WriteLine ("FenceEntered !");
                Notification.Builder builder = new Notification.Builder (Application.Context)
                    .SetContentTitle ("Mojio Notification")
                    .SetContentText ("Vehicle Inside Fence!")
                    .SetSmallIcon (Resource.Drawable.Icon);

                NotificationManager notificationManager = Application.Context.GetSystemService (Context.NotificationService) as NotificationManager;
                builder.SetDefaults (NotificationDefaults.Sound | NotificationDefaults.Vibrate);
                Notification notification = builder.Build();

                // Publish the notification:
                const int notificationId = 0;
                notificationManager.Notify (notificationId, notification);
            } else if (events.EventType == EventType.FenceExited) {
                Console.WriteLine ("FenceExited !");
                Notification.Builder builder = new Notification.Builder (Application.Context)
                    .SetContentTitle ("Mojio Notification")
                    .SetContentText ("Vehicle Outside!")
                    .SetSmallIcon (Resource.Drawable.Icon);

                NotificationManager notificationManager = Application.Context.GetSystemService (Context.NotificationService) as NotificationManager;
                builder.SetDefaults (NotificationDefaults.Sound | NotificationDefaults.Vibrate);
                Notification notification = builder.Build();

                // Publish the notification:
                const int notificationId = 0;
                notificationManager.Notify (notificationId, notification);
            }
        }
    }
}