using System;
using System.Windows;

namespace WebHookNotifier;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Nastavení, aby aplikace běžela i po zavření hlavního okna
        Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;

        // Vytvoření a zobrazení hlavního okna
        MainWindow mainWindow = new MainWindow();
        mainWindow.Show();
    }
}

