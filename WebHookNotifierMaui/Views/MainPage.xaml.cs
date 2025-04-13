using System;
using WebHookNotifierMaui.Models;
using WebHookNotifierMaui.Services;
using WebHookNotifierMaui.Data;
#if ANDROID
using WebHookNotifierMaui.Platforms.Android;
#endif

namespace WebHookNotifierMaui.Views
{
    public partial class MainPage : ContentPage
    {
        private NotificationService? _notificationService;
        private DatabaseService? _databaseService;
        private NotificationHistoryService? _historyService;
        private string _apiUrl;
        private string _signalRKey;
        private bool _useDirectWebSockets;

        public string ApiUrl
        {
            get => _apiUrl;
            set
            {
                _apiUrl = value;
                OnPropertyChanged();
            }
        }

        public string SignalRKey
        {
            get => _signalRKey;
            set
            {
                _signalRKey = value;
                OnPropertyChanged();
            }
        }

        public bool IsAndroid => DeviceInfo.Platform == DevicePlatform.Android;

        public bool UseDirectWebSockets
        {
            get => _useDirectWebSockets;
            set
            {
                _useDirectWebSockets = value;
                NotificationSettings.Instance.UseDirectWebSocketsOnAndroid = value;
                NotificationSettings.Instance.Save();
                OnPropertyChanged();
            }
        }

        public MainPage()
        {
            InitializeComponent();

            // Inicializovat vlastnosti z nastavení
            _apiUrl = NotificationSettings.Instance.ApiUrl;
            _signalRKey = NotificationSettings.Instance.SignalRKey;
            _useDirectWebSockets = NotificationSettings.Instance.UseDirectWebSocketsOnAndroid;

            // Nastavit binding context
            BindingContext = this;

            // Initialize database services
            InitializeDatabaseServices();

            // Zobrazit informaci o připojení pro Android zařízení
            if (DeviceInfo.Platform == DevicePlatform.Android)
            {
                ShowAndroidConnectionInfo();
            }

            // Přidat handler pro změnu nastavení WebSockets
            if (UseDirectWebSocketsCheckbox != null)
            {
                UseDirectWebSocketsCheckbox.CheckedChanged += (s, e) => {
                    UseDirectWebSockets = UseDirectWebSocketsCheckbox.IsChecked;
                };
            }
        }

        private async void ShowAndroidConnectionInfo()
        {
            try
            {
                // Získat IP adresu počítače pro připojení z fyzického zařízení
                string message = "Pro připojení z Android zařízení:\n\n";

                // Pro emulátor
                message += "1. Pro Android emulátor: Použijte '10.0.2.2' místo 'localhost'\n";
                message += "   (aplikace to udělá automaticky)\n\n";

                // Pro fyzické zařízení
                message += "2. Pro fyzické zařízení: Použijte IP adresu vašeho počítače v lokální síti\n";
                message += "   a ujistěte se, že vaše zařízení je ve stejné síti jako počítač.\n\n";
                message += "3. Ujistěte se, že server je dostupný z vašeho zařízení\n";
                message += "   (firewall, port forwarding, atd.).";

                await DisplayAlert("Informace o připojení pro Android", message, "OK");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error showing Android connection info: {ex.Message}");
            }
        }

        private void InitializeDatabaseServices()
        {
            try
            {
                // Initialize database service
                _databaseService = new DatabaseService(NotificationSettings.Instance);
                var dbContext = _databaseService.InitializeDatabase();

                // Initialize history service
                _historyService = new NotificationHistoryService(dbContext, NotificationSettings.Instance);
            }
            catch (Exception ex)
            {
                DisplayAlert("Database Error", $"Error initializing database: {ex.Message}", "OK");
            }
        }

