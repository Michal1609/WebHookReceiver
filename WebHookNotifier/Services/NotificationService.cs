using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using WebHookNotifier.Models;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace WebHookNotifier.Services
{
    public class NotificationService
    {
        private readonly SignalRService _signalRService;

        // Rate limiting settings
        private TimeSpan _minTimeBetweenNotifications => TimeSpan.FromSeconds(NotificationSettings.Instance.MinSecondsBetweenNotifications);
        private int _maxQueuedNotifications => NotificationSettings.Instance.MaxQueuedNotifications;
        private readonly SemaphoreSlim _notificationSemaphore = new SemaphoreSlim(1, 1);
        private readonly Queue<WebhookData> _notificationQueue = new Queue<WebhookData>();
        private DateTime _lastNotificationTime = DateTime.MinValue;
        private bool _processingQueue = false;

        public NotificationService(string hubUrl)
        {
            _signalRService = new SignalRService(hubUrl);
            _signalRService.NotificationReceived += OnNotificationReceived;
        }

        private async void OnNotificationReceived(object? sender, WebhookData data)
        {
            await _notificationSemaphore.WaitAsync();
            try
            {
                // Add to queue
                _notificationQueue.Enqueue(data);

                // Trim queue if it gets too large
                while (_notificationQueue.Count > _maxQueuedNotifications)
                {
                    _notificationQueue.Dequeue();
                }

                // Start processing if not already processing
                if (!_processingQueue)
                {
                    _processingQueue = true;
                    _ = Task.Run(ProcessNotificationQueueAsync);
                }
            }
            finally
            {
                _notificationSemaphore.Release();
            }
        }

        private async Task ProcessNotificationQueueAsync()
        {
            while (true)
            {
                await _notificationSemaphore.WaitAsync();
                WebhookData? data = null;
                bool hasMoreItems = false;

                try
                {
                    if (_notificationQueue.Count == 0)
                    {
                        _processingQueue = false;
                        return;
                    }

                    // Get the next notification
                    data = _notificationQueue.Dequeue();
                    hasMoreItems = _notificationQueue.Count > 0;
                }
                finally
                {
                    _notificationSemaphore.Release();
                }

                if (data != null)
                {
                    // Calculate time to wait before showing next notification
                    TimeSpan timeSinceLastNotification = DateTime.Now - _lastNotificationTime;
                    TimeSpan timeToWait = _minTimeBetweenNotifications - timeSinceLastNotification;

                    if (timeToWait > TimeSpan.Zero)
                    {
                        await Task.Delay(timeToWait);
                    }

                    // Show notification on UI thread
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        ShowNotification(data, hasMoreItems);
                    });

                    _lastNotificationTime = DateTime.Now;
                }
            }
        }

        private void ShowNotification(WebhookData data, bool hasMoreItems = false)
        {
            // Create notification text
            string title = $"Webhook: {data.Event}";
            string message = data.Message ?? "No message";

            // Add indicator if there are more notifications in queue
            if (hasMoreItems)
            {
                title += " (+more)";
            }

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
