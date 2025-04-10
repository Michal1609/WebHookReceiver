using System;
using Microsoft.Data.SqlClient;
using System.Windows;
using Microsoft.Data.Sqlite;
using WebHookNotifier.Models;
using WebHookNotifier.Security;
using MessageBox = System.Windows.MessageBox;

namespace WebHookNotifier
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private readonly NotificationSettings _settings;

        public SettingsWindow()
        {
            InitializeComponent();

            // Load current settings
            _settings = NotificationSettings.Instance;

            // Populate UI with current settings
            MinSecondsBetweenNotificationsTextBox.Text = _settings.MinSecondsBetweenNotifications.ToString();
            MaxQueuedNotificationsTextBox.Text = _settings.MaxQueuedNotifications.ToString();
            EnableNotificationSoundsCheckBox.IsChecked = _settings.EnableNotificationSounds;
            EnableEncryptionCheckBox.IsChecked = _settings.EnableEncryption;

            // History settings
            EnableHistoryTrackingCheckBox.IsChecked = _settings.EnableHistoryTracking;
            HistoryRetentionDaysTextBox.Text = _settings.HistoryRetentionDays.ToString();

            // Database settings
            if (_settings.DatabaseType == DatabaseType.SQLite)
            {
                SqliteRadioButton.IsChecked = true;
                SqlServerRadioButton.IsChecked = false;
                SqlServerConnectionGrid.Visibility = Visibility.Collapsed;
            }
            else
            {
                SqliteRadioButton.IsChecked = false;
                SqlServerRadioButton.IsChecked = true;
                SqlServerConnectionGrid.Visibility = Visibility.Visible;
                SqlServerConnectionStringTextBox.Text = _settings.SqlServerConnectionString;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validate and save settings
                if (int.TryParse(MinSecondsBetweenNotificationsTextBox.Text, out int minSeconds) && minSeconds >= 0)
                {
                    _settings.MinSecondsBetweenNotifications = minSeconds;
                }
                else
                {
                    MessageBox.Show("Please enter a valid number for minimum seconds between notifications.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (int.TryParse(MaxQueuedNotificationsTextBox.Text, out int maxQueue) && maxQueue >= 1)
                {
                    _settings.MaxQueuedNotifications = maxQueue;
                }
                else
                {
                    MessageBox.Show("Please enter a valid number (at least 1) for maximum queued notifications.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                _settings.EnableNotificationSounds = EnableNotificationSoundsCheckBox.IsChecked ?? true;
                _settings.EnableEncryption = EnableEncryptionCheckBox.IsChecked ?? true;

                // History settings
                _settings.EnableHistoryTracking = EnableHistoryTrackingCheckBox.IsChecked ?? true;

                if (int.TryParse(HistoryRetentionDaysTextBox.Text, out int retentionDays) && retentionDays >= 1)
                {
                    _settings.HistoryRetentionDays = retentionDays;
                }
                else
                {
                    MessageBox.Show("Please enter a valid number (at least 1) for history retention days.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Database settings
                _settings.DatabaseType = SqliteRadioButton.IsChecked == true ? DatabaseType.SQLite : DatabaseType.SQLServer;

                if (_settings.DatabaseType == DatabaseType.SQLServer)
                {
                    string connectionString = SqlServerConnectionStringTextBox.Text.Trim();
                    if (string.IsNullOrEmpty(connectionString))
                    {
                        MessageBox.Show("Please enter a SQL Server connection string.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    _settings.SqlServerConnectionString = connectionString;
                }

                // Save settings to file
                _settings.Save();

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void SqliteRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (SqlServerConnectionGrid != null)
            {
                SqlServerConnectionGrid.Visibility = Visibility.Collapsed;
            }
        }

        private void SqlServerRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (SqlServerConnectionGrid != null)
            {
                SqlServerConnectionGrid.Visibility = Visibility.Visible;
            }
        }

        private void TestConnectionButton_Click(object sender, RoutedEventArgs e)
        {
            string connectionString = SqlServerConnectionStringTextBox.Text.Trim();

            if (string.IsNullOrEmpty(connectionString))
            {
                MessageBox.Show("Please enter a connection string.", "Test Connection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    MessageBox.Show("Connection successful!", "Test Connection", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Connection failed: {ex.Message}", "Test Connection", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
