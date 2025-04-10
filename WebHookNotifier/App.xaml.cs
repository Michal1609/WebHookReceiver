using System;
using System.Windows;
using System.Windows.Forms;
using Application = System.Windows.Application;

namespace WebHookNotifier;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        // Set DPI mode for better display scaling
        System.Windows.Forms.Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);

        base.OnStartup(e);

        // Set the application to continue running after the main window is closed
        Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;

        // Create and show the main window
        MainWindow mainWindow = new MainWindow();
        mainWindow.Show();
    }
}

