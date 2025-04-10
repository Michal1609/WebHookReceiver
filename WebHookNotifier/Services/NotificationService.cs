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
            // Zajistíme, že notifikace se zobrazí v UI vlákně
            Application.Current.Dispatcher.Invoke(() =>
            {
                ShowNotification(data);
            });
        }

        private void ShowNotification(WebhookData data)
        {
            // Vytvoření textu notifikace
            string title = $"Webhook: {data.Event}";
            string message = data.Message ?? "Bez zprávy";

            if (data.AdditionalData != null && data.AdditionalData.Count > 0)
            {
                message += "\n\nDodatečná data:";
                foreach (var item in data.AdditionalData)
                {
                    message += $"\n{item.Key}: {item.Value}";
                }
            }

            // Zobrazení notifikace pomocí systémové lišty
            MainWindow.ShowBalloonTip(title, message, Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Info);

            // Záložní metoda pro zobrazení notifikace - MessageBox pro testování
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
