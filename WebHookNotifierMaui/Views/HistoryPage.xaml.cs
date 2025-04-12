using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using WebHookNotifierMaui.Models;
using WebHookNotifierMaui.Services;

namespace WebHookNotifierMaui.Views
{
    public partial class HistoryPage : ContentPage, IQueryAttributable
    {
        private readonly NotificationHistoryService _historyService;
        private List<NotificationHistory> _currentNotifications = new List<NotificationHistory>();
        private bool _isDetailsVisible = false;
        private string _detailsText = string.Empty;

        public bool IsDetailsVisible
        {
            get => _isDetailsVisible;
            set
            {
                _isDetailsVisible = value;
                OnPropertyChanged();
            }
        }

        public string DetailsText
        {
            get => _detailsText;
            set
            {
                _detailsText = value;
                OnPropertyChanged();
            }
        }

        public HistoryPage(NotificationHistoryService historyService)
        {
            InitializeComponent();
            
            _historyService = historyService;
            BindingContext = this;

            // Set default date range (last 7 days)
            FromDatePicker.Date = DateTime.Now.AddDays(-7);
            ToDatePicker.Date = DateTime.Now;

            // Initialize
            LoadEventTypesAsync();
            SearchNotificationsAsync();
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            // This method can be used if we need to pass parameters to the page
        }

        private async void LoadEventTypesAsync()
        {
            try
            {
                // Add "All Events" option
                EventTypePicker.Items.Add("All Events");
                
                // Get event types from database
                var eventTypes = await _historyService.GetEventTypesAsync();
                
                // Add event types to picker
                foreach (var eventType in eventTypes)
                {
                    EventTypePicker.Items.Add(eventType);
                }
                
                // Select "All Events" by default
                EventTypePicker.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error loading event types: {ex.Message}", "OK");
            }
        }

        private async void SearchNotificationsAsync()
        {
            try
            {
                // Show loading indicator
                IsBusy = true;
                
                // Get search parameters
                string searchText = SearchEntry.Text;
                DateTime? fromDate = FromDatePicker.Date;
                DateTime? toDate = ToDatePicker.Date;
                
                string? eventType = null;
                if (EventTypePicker.SelectedIndex > 0) // Skip "All Events" option
                {
                    eventType = EventTypePicker.Items[EventTypePicker.SelectedIndex];
                }
                
                // Search notifications
                _currentNotifications = await _historyService.SearchNotificationsAsync(
                    searchText, fromDate, toDate, eventType);
                
                // Update the collection view
                NotificationsCollection.ItemsSource = _currentNotifications;
                
                // Clear details
                IsDetailsVisible = false;
                DetailsText = string.Empty;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error searching notifications: {ex.Message}", "OK");
            }
            finally
            {
                // Hide loading indicator
                IsBusy = false;
            }
        }

        private void SearchButton_Clicked(object sender, EventArgs e)
        {
            SearchNotificationsAsync();
        }

        private void SearchEntry_Completed(object sender, EventArgs e)
        {
            SearchNotificationsAsync();
        }

        private void NotificationsCollection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is NotificationHistory selectedNotification)
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
                
                DetailsText = details.ToString();
                IsDetailsVisible = true;
            }
            else
            {
                DetailsText = string.Empty;
                IsDetailsVisible = false;
            }
            
            // Clear selection
            NotificationsCollection.SelectedItem = null;
        }

        private async void ExportCsvButton_Clicked(object sender, EventArgs e)
        {
            await ExportNotificationsAsync("csv");
        }

        private async void ExportJsonButton_Clicked(object sender, EventArgs e)
        {
            await ExportNotificationsAsync("json");
        }

        private async Task ExportNotificationsAsync(string format)
        {
            try
            {
                if (_currentNotifications.Count == 0)
                {
                    await DisplayAlert("Export", "No notifications to export.", "OK");
                    return;
                }
                
                // Show loading indicator
                IsBusy = true;
                
                // Export to file
                string filePath = await _historyService.SaveToFileAsync(format, _currentNotifications);
                
                // Share the file
                await ShareFile(filePath, format);
                
                await DisplayAlert("Export Complete", 
                    $"Successfully exported {_currentNotifications.Count} notifications to {format.ToUpper()}.", 
                    "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error exporting to {format.ToUpper()}: {ex.Message}", "OK");
            }
            finally
            {
                // Hide loading indicator
                IsBusy = false;
            }
        }
        
        private async Task ShareFile(string filePath, string format)
        {
            try
            {
                // Create a share request
                var shareFile = new ShareFileRequest
                {
                    Title = $"Export Notifications to {format.ToUpper()}",
                    File = new ShareFile(filePath)
                };
                
                // Share the file
                await Share.RequestAsync(shareFile);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Sharing Error", 
                    $"The file was exported but could not be shared: {ex.Message}\n\nFile location: {filePath}", 
                    "OK");
            }
        }

        private async void CloseButton_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}
