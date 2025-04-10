using System;
using System.Windows;
using WebHookNotifier.Models;
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
    }
}
