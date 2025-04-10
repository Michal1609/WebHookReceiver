using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;
using WebHookNotifier.Models;

namespace WebHookNotifier.Services
{
    public class SignalRService
    {
        private HubConnection? _hubConnection;
        private readonly string _hubUrl;
        private bool _isConnected;

        public event EventHandler<WebhookData>? NotificationReceived;

        public SignalRService(string hubUrl)
        {
            _hubUrl = hubUrl;
            _isConnected = false;
        }

        public async Task StartAsync()
        {
            if (_hubConnection == null)
            {
                _hubConnection = new HubConnectionBuilder()
                    .WithUrl(_hubUrl)
                    .WithAutomaticReconnect()
                    .Build();

                _hubConnection.On<WebhookData>("ReceiveNotification", (data) =>
                {
                    NotificationReceived?.Invoke(this, data);
                });

                _hubConnection.Closed += async (error) =>
                {
                    _isConnected = false;
                    await Task.Delay(new Random().Next(0, 5) * 1000);
                    await ConnectAsync();
                };
            }

            await ConnectAsync();
        }

        private async Task ConnectAsync()
        {
            if (_hubConnection == null)
                return;

            try
            {
                await _hubConnection.StartAsync();
                _isConnected = true;
            }
            catch (Exception ex)
            {
                _isConnected = false;
                Console.WriteLine($"Error connecting to hub: {ex.Message}");
            }
        }

        public async Task StopAsync()
        {
            if (_hubConnection != null)
            {
                await _hubConnection.StopAsync();
                await _hubConnection.DisposeAsync();
                _hubConnection = null;
                _isConnected = false;
            }
        }

        public bool IsConnected => _isConnected;
    }
}
