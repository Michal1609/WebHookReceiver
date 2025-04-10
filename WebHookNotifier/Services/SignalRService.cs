using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using WebHookNotifier.Models;
using WebHookNotifier.Security;

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

                _hubConnection.On<string>("ReceiveNotification", (encryptedData) =>
                {
                    try
                    {
                        WebhookData? data;

                        // Check if encryption is enabled in settings
                        if (NotificationSettings.Instance.EnableEncryption)
                        {
                            // Decrypt the received data
                            string decryptedJson = EncryptionService.Decrypt(encryptedData);

                            // Deserialize the JSON data
                            data = JsonSerializer.Deserialize<WebhookData>(decryptedJson);
                        }
                        else
                        {
                            // If encryption is disabled, try to deserialize directly
                            try
                            {
                                data = JsonSerializer.Deserialize<WebhookData>(encryptedData);
                            }
                            catch
                            {
                                // If direct deserialization fails, try decryption as fallback
                                string decryptedJson = EncryptionService.Decrypt(encryptedData);
                                data = JsonSerializer.Deserialize<WebhookData>(decryptedJson);
                            }
                        }

                        if (data != null)
                        {
                            NotificationReceived?.Invoke(this, data);
                        }
                        else
                        {
                            Console.WriteLine("Error: Received null data after deserialization");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing notification: {ex.Message}");
                    }
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
