using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using WebHookNotifier.Data;
using WebHookNotifier.Models;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace WebHookNotifier.Services
{
    public class NotificationService
    {
        private readonly SignalRService _signalRService;
        private readonly NotificationHistoryService? _historyService;

        // Rate limiting settings
        private TimeSpan _minTimeBetweenNotifications => TimeSpan.FromSeconds(NotificationSettings.Instance.MinSecondsBetweenNotifications);
        private int _maxQueuedNotifications => NotificationSettings.Instance.MaxQueuedNotifications;
        private readonly SemaphoreSlim _notificationSemaphore = new SemaphoreSlim(1, 1);
        private readonly Queue<WebhookData> _notificationQueue = new Queue<WebhookData>();
        private DateTime _lastNotificationTime = DateTime.MinValue;
        private bool _processingQueue = false;

        public NotificationService(string hubUrl, NotificationHistoryService? historyService = null)
        {
            _signalRService = new SignalRService(hubUrl);
            _signalRService.NotificationReceived += OnNotificationReceived;
            _historyService = historyService;
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

                    // Save to history if enabled
                    await SaveToHistoryAsync(data);
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

        public async Task<bool> Start()
        {
            bool result = await _signalRService.StartAsync();
            return result;
        }

        public async Task Stop()
        {
            try
            {
                await _signalRService.StopAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error stopping SignalR service: {ex.Message}");
                throw; // Re-throw to be handled by the caller
            }
        }

        /// <summary>
        /// Saves a notification to the history database if history service is available
        /// </summary>
        private async Task SaveToHistoryAsync(WebhookData data)
        {
            if (_historyService != null && NotificationSettings.Instance.EnableHistoryTracking)
            {
                try
                {
                    await _historyService.AddNotificationAsync(data);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error saving notification to history: {ex.Message}");
                }
            }
        }
    }
}
