using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using Microsoft.Win32;
using MessageBox = System.Windows.MessageBox;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;
using WebHookNotifier.Models;
using WebHookNotifier.Services;

namespace WebHookNotifier
{
    /// <summary>
    /// Interaction logic for HistoryWindow.xaml
    /// </summary>
    public partial class HistoryWindow : Window
    {
        private readonly NotificationHistoryService _historyService;
        private List<NotificationHistory> _currentNotifications = new List<NotificationHistory>();

        public HistoryWindow(NotificationHistoryService historyService)
        {
            InitializeComponent();

            _historyService = historyService;

            // Set default date range (last 7 days)
            FromDatePicker.SelectedDate = DateTime.Now.AddDays(-7);
            ToDatePicker.SelectedDate = DateTime.Now;

            // Load event types for filtering
            LoadEventTypes();

            // Load initial data
            SearchNotifications();
        }

        private async void LoadEventTypes()
        {
            try
            {
                var eventTypes = await _historyService.GetEventTypesAsync();

                // Add event types to the combo box
                foreach (var eventType in eventTypes)
                {
                    EventTypeComboBox.Items.Add(new ComboBoxItem { Content = eventType });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading event types: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void SearchNotifications()
        {
            try
            {
                // Get search parameters
                string searchText = SearchTextBox.Text;
                DateTime? fromDate = FromDatePicker.SelectedDate;
                DateTime? toDate = ToDatePicker.SelectedDate;

                string? eventType = null;
                if (EventTypeComboBox.SelectedIndex > 0) // Skip "All Events" option
                {
                    eventType = (EventTypeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
                }

                // Search notifications
                _currentNotifications = await _historyService.SearchNotificationsAsync(
                    searchText, fromDate, toDate, eventType);

                // Update the data grid
                NotificationsDataGrid.ItemsSource = _currentNotifications;

                // Clear details
                DetailsTextBox.Text = string.Empty;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching notifications: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            SearchNotifications();
        }

        private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SearchNotifications();
            }
        }

        private void NotificationsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NotificationsDataGrid.SelectedItem is NotificationHistory selectedNotification)
            {
                // Format details
                StringBuilder details = new StringBuilder();
                details.AppendLine($"Event: {selectedNotification.Event}");
                details.AppendLine($"Timestamp: {selectedNotification.Timestamp:yyyy-MM-dd HH:mm:ss}");
                details.AppendLine($"Message: {selectedNotification.Message}");

                if (selectedNotification.AdditionalData != null && selectedNotification.AdditionalData.Count > 0)
                {
                    details.AppendLine("\nAdditional Data:");
                    foreach (var item in selectedNotification.AdditionalData)
                    {
                        details.AppendLine($"  {item.Key}: {item.Value}");
                    }
                }

                DetailsTextBox.Text = details.ToString();
            }
            else
            {
                DetailsTextBox.Text = string.Empty;
            }
        }

        private async void ExportCsvButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Create save file dialog
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "CSV files (*.csv)|*.csv",
                    DefaultExt = ".csv",
                    FileName = $"notification_history_{DateTime.Now:yyyyMMdd}.csv"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    // Export to CSV
                    string csv = await _historyService.ExportToCsvAsync(_currentNotifications);

                    // Save to file
                    File.WriteAllText(saveFileDialog.FileName, csv, Encoding.UTF8);

                    MessageBox.Show($"Successfully exported {_currentNotifications.Count} notifications to CSV.",
                        "Export Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting to CSV: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ExportJsonButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Create save file dialog
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "JSON files (*.json)|*.json",
                    DefaultExt = ".json",
                    FileName = $"notification_history_{DateTime.Now:yyyyMMdd}.json"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    // Export to JSON
                    string json = await _historyService.ExportToJsonAsync(_currentNotifications);

                    // Save to file
                    File.WriteAllText(saveFileDialog.FileName, json, Encoding.UTF8);

                    MessageBox.Show($"Successfully exported {_currentNotifications.Count} notifications to JSON.",
                        "Export Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting to JSON: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
