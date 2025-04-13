using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.EntityFrameworkCore;
using System;
using System.Windows;
using System.Windows.Forms;
using WebHookNotifier.Data;
using WebHookNotifier.Models;
using WebHookNotifier.Services;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace WebHookNotifier;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private NotificationService? _notificationService;
    private static TaskbarIcon? _notifyIcon;
    private DatabaseService? _databaseService;
    private NotificationHistoryService? _historyService;

    public MainWindow()
    {
        InitializeComponent();
        _notifyIcon = NotifyIcon;

        // Initialize database services
        InitializeDatabaseServices();

        // Load connection settings
        ServerUrlTextBox.Text = NotificationSettings.Instance.ApiUrl;
        SignalRKeyTextBox.Text = NotificationSettings.Instance.SignalRKey;
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
            MessageBox.Show($"Error initializing database: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ConnectButton_Click(object sender, RoutedEventArgs e)
    {
        string hubUrl = ServerUrlTextBox.Text.Trim();
        if (string.IsNullOrEmpty(hubUrl))
        {
            MessageBox.Show("Enter server URL", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        string signalRKey = SignalRKeyTextBox.Text.Trim();
        if (string.IsNullOrEmpty(signalRKey))
        {
            MessageBox.Show("Please enter a valid SignalR key", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        // Save settings for next time
        NotificationSettings.Instance.ApiUrl = hubUrl;
        NotificationSettings.Instance.SignalRKey = signalRKey;
        NotificationSettings.Instance.Save();

        try
        {
            _notificationService = new NotificationService(hubUrl, _historyService);
            _notificationService.Start();

            StatusText.Text = $"Connected to {hubUrl}";
            ConnectButton.IsEnabled = false;
            DisconnectButton.IsEnabled = true;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Connection error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void DisconnectButton_Click(object sender, RoutedEventArgs e)
    {
        if (_notificationService != null)
        {
            _notificationService.Stop();
            _notificationService = null;

            StatusText.Text = "Disconnected";
            ConnectButton.IsEnabled = true;
            DisconnectButton.IsEnabled = false;
        }
    }

    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        Hide();
    }

    private void NotifyIcon_TrayLeftMouseDown(object sender, RoutedEventArgs e)
    {
        Show();
        WindowState = WindowState.Normal;
        Activate();
    }

    private void NotifyIcon_TrayMouseDoubleClick(object sender, RoutedEventArgs e)
    {
        // Show settings window on double-click
        ShowSettingsWindow();
    }

    private void NotifyIcon_TrayRightMouseDown(object sender, RoutedEventArgs e)
    {
        // Right-click is handled by the context menu
    }

    private void NotifyIcon_BalloonTipClicked(object sender, RoutedEventArgs e)
    {
        Show();
        WindowState = WindowState.Normal;
        Activate();
    }

    private void OpenMenuItem_Click(object sender, RoutedEventArgs e)
    {
        Show();
        WindowState = WindowState.Normal;
        Activate();
    }

    private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }

    private void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        ShowSettingsWindow();
    }

    private void SettingsMenuItem_Click(object sender, RoutedEventArgs e)
    {
        ShowSettingsWindow();
    }

    private void HistoryButton_Click(object sender, RoutedEventArgs e)
    {
        ShowHistoryWindow();
    }

    private void HistoryMenuItem_Click(object sender, RoutedEventArgs e)
    {
        ShowHistoryWindow();
    }

    private void ShowSettingsWindow()
    {
        var settingsWindow = new SettingsWindow();
        settingsWindow.Owner = this;
        settingsWindow.ShowDialog();
    }

    private void ShowHistoryWindow()
    {
        try
        {
            if (_historyService == null)
            {
                MessageBox.Show("History service is not available.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var historyWindow = new HistoryWindow(_historyService);
            historyWindow.Owner = this;
            historyWindow.ShowDialog();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error showing history: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        if (_notificationService != null)
        {
            _notificationService.Stop();
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
        if (message.Length > 40) // Reduced from 80 to 40 for better display in notifications
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
                // Reduced from 60 to 40 characters for better display in notifications
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

    public static void ShowBalloonTip(string title, string message, BalloonIcon icon)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            try
            {
                // Create a more modern-looking notification using NotifyIcon
                // The NotifyIcon control will show notifications in the Windows notification style
                // on Windows 10 and newer

                Console.WriteLine($"Notification sent: {title} - {message}");
                if (_notifyIcon != null)
                {
                    // Format message for better display in balloon tip
                    string formattedMessage = FormatNotificationMessage(message);

                    // Set maximum length for notification (increased from 200 to 800 characters)
                    // Windows 10/11 notifications can display more text with proper formatting
                    string shortMessage = formattedMessage;
                    if (shortMessage.Length > 800)
                    {
                        shortMessage = shortMessage.Substring(0, 797) + "...";
                    }

                    // Show notification with or without sound based on user preference
                    bool playSound = NotificationSettings.Instance.EnableNotificationSounds;
                    _notifyIcon.ShowBalloonTip(title, shortMessage, icon);
                }
                else
                {
                    Console.WriteLine("NotifyIcon is null, cannot display notification");
                }

                // Update last notification in UI if window is open
                if (Application.Current.MainWindow is MainWindow mainWindow)
                {
                    mainWindow.LastNotificationText.Text = $"{title}: {message}";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error displaying notification: {ex.Message}");
                MessageBox.Show($"Error displaying notification: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        });
    }
}