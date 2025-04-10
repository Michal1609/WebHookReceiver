using System;
using System.Windows;
using WebHookNotifier.Models;

namespace WebHookNotifier.Services
{
    public class NotificationService
    {
        private readonly SignalRService _signalRService;

        public NotificationService(string hubUrl)
        {
            _signalRService = new SignalRService(hubUrl);
            _signalRService.NotificationReceived += OnNotificationReceived;
        }

        private void OnNotificationReceived(object? sender, WebhookData data)
        {
            // Ensure notification is displayed in UI thread
            Application.Current.Dispatcher.Invoke(() =>
            {
                ShowNotification(data);
            });
        }

        private void ShowNotification(WebhookData data)
        {
            // Create notification text
            string title = $"Webhook: {data.Event}";
            string message = data.Message ?? "No message";

            if (data.AdditionalData != null && data.AdditionalData.Count > 0)
            {
                message += "\n\nAdditional data:";
                foreach (var item in data.AdditionalData)
                {
                    message += $"\n{item.Key}: {item.Value}";
                }
            }

            // Display notification using system tray
            MainWindow.ShowBalloonTip(title, message, Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Info);

            // Fallback method for displaying notification - MessageBox for testing
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public async void Start()
        {
            await _signalRService.StartAsync();
        }

        public async void Stop()
        {
            await _signalRService.StopAsync();
        }
    }
}
