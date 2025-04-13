using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using WebHookNotifierMaui.Models;
using WebHookNotifierMaui.Security;

namespace WebHookNotifierMaui.Services
{
    /// <summary>
    /// Service for SignalR communication with the API
    /// </summary>
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
                Console.WriteLine($"Creating new hub connection to {_hubUrl}");
                Console.WriteLine($"Configuring SignalR connection for platform: {DeviceInfo.Platform}");

                // Get SignalR key from settings
                string signalRKey = NotificationSettings.Instance.SignalRKey;
                Console.WriteLine($"Using SignalR key for authentication: {signalRKey.Substring(0, 3)}...");

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
                Console.WriteLine($"Connecting to hub with auth: {hubUrlWithAuth}");

                var builder = new HubConnectionBuilder()
                    .WithUrl(hubUrlWithAuth, options => {
                        // Nastavit timeout na delší dobu
                        options.HttpMessageHandlerFactory = (handler) => {
                            if (handler is HttpClientHandler clientHandler)
                            {
                                // Ignorovat SSL certifikáty pro testování (POUZE PRO VÝVOJ!)
                                #if DEBUG
                                clientHandler.ServerCertificateCustomValidationCallback =
                                    (sender, cert, chain, sslPolicyErrors) => { return true; };
                                #endif

                                // Nastavit timeout
                                clientHandler.MaxConnectionsPerServer = 10;

                                // Speciální nastavení pro Android
                                if (DeviceInfo.Platform == DevicePlatform.Android)
                                {
                                    Console.WriteLine("Applying Android-specific settings");
                                    // Použít moderní TLS
                                    clientHandler.SslProtocols = System.Security.Authentication.SslProtocols.Tls12 |
                                                                System.Security.Authentication.SslProtocols.Tls13;

                                    // Povolit všechny redirecty
                                    clientHandler.AllowAutoRedirect = true;
                                    clientHandler.MaxAutomaticRedirections = 10;
                                }
                            }
                            return handler;
                        };

                        // Nastavit timeout pro transport
                        options.TransportMaxBufferSize = 1024 * 1024; // 1MB
                        options.ClientCertificates = new System.Security.Cryptography.X509Certificates.X509CertificateCollection();

                        // Speciální nastavení pro Android
                        if (DeviceInfo.Platform == DevicePlatform.Android)
                        {
                            // Použít WebSockets jako primární transport
                            options.Transports = Microsoft.AspNetCore.Http.Connections.HttpTransportType.WebSockets;
                            options.SkipNegotiation = true;
                            Console.WriteLine("Configured Android to use WebSockets transport");
                        }

                        // Logování
                        Console.WriteLine($"Configuring SignalR transport with URL: {_hubUrl}");
                    })
                    .WithAutomaticReconnect(new[] { TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(2),
                                                TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10) })
                    .Build();

                _hubConnection = builder;

                // Log connection state changes
                _hubConnection.Reconnecting += error =>
                {
                    Console.WriteLine($"SignalR reconnecting due to error: {error?.Message}");
                    return Task.CompletedTask;
                };

                _hubConnection.Reconnected += connectionId =>
                {
                    Console.WriteLine($"SignalR reconnected with connection ID: {connectionId}");
                    _isConnected = true;
                    return Task.CompletedTask;
                };

                _hubConnection.On<string>("ReceiveNotification", (encryptedData) =>
                {
                    try
                    {
                        Console.WriteLine($"Received notification data: {encryptedData.Substring(0, Math.Min(50, encryptedData.Length))}...");
                        WebhookData? data;

                        // Check if encryption is enabled in settings
                        if (NotificationSettings.Instance.EnableEncryption)
                        {
                            Console.WriteLine("Encryption enabled, attempting to decrypt");
                            // Decrypt the received data
                            string decryptedJson = EncryptionService.Decrypt(encryptedData);
                            Console.WriteLine($"Decrypted JSON: {decryptedJson.Substring(0, Math.Min(50, decryptedJson.Length))}...");

                            // Deserialize the JSON data
                            data = JsonSerializer.Deserialize<WebhookData>(decryptedJson);
                        }
                        else
                        {
                            Console.WriteLine("Encryption disabled, attempting direct deserialization");
                            // If encryption is disabled, try to deserialize directly
                            try
                            {
                                data = JsonSerializer.Deserialize<WebhookData>(encryptedData);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Direct deserialization failed: {ex.Message}, trying decryption as fallback");
                                // If direct deserialization fails, try decryption as fallback
                                string decryptedJson = EncryptionService.Decrypt(encryptedData);
                                data = JsonSerializer.Deserialize<WebhookData>(decryptedJson);
                            }
                        }

                        if (data != null)
                        {
                            Console.WriteLine($"Successfully processed notification: {data.Event}, ID: {data.Id}");
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
                        Console.WriteLine($"Stack trace: {ex.StackTrace}");
                    }
                });

                _hubConnection.Closed += async (error) =>
                {
                    _isConnected = false;
                    Console.WriteLine($"SignalR connection closed with error: {error?.Message}");
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
                Console.WriteLine($"Attempting to connect to SignalR hub at {_hubUrl} on platform {DeviceInfo.Platform}");
                Console.WriteLine($"Current connection state: {_hubConnection.State}");

                // Zkontrolovat dostupnost serveru před připojením
                bool isServerReachable = await CheckServerReachableAsync(_hubUrl);
                if (!isServerReachable)
                {
                    Console.WriteLine($"WARNING: Server at {_hubUrl} appears to be unreachable");
                }

                // Pokusit se o připojení s timeout
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30)); // 30 sekund timeout
                await _hubConnection.StartAsync(cts.Token);

                _isConnected = true;
                Console.WriteLine($"Successfully connected to SignalR hub. State: {_hubConnection.State}");

                // Otestovat připojení
                if (_hubConnection.State == HubConnectionState.Connected)
                {
                    try
                    {
                        // Zkusit zavolat metodu na serveru, pokud existuje
                        // await _hubConnection.InvokeAsync("Ping", cts.Token);
                        // Console.WriteLine("Successfully pinged the server");
                    }
                    catch (Exception pingEx)
                    {
                        Console.WriteLine($"Ping test failed: {pingEx.Message}");
                    }
                }
            }
            catch (TaskCanceledException)
            {
                _isConnected = false;
                Console.WriteLine("Connection attempt timed out");
            }
            catch (Exception ex)
            {
                _isConnected = false;
                Console.WriteLine($"Error connecting to hub: {ex.Message}");
                Console.WriteLine($"Connection error details: {ex.GetType().Name}, Stack trace: {ex.StackTrace}");

                // Check for common connection issues
                if (ex.Message.Contains("Failed to start the connection") ||
                    ex.Message.Contains("Connection was stopped during initialization"))
                {
                    Console.WriteLine("Possible causes: Server not running, incorrect URL, or network issues");
                }
                else if (ex.Message.Contains("The server returned status code '404'"))
                {
                    Console.WriteLine("Hub endpoint not found - check that the URL path is correct");
                }
                else if (ex.Message.Contains("The server returned status code '401'") ||
                         ex.Message.Contains("The server returned status code '403'"))
                {
                    Console.WriteLine("Authentication or authorization error - check credentials");
                }
                else if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                    Console.WriteLine($"Inner exception type: {ex.InnerException.GetType().Name}");
                }
            }
        }

        private async Task<bool> CheckServerReachableAsync(string url)
        {
            try
            {
                // Extrahovat základní URL bez cesty k hubu
                var uri = new Uri(url);
                string baseUrl = $"{uri.Scheme}://{uri.Authority}";

                Console.WriteLine($"Checking if server is reachable at {baseUrl}");

                #if DEBUG
                // Ignorovat SSL certifikáty pro testování
                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
                };
                using (var client = new HttpClient(handler))
                #else
                using (var client = new HttpClient())
                #endif
                {
                    client.Timeout = TimeSpan.FromSeconds(10);
                    client.DefaultRequestHeaders.ConnectionClose = true;

                    // Zkusit získat odpověď ze serveru
                    var response = await client.GetAsync(baseUrl);
                    Console.WriteLine($"Server response: {response.StatusCode}");

                    // I když dostaneme chybu 404, server je dostupný
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Server reachability check failed: {ex.Message}");
                return false;
            }
        }

        public async Task StopAsync()
        {
            if (_hubConnection != null)
            {
                try
                {
                    Console.WriteLine("Stopping SignalR connection");
                    await _hubConnection.StopAsync();
                    await _hubConnection.DisposeAsync();
                    _hubConnection = null;
                    _isConnected = false;
                    Console.WriteLine("SignalR connection stopped successfully");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error stopping SignalR connection: {ex.Message}");
                    // Force disconnect
                    _hubConnection = null;
                    _isConnected = false;
                }
            }
        }

        public bool IsConnected => _isConnected;
    }
}
