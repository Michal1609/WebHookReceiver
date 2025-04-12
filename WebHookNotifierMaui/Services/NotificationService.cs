using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WebHookNotifierMaui.Models;

namespace WebHookNotifierMaui.Services
{
    /// <summary>
    /// Service for handling notifications
    /// </summary>
    public class NotificationService
    {
        private readonly SignalRService? _signalRService;
        private readonly DirectWebSocketService? _webSocketService;
        private readonly NotificationHistoryService? _historyService;
        private readonly bool _useDirectWebSockets;

        // Rate limiting settings
        private TimeSpan _minTimeBetweenNotifications => TimeSpan.FromSeconds(NotificationSettings.Instance.MinSecondsBetweenNotifications);
        private int _maxQueuedNotifications => NotificationSettings.Instance.MaxQueuedNotifications;
        private readonly SemaphoreSlim _notificationSemaphore = new SemaphoreSlim(1, 1);
        private readonly Queue<WebhookData> _notificationQueue = new Queue<WebhookData>();
        private DateTime _lastNotificationTime = DateTime.MinValue;
        private bool _processingQueue = false;

        // Event for when a notification is ready to be displayed
        public event EventHandler<WebhookData>? NotificationReady;

        public NotificationService(string hubUrl, NotificationHistoryService? historyService = null)
        {
            _historyService = historyService;

            // Na Androidu zkusit použít přímé WebSockets, pokud je to povoleno v nastavení
            _useDirectWebSockets = DeviceInfo.Platform == DevicePlatform.Android &&
                                   NotificationSettings.Instance.UseDirectWebSocketsOnAndroid;

            if (_useDirectWebSockets)
            {
                Console.WriteLine("Using direct WebSockets for Android");
                _webSocketService = new DirectWebSocketService(hubUrl);
                _webSocketService.NotificationReceived += OnNotificationReceived;
            }
            else
            {
                Console.WriteLine("Using SignalR service");
                _signalRService = new SignalRService(hubUrl);
                _signalRService.NotificationReceived += OnNotificationReceived;
            }
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

                    // Trigger notification event on main thread
                    await MainThread.InvokeOnMainThreadAsync(() =>
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

            // Trigger the notification event
            NotificationReady?.Invoke(this, data);
        }

        public async Task StartAsync()
        {
            try
            {
                Console.WriteLine("Starting notification service");
                if (_useDirectWebSockets && _webSocketService != null)
                {
                    await _webSocketService.StartAsync();
                }
                else if (_signalRService != null)
                {
                    await _signalRService.StartAsync();
                }
                else
                {
                    throw new InvalidOperationException("No communication service available");
                }
                Console.WriteLine("Notification service started successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error starting notification service: {ex.Message}");
                throw;
            }
        }

        public async Task StopAsync()
        {
            try
            {
                Console.WriteLine("Stopping notification service");
                if (_useDirectWebSockets && _webSocketService != null)
                {
                    await _webSocketService.StopAsync();
                }
                else if (_signalRService != null)
                {
                    await _signalRService.StopAsync();
                }
                Console.WriteLine("Notification service stopped successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error stopping notification service: {ex.Message}");
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

        public bool IsConnected => _useDirectWebSockets ?
            (_webSocketService?.IsConnected ?? false) :
            (_signalRService?.IsConnected ?? false);
    }
}
