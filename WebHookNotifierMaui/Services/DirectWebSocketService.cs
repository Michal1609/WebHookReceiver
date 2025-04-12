using System;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using WebHookNotifierMaui.Models;

namespace WebHookNotifierMaui.Services
{
    /// <summary>
    /// Alternativní implementace pro Android, která používá nativní WebSockets místo SignalR
    /// </summary>
    public class DirectWebSocketService
    {
        private ClientWebSocket _webSocket;
        private readonly string _webSocketUrl;
        private bool _isConnected;
        private CancellationTokenSource _cts;

        public event EventHandler<WebhookData> NotificationReceived;

        public DirectWebSocketService(string hubUrl)
        {
            // Převést SignalR hub URL na WebSocket URL
            _webSocketUrl = hubUrl.Replace("http://", "ws://").Replace("https://", "wss://");
            _isConnected = false;
            _cts = new CancellationTokenSource();
        }

        public async Task StartAsync()
        {
            try
            {
                Console.WriteLine($"Starting direct WebSocket connection to {_webSocketUrl}");
                
                _webSocket = new ClientWebSocket();
                
                // Nastavit timeout
                _webSocket.Options.SetRequestHeader("Connection", "keep-alive");
                
                // Připojit se k WebSocket serveru
                await _webSocket.ConnectAsync(new Uri(_webSocketUrl), _cts.Token);
                _isConnected = true;
                
                Console.WriteLine("WebSocket connected successfully");
                
                // Spustit naslouchání zpráv
                _ = Task.Run(ReceiveMessagesAsync);
            }
            catch (Exception ex)
            {
                _isConnected = false;
                Console.WriteLine($"Error connecting WebSocket: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                
                throw;
            }
        }

        private async Task ReceiveMessagesAsync()
        {
            var buffer = new byte[4096];
            
            try
            {
                while (_webSocket.State == WebSocketState.Open && !_cts.Token.IsCancellationRequested)
                {
                    var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), _cts.Token);
                    
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        Console.WriteLine("WebSocket connection closed by server");
                        _isConnected = false;
                        break;
                    }
                    
                    // Zpracovat přijatou zprávu
                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        Console.WriteLine($"Received WebSocket message: {message}");
                        
                        try
                        {
                            // Zkusit deserializovat jako WebhookData
                            var data = JsonSerializer.Deserialize<WebhookData>(message);
                            
                            if (data != null)
                            {
                                Console.WriteLine($"Deserialized WebSocket message as WebhookData: {data.Event}");
                                NotificationReceived?.Invoke(this, data);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error deserializing WebSocket message: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error receiving WebSocket messages: {ex.Message}");
                _isConnected = false;
            }
        }

        public async Task StopAsync()
        {
            try
            {
                if (_webSocket != null && _webSocket.State == WebSocketState.Open)
                {
                    Console.WriteLine("Closing WebSocket connection");
                    
                    // Zrušit token pro ukončení naslouchání
                    _cts.Cancel();
                    
                    // Zavřít WebSocket spojení
                    await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client closing", CancellationToken.None);
                    _webSocket.Dispose();
                    _webSocket = null;
                    
                    Console.WriteLine("WebSocket connection closed successfully");
                }
                
                _isConnected = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error closing WebSocket: {ex.Message}");
            }
        }

        public bool IsConnected => _isConnected && _webSocket?.State == WebSocketState.Open;
    }
}
