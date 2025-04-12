#if WINDOWS
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
using System;

namespace WebHookNotifierMaui.Platforms.Windows
{
    /// <summary>
    /// Manages Windows notifications
    /// </summary>
    public class WindowsNotificationManager
    {
        private static WindowsNotificationManager _instance;

        public static WindowsNotificationManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new WindowsNotificationManager();
                }
                return _instance;
            }
        }

        private WindowsNotificationManager()
        {
            try
            {
                // Initialize Windows notifications
                Console.WriteLine("Initializing Windows notification manager");
                AppNotificationManager.Default.Register();
                Console.WriteLine("Windows notification manager initialized successfully");

                // Set up notification event handlers
                AppNotificationManager.Default.NotificationInvoked += (sender, args) =>
                {
                    Console.WriteLine($"Notification invoked: {args.Arguments}");
                };

                // Send a test notification to verify initialization
                SendTestNotification();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing Windows notification manager: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        private void SendTestNotification()
        {
            try
            {
                var builder = new AppNotificationBuilder()
                    .AddText("Notification System Initialized")
                    .AddText("The notification system is now ready to display notifications.");

                var notification = builder.BuildNotification();
                AppNotificationManager.Default.Show(notification);
                Console.WriteLine("Test notification sent");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending test notification: {ex.Message}");
            }
        }

        public void SendNotification(string title, string message)
        {
            try
            {
                Console.WriteLine($"Sending Windows notification: {title}");

                // Create notification with more formatting
                var builder = new AppNotificationBuilder()
                    .AddText(title)
                    .AddText(message);

                // Show notification
                var notification = builder.BuildNotification();
                AppNotificationManager.Default.Show(notification);

                Console.WriteLine("Notification sent successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error showing Windows notification: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");

                // Try to reinitialize and send again
                try
                {
                    Console.WriteLine("Attempting to reinitialize notification manager and resend");
                    AppNotificationManager.Default.Unregister();
                    AppNotificationManager.Default.Register();

                    var builder = new AppNotificationBuilder()
                        .AddText(title)
                        .AddText(message);

                    var notification = builder.BuildNotification();
                    AppNotificationManager.Default.Show(notification);
                    Console.WriteLine("Notification resent after reinitialization");
                }
                catch (Exception retryEx)
                {
                    Console.WriteLine($"Retry also failed: {retryEx.Message}");
                }
            }
        }

        public void Unregister()
        {
            try
            {
                Console.WriteLine("Unregistering Windows notification manager");
                AppNotificationManager.Default.Unregister();
                Console.WriteLine("Windows notification manager unregistered successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error unregistering Windows notification manager: {ex.Message}");
            }
        }
    }
}
#endif
