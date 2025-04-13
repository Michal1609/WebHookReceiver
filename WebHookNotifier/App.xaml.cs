using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using Application = System.Windows.Application;

namespace WebHookNotifier;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    // Mutex pro kontrolu jediné instance
    private static Mutex? _mutex = null;
    private const string MutexName = "WebHookNotifierSingleInstanceMutex";

    protected override void OnStartup(StartupEventArgs e)
    {
        // Kontrola, zda již neběží jiná instance aplikace
        bool createdNew;
        _mutex = new Mutex(true, MutexName, out createdNew);

        if (!createdNew)
        {
            // Aplikace již běží, aktivujeme existující okno a ukončíme tuto instanci
            ActivateExistingInstance();
            Shutdown();
            return;
        }

        // Set DPI mode for better display scaling
        System.Windows.Forms.Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);

        base.OnStartup(e);

        // Set the application to continue running after the main window is closed
        Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;

        // Create and show the main window
        MainWindow mainWindow = new MainWindow();
        mainWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        // Uvolníme mutex při ukončení aplikace
        if (_mutex != null)
        {
            _mutex.ReleaseMutex();
            _mutex.Dispose();
        }
        base.OnExit(e);
    }

    /// <summary>
    /// Aktivuje existující instanci aplikace
    /// </summary>
    private void ActivateExistingInstance()
    {
        // Najdeme existující proces
        var currentProcess = Process.GetCurrentProcess();
        var processes = Process.GetProcessesByName(currentProcess.ProcessName);

        foreach (var process in processes)
        {
            // Přeskočíme aktuální proces
            if (process.Id == currentProcess.Id)
                continue;

            // Pokusíme se aktivovat okno existující instance
            IntPtr hWnd = process.MainWindowHandle;
            if (hWnd != IntPtr.Zero)
            {
                // Obnovit okno, pokud je minimalizované
                if (IsIconic(hWnd))
                    ShowWindow(hWnd, SW_RESTORE);

                // Přenese okno do popředí
                SetForegroundWindow(hWnd);
                break;
            }
        }
    }

    // Win32 API pro práci s okny
    [DllImport("user32.dll")]
    private static extern bool IsIconic(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    private const int SW_RESTORE = 9;
}