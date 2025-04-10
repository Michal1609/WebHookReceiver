using System;
using System.IO;
using System.Text.Json;
using WebHookNotifier.Security;

namespace WebHookNotifier.Models
{
    public class NotificationSettings
    {
        private static readonly string SettingsFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "WebHookNotifier",
            "settings.json");

        // Rate limiting settings with default values
        public int MinSecondsBetweenNotifications { get; set; } = 2;
        public int MaxQueuedNotifications { get; set; } = 5;
        public bool EnableNotificationSounds { get; set; } = true;

        // Security settings
        public bool EnableEncryption { get; set; } = true;

        // History settings
        public bool EnableHistoryTracking { get; set; } = true;
        public int HistoryRetentionDays { get; set; } = 30;

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
                // Create directory if it doesn't exist
                string? directoryPath = Path.GetDirectoryName(SettingsFilePath);
                if (directoryPath != null && !Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                // Load settings from file if it exists
                if (File.Exists(SettingsFilePath))
                {
                    string json = File.ReadAllText(SettingsFilePath);
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
                string json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });

                // Create directory if it doesn't exist
                string? directoryPath = Path.GetDirectoryName(SettingsFilePath);
                if (directoryPath != null && !Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                File.WriteAllText(SettingsFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving settings: {ex.Message}");
            }
        }
    }
}
