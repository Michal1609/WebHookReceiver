using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Text.Json;
using System.Threading;
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

        public async Task<bool> StartAsync()
        {
            if (_hubConnection == null)
            {
                // Get SignalR key from settings
                string signalRKey = NotificationSettings.Instance.SignalRKey;

                // Build connection with authentication
                // Add SignalR key as query parameter
                string hubUrlWithAuth = _hubUrl;
                if (_hubUrl.Contains("?"))
                {
                    hubUrlWithAuth += $"&signalRKey={signalRKey}";
                }
                else
                {
                    hubUrlWithAuth += $"?signalRKey={signalRKey}";
                }

                _hubConnection = new HubConnectionBuilder()
                    .WithUrl(hubUrlWithAuth)
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

            return await ConnectAsync();
        }

        private async Task<bool> ConnectAsync()
        {
            if (_hubConnection == null)
                return false;

            try
            {
                // Nastavíme timeout pro připojení
                var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));

                // Pokusíme se připojit
                await _hubConnection.StartAsync(cancellationTokenSource.Token);

                // Počkáme chvíli a zkontrolujeme, zda jsme stále připojení
                // Toto je potřeba, protože server může odmítnout připojení kvůli neplatnému klíči
                await Task.Delay(500); // Počkáme 500ms

                // Zkontrolujeme stav připojení
                var state = _hubConnection.State;
                _isConnected = (state == HubConnectionState.Connected);

                if (!_isConnected)
                {
                    Console.WriteLine($"Connection failed. State: {state}");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _isConnected = false;
                Console.WriteLine($"Error connecting to hub: {ex.Message}");
                throw; // Re-throw the exception to be caught by the caller
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