        private async void ConnectButton_Clicked(object sender, EventArgs e)
        {
            string hubUrl = ServerUrlEntry.Text.Trim();
            if (string.IsNullOrEmpty(hubUrl))
            {
                await DisplayAlert("Error", "Enter server URL", "OK");
                return;
            }

            string signalRKey = SignalRKeyEntry.Text.Trim();
            if (string.IsNullOrEmpty(signalRKey))
            {
                await DisplayAlert("Error", "Please enter a valid SignalR key", "OK");
                return;
            }

            try
            {
                // Upravit URL pro Android, pokud je to potřeba
                if (DeviceInfo.Platform == DevicePlatform.Android && hubUrl.Contains("localhost"))
                {
                    string originalUrl = hubUrl;
                    hubUrl = hubUrl.Replace("localhost", "10.0.2.2");
                    Console.WriteLine($"Upravena URL pro Android: {originalUrl} -> {hubUrl}");

                    // Informovat uživatele o změně URL
                    await DisplayAlert("URL Upravena pro Android",
                        $"Pro Android zařízení byla URL upravena z '{originalUrl}' na '{hubUrl}', protože 'localhost' na Android zařízení odkazuje na samotné zařízení, ne na váš počítač.",
                        "OK");
                }

                // Update UI to show connecting state
                StatusLabel.Text = $"Connecting to {hubUrl}...";
                ConnectButton.IsEnabled = false;

                Console.WriteLine($"Connecting to hub at {hubUrl}");

                // Save the URL and key to settings
                NotificationSettings.Instance.ApiUrl = hubUrl;
                NotificationSettings.Instance.SignalRKey = signalRKey;
                NotificationSettings.Instance.Save();
                Console.WriteLine("Saved URL and SignalR key to settings");

                // Create and start notification service
                _notificationService = new NotificationService(hubUrl, _historyService);
                _notificationService.NotificationReady += OnNotificationReady;
                Console.WriteLine("Created notification service and registered event handler");

                await _notificationService.StartAsync();
                Console.WriteLine("Notification service started");

                // Check if connection was successful
                if (_notificationService.IsConnected)
                {
                    StatusLabel.Text = $"Connected to {hubUrl}";
                    DisconnectButton.IsEnabled = true;

                    // Show a test notification to confirm everything is working
                    await DisplayAlert("Connection Successful",
                        "Connected to the notification hub. You should now receive notifications when events occur.",
                        "OK");
                }
                else
                {
                    StatusLabel.Text = "Failed to connect";
                    ConnectButton.IsEnabled = true;
                    await DisplayAlert("Connection Warning",
                        "Connection attempt completed but the connection state is disconnected. Check the server URL and try again.",
                        "OK");
                }
            }
            catch (Exception ex)
            {
                StatusLabel.Text = "Connection failed";
                ConnectButton.IsEnabled = true;
                Console.WriteLine($"Connection error: {ex.Message}");
                await DisplayAlert("Connection Error", $"Error connecting to server: {ex.Message}", "OK");
            }
        }

        private async void DisconnectButton_Clicked(object sender, EventArgs e)
        {
            if (_notificationService != null)
            {
                try
                {
                    // Update UI
                    StatusLabel.Text = "Disconnecting...";
                    DisconnectButton.IsEnabled = false;

                    Console.WriteLine("Disconnecting from notification service");

                    // Stop the service
                    await _notificationService.StopAsync();
                    _notificationService.NotificationReady -= OnNotificationReady;
                    _notificationService = null;

                    Console.WriteLine("Successfully disconnected");
                    StatusLabel.Text = "Disconnected";
                    ConnectButton.IsEnabled = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error disconnecting: {ex.Message}");
                    StatusLabel.Text = "Disconnection error";
                    ConnectButton.IsEnabled = true;
                    DisconnectButton.IsEnabled = false;
                    await DisplayAlert("Disconnection Error", $"Error disconnecting from server: {ex.Message}", "OK");
                }
            }
        }

        private async void HistoryButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (_historyService == null)
                {
                    await DisplayAlert("Error", "History service is not available.", "OK");
                    return;
                }

