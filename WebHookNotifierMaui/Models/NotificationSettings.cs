using System;
using System.IO;
using System.Text.Json;
using WebHookNotifierMaui.Security;

namespace WebHookNotifierMaui.Models
{
    /// <summary>
    /// Manages application settings for notifications
    /// </summary>
    public class NotificationSettings
    {
        // Rate limiting settings with default values
        public int MinSecondsBetweenNotifications { get; set; } = 2;
        public int MaxQueuedNotifications { get; set; } = 5;
        public bool EnableNotificationSounds { get; set; } = true;

        // Security settings
        public bool EnableEncryption { get; set; } = true;

        // Android settings
        public bool UseDirectWebSocketsOnAndroid { get; set; } = true;

        // History settings
        public bool EnableHistoryTracking { get; set; } = true;
        public int HistoryRetentionDays { get; set; } = 30;

        // API settings
        private string _apiUrl = "http://localhost:5017/notificationHub";
        public string ApiUrl
        {
            get
            {
                // Pokud je to Android emulátor, nahradit localhost za 10.0.2.2
                if (DeviceInfo.Platform == DevicePlatform.Android && _apiUrl.Contains("localhost"))
                {
                    return _apiUrl.Replace("localhost", "10.0.2.2");
                }
                return _apiUrl;
            }
            set
            {
                _apiUrl = value;
            }
        }

        // SignalR authentication key
        public string SignalRKey { get; set; } = "signalr-connection-key-2025";

        // Database settings
        public DatabaseType DatabaseType { get; set; } = DatabaseType.SQLite;
        private string _encryptedSqlServerConnectionString = string.Empty;

        // Property to safely get/set the encrypted connection string
        public string SqlServerConnectionString
        {
            get
            {
                if (string.IsNullOrEmpty(_encryptedSqlServerConnectionString))
                    return string.Empty;

                return ConnectionStringProtection.DecryptConnectionString(_encryptedSqlServerConnectionString);
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    _encryptedSqlServerConnectionString = string.Empty;
                else
                    _encryptedSqlServerConnectionString = ConnectionStringProtection.EncryptConnectionString(value);
            }
        }

        // This property is used for serialization to avoid exposing the decrypted connection string
        public string EncryptedSqlServerConnectionString
        {
            get => _encryptedSqlServerConnectionString;
            set => _encryptedSqlServerConnectionString = value;
        }

        // Singleton instance
        private static NotificationSettings? _instance;
        public static NotificationSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Load();
                }
                return _instance;
            }
        }

        // Load settings from file
        public static NotificationSettings Load()
        {
            try
            {
                // Get the settings file path
                string settingsFilePath = GetSettingsFilePath();

                // Create directory if it doesn't exist
                string? directoryPath = Path.GetDirectoryName(settingsFilePath);
                if (directoryPath != null && !Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                // Load settings from file if it exists
                if (File.Exists(settingsFilePath))
                {
                    string json = File.ReadAllText(settingsFilePath);
                    var settings = JsonSerializer.Deserialize<NotificationSettings>(json);
                    if (settings != null)
                    {
                        return settings;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading settings: {ex.Message}");
            }

            // Return default settings if loading fails
            return new NotificationSettings();
        }

        // Save settings to file
        public void Save()
        {
            try
            {
                string settingsFilePath = GetSettingsFilePath();
                string json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });

                // Create directory if it doesn't exist
                string? directoryPath = Path.GetDirectoryName(settingsFilePath);
                if (directoryPath != null && !Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                File.WriteAllText(settingsFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving settings: {ex.Message}");
            }
        }

        // Get the path to the settings file
        private static string GetSettingsFilePath()
        {
            // For MAUI, we use the app data directory which is platform-specific
            string appDataPath = FileSystem.AppDataDirectory;
            return Path.Combine(appDataPath, "settings.json");
        }
    }
}
