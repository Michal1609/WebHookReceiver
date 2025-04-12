using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;
using System;

namespace WebHookNotifierMaui.Platforms.Android
{
    /// <summary>
    /// Manages Android notifications
    /// </summary>
    public class AndroidNotificationManager
    {
        private const string ChannelId = "webhook_notifications";
        private const string ChannelName = "WebHook Notifications";
        private const string ChannelDescription = "Notifications from WebHook Receiver";

        private NotificationManager _notificationManager;
        private static AndroidNotificationManager _instance;
        private int _notificationId = 100;

        public static AndroidNotificationManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new AndroidNotificationManager();
                }
                return _instance;
            }
        }

        private AndroidNotificationManager()
        {
            _notificationManager = (NotificationManager)global::Android.App.Application.Context.GetSystemService(Context.NotificationService);

            // Create notification channel for Android 8.0 and higher
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var channel = new NotificationChannel(ChannelId, ChannelName, NotificationImportance.Default)
                {
                    Description = ChannelDescription
                };

                _notificationManager.CreateNotificationChannel(channel);
            }
        }

        public void SendNotification(string title, string message)
        {
            // Get the main activity intent
            var intent = new Intent(global::Android.App.Application.Context, typeof(MainActivity));
            intent.AddFlags(ActivityFlags.ClearTop);

            // Create pending intent
            var pendingIntent = PendingIntent.GetActivity(
                global::Android.App.Application.Context,
                0,
                intent,
                PendingIntentFlags.Immutable);

            // Build the notification
            var builder = new NotificationCompat.Builder(global::Android.App.Application.Context, ChannelId)
                .SetContentTitle(title)
                .SetContentText(message)
                //.SetSmallIcon()
                //.SetSmallIcon(Android.Resource.Drawable.IcDialogInfo)
                .SetContentIntent(pendingIntent)
                .SetAutoCancel(true);

            // For longer messages, use big text style
            if (message.Length > 40)
            {
                var bigTextStyle = new NotificationCompat.BigTextStyle();
                bigTextStyle.BigText(message);
                builder.SetStyle(bigTextStyle);
            }

            // Show the notification
            _notificationManager.Notify(_notificationId++, builder.Build());
        }
    }
}