                // Navigate to history page
                await Navigation.PushAsync(new HistoryPage(_historyService));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error showing history: {ex.Message}", "OK");
            }
        }

        private async void SettingsButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                // Navigate to settings page
                await Navigation.PushAsync(new SettingsPage());
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error showing settings: {ex.Message}", "OK");
            }
        }

        private void TestButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                // Create a test notification
                var testData = new WebhookData
                {
                    Id = Guid.NewGuid().ToString(),
                    Event = "Test Event",
                    Message = "This is a test notification from the app.",
                    Timestamp = DateTime.Now,
                    AdditionalData = new Dictionary<string, object>
                    {
                        { "Source", "Test Button" },
                        { "Priority", "High" }
                    }
                };

                // Display the notification
                OnNotificationReady(this, testData);

                // Save to history if available
                if (_historyService != null && NotificationSettings.Instance.EnableHistoryTracking)
                {
                    _historyService.AddNotificationAsync(testData).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                DisplayAlert("Error", $"Error creating test notification: {ex.Message}", "OK");
            }
        }

        private void OnNotificationReady(object? sender, WebhookData data)
        {
            try
            {
                Console.WriteLine($"OnNotificationReady called with data: Event={data.Event}, ID={data.Id}");

                // Update the UI with the notification
                string title = $"Webhook: {data.Event}";
                string message = data.Message ?? "No message";

                // Update last notification text
                LastNotificationText.Text = $"{title}: {message}";
                Console.WriteLine("Updated LastNotificationText");

                // Show platform notification
                ShowPlatformNotification(title, message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in OnNotificationReady: {ex.Message}");
                DisplayAlert("Notification Error", $"Error displaying notification: {ex.Message}", "OK");
            }
        }

        private void ShowPlatformNotification(string title, string message)
        {
            try
            {
                Console.WriteLine($"ShowPlatformNotification called with title: {title}");

                // Format message for better display
                string formattedMessage = FormatNotificationMessage(message);

                // Set maximum length for notification
                string shortMessage = formattedMessage;
                if (shortMessage.Length > 800)
                {
                    shortMessage = shortMessage.Substring(0, 797) + "...";
                }

                // Show platform notification
#if ANDROID
                // Android notification
                Console.WriteLine("Sending Android notification");
                AndroidNotificationManager.Instance.SendNotification(title, shortMessage);
#elif WINDOWS
                // Windows notification
                Console.WriteLine("Sending Windows notification");
                WebHookNotifierMaui.Platforms.Windows.WindowsNotificationManager.Instance.SendNotification(title, shortMessage);
#else
                // No platform-specific notification available
                Console.WriteLine("No platform-specific notification manager available for this platform");
#endif
                Console.WriteLine("Platform notification sent successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error displaying notification: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        // Format notification message for better display
        private static string FormatNotificationMessage(string message)
        {
            // Process JSON data to make it more readable
            if (message.Contains("{") && message.Contains("}"))
            {
                try
                {
                    // Try to format JSON more nicely
                    message = message.Replace("{\":", "{\r\n  \"")
                                   .Replace(",\":", ",\r\n  \"")
                                   .Replace("}", "\r\n}");
                }
                catch
                {
                    // If formatting fails, continue with original message
                }
            }

            // Break long lines for better readability
            if (message.Length > 40)
            {
                // Insert line breaks at logical points (after punctuation or spaces)
                var result = new System.Text.StringBuilder();
                int lineLength = 0;

                for (int i = 0; i < message.Length; i++)
                {
                    char c = message[i];
                    result.Append(c);
                    lineLength++;

                    // If we've reached a good breaking point and the line is getting long
                    if (lineLength > 40 && (c == ' ' || c == '.' || c == ',' || c == ';' || c == ':'))
                    {
                        result.Append(Environment.NewLine);
                        lineLength = 0;
                    }
                }

                return result.ToString();
            }

            return message;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            // Disconnect when page is closed
            if (_notificationService != null)
            {
                _notificationService.StopAsync().ConfigureAwait(false);
            }
        }
    }
}
