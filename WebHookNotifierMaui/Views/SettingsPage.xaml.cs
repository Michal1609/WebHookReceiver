using System;
using Microsoft.Data.SqlClient;
using WebHookNotifierMaui.Models;

namespace WebHookNotifierMaui.Views
{
    public partial class SettingsPage : ContentPage
    {
        private readonly NotificationSettings _settings;

        public SettingsPage()
        {
            InitializeComponent();

            // Load current settings
            _settings = NotificationSettings.Instance;

            // Populate UI with current settings
            LoadSettings();

            // Set up event handlers
            DatabaseTypePicker.SelectedIndexChanged += DatabaseTypePicker_SelectedIndexChanged;
        }

        private void LoadSettings()
        {
            // Rate limiting settings
            MinSecondsBetweenNotificationsEntry.Text = _settings.MinSecondsBetweenNotifications.ToString();
            MaxQueuedNotificationsEntry.Text = _settings.MaxQueuedNotifications.ToString();
            EnableNotificationSoundsCheckBox.IsChecked = _settings.EnableNotificationSounds;

            // API settings moved to main page

            // Security settings
            EnableEncryptionCheckBox.IsChecked = _settings.EnableEncryption;

            // History settings
            EnableHistoryTrackingCheckBox.IsChecked = _settings.EnableHistoryTracking;
            HistoryRetentionDaysEntry.Text = _settings.HistoryRetentionDays.ToString();

            // Database settings
            DatabaseTypePicker.SelectedIndex = (int)_settings.DatabaseType;
            SqlServerConnectionStringEntry.Text = _settings.SqlServerConnectionString;

            // Show/hide SQL Server settings based on database type
            UpdateDatabaseSettingsVisibility();
        }

        private void DatabaseTypePicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateDatabaseSettingsVisibility();
        }

        private void UpdateDatabaseSettingsVisibility()
        {
            // Show SQL Server settings only when SQL Server is selected
            SqlServerSettingsLayout.IsVisible = DatabaseTypePicker.SelectedIndex == (int)DatabaseType.SQLServer;
        }

        private async void TestConnectionButton_Clicked(object sender, EventArgs e)
        {
            string connectionString = SqlServerConnectionStringEntry.Text.Trim();

            if (string.IsNullOrEmpty(connectionString))
            {
                await DisplayAlert("Test Connection", "Please enter a connection string.", "OK");
                return;
            }

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    // Try to open the connection with a short timeout
                    await connection.OpenAsync();

                    await DisplayAlert("Test Connection", "Connection successful!", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Connection Failed", $"Error: {ex.Message}", "OK");
            }
        }

        private async void SaveButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                // Validate and save rate limiting settings
                if (int.TryParse(MinSecondsBetweenNotificationsEntry.Text, out int minSeconds) && minSeconds >= 0)
                {
                    _settings.MinSecondsBetweenNotifications = minSeconds;
                }
                else
                {
                    await DisplayAlert("Invalid Input", "Please enter a valid number (at least 0) for minimum seconds between notifications.", "OK");
                    return;
                }

                if (int.TryParse(MaxQueuedNotificationsEntry.Text, out int maxQueued) && maxQueued >= 1)
                {
                    _settings.MaxQueuedNotifications = maxQueued;
                }
                else
                {
                    await DisplayAlert("Invalid Input", "Please enter a valid number (at least 1) for maximum queued notifications.", "OK");
                    return;
                }

                _settings.EnableNotificationSounds = EnableNotificationSoundsCheckBox.IsChecked;
                _settings.EnableEncryption = EnableEncryptionCheckBox.IsChecked;

                // API settings moved to main page

                // History settings
                _settings.EnableHistoryTracking = EnableHistoryTrackingCheckBox.IsChecked;

                if (int.TryParse(HistoryRetentionDaysEntry.Text, out int retentionDays) && retentionDays >= 1)
                {
                    _settings.HistoryRetentionDays = retentionDays;
                }
                else
                {
                    await DisplayAlert("Invalid Input", "Please enter a valid number (at least 1) for history retention days.", "OK");
                    return;
                }

                // Database settings
                _settings.DatabaseType = (DatabaseType)DatabaseTypePicker.SelectedIndex;
                _settings.SqlServerConnectionString = SqlServerConnectionStringEntry.Text.Trim();

                // Save settings
                _settings.Save();

                await DisplayAlert("Settings Saved", "Your settings have been saved successfully.", "OK");

                // Close the page
                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error saving settings: {ex.Message}", "OK");
            }
        }

        private async void CancelButton_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}
