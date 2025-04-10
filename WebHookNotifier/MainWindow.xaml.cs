using Hardcodet.Wpf.TaskbarNotification;
using System;
using System.Windows;
using System.Windows.Forms;
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

    public MainWindow()
    {
        InitializeComponent();
        _notifyIcon = NotifyIcon;
    }

    private void ConnectButton_Click(object sender, RoutedEventArgs e)
    {
        string hubUrl = ServerUrlTextBox.Text.Trim();
        if (string.IsNullOrEmpty(hubUrl))
        {
            MessageBox.Show("Enter server URL", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        try
        {
            _notificationService = new NotificationService(hubUrl);
            _notificationService.Start();

            StatusText.Text = "Connected";
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

    private void ShowSettingsWindow()
    {
        var settingsWindow = new SettingsWindow();
        settingsWindow.Owner = this;
        settingsWindow.ShowDialog();
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        if (_notificationService != null)
        {
            _notificationService.Stop();
        }
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
                    // Shorten message for balloon tip (too long messages can cause problems)
                    string shortMessage = message;
                    if (shortMessage.Length > 200)
                    {
                        shortMessage = shortMessage.Substring(0, 197) + "...";
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