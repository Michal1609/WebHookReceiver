# 📘 WebHook Receiver - Comprehensive Usage Guide

This document provides a detailed guide for using all applications within the WebHook Receiver project, including examples, settings, and explanations of features.

## 📋 Contents

- [Component Overview](#component-overview)
- [WebHookReceiverApi](#webhookreceiverapi)
  - [Installation and Launch](#installation-and-launch-api)
  - [API Security](#api-security)
  - [Example API Call using cURL](#example-api-call-using-curl)
  - [Data Structure](#data-structure)
  - [SignalR Key](#signalr-key)
- [WebHookNotifier (Windows)](#webhooknotifier-windows)
  - [Installation and Launch](#installation-and-launch-windows-application)
  - [Connecting to the API](#connecting-to-the-api)
  - [Settings](#windows-application-settings)
  - [Notification History](#notification-history-windows)
- [WebHookNotifierMaui (Android)](#webhooknotifiermaui-android)
  - [Installation and Launch](#installation-and-launch-android-application)
  - [Connecting to the API](#connecting-to-the-api-android)
  - [Settings](#android-application-settings)
  - [Notification History](#notification-history-android)
- [ApiKeyGenerator](#apikeygenerator)
  - [Installation and Launch](#installation-and-launch-apikeygenerator)
  - [Usage](#apikeygenerator-usage)
- [Frequently Asked Questions](#frequently-asked-questions)

## Component Overview

WebHook Receiver consists of the following components:

1. **WebHookReceiverApi** - ASP.NET Core Web API application that receives webhooks and forwards them to clients using SignalR
2. **WebHookNotifier** - Windows desktop application (WPF) that displays notifications based on received webhooks
3. **WebHookNotifierMaui** - Cross-platform application (.NET MAUI) for Windows, Android, iOS, and macOS
4. **ApiKeyGenerator** - Tool for generating API keys for API security

## WebHookReceiverApi

WebHookReceiverApi is the central component that receives webhooks from external services and forwards them to connected clients in real-time using SignalR.

### Installation and Launch (API)

1. Download the latest version from [GitHub Releases](https://github.com/Michal1609/WebHookReceiver/releases)
2. Extract the `WebHookReceiverApi-[version].zip` file
3. Launch the application using the command:

```bash
dotnet WebHookReceiverApi.dll
```

Or on Windows, you can run `WebHookReceiverApi.exe`.

The API will be available at `http://localhost:5017`.

### API Security

The API is secured using an API key. Each request to the API must include an `X-API-Key` header with a valid API key.

The API key is stored in the `appsettings.json` file in the `AppSettings.ApiKey` section. To generate a new API key, use the ApiKeyGenerator tool.

In addition to the API key for accessing the API, a SignalR key is required for clients to connect to the SignalR hub. This key is stored in the `appsettings.json` file in the `AppSettings.SignalRKey` section.

### Example API Call using cURL

Here's an example of how to send a webhook to the API using cURL:

```bash
curl -X POST "http://localhost:5017/api/webhook" \
     -H "Content-Type: application/json" \
     -H "X-API-Key: your-api-key-here" \
     -d '{
           "event": "deployment",
           "message": "Application successfully deployed",
           "timestamp": "2025-04-14T12:00:00Z",
           "source": "CI/CD Pipeline",
           "severity": "info",
           "data": {
             "version": "1.0.0",
             "environment": "production",
             "duration": 120
           }
         }'
```

### Data Structure

Webhook data has the following structure:

```json
{
  "event": "string",       // Event type (required)
  "message": "string",     // Message (required)
  "timestamp": "string",   // Timestamp in ISO 8601 format (optional, defaults to current time)
  "source": "string",      // Event source (optional)
  "severity": "string",    // Severity (optional, options: info, warning, error, critical)
  "data": {                // Additional data (optional)
    "key1": "value1",
    "key2": "value2"
  }
}
```

### SignalR Key

To connect clients to the SignalR hub, a SignalR key is required. This key is added as a parameter in the URL when connecting to the hub:

```
http://localhost:5017/notificationHub?signalRKey=your-signalr-key-here
```

The SignalR key is stored in the `appsettings.json` file in the `AppSettings.SignalRKey` section.

## WebHookNotifier (Windows)

WebHookNotifier is a Windows desktop application that displays notifications based on received webhooks.

### Installation and Launch (Windows Application)

1. Download the latest version from [GitHub Releases](https://github.com/Michal1609/WebHookReceiver/releases)
2. Extract the `WebHookNotifier-[version].zip` file
3. Run the `WebHookNotifier.exe` application

The application will start and appear in the system tray.

### Connecting to the API

1. Click on the application icon in the system tray to display the main window
2. Enter the API URL (e.g., `http://localhost:5017/notificationHub`)
3. Enter the SignalR key
4. Click the "Connect" button

After successful connection, the message "Connected to [URL]" will be displayed, and the application will start receiving notifications.

### Windows Application Settings

Click on "Settings" in the main window or in the context menu in the system tray to open the settings window.

#### Notification Settings

- **Minimum seconds between notifications** - Minimum number of seconds between displaying notifications (to limit the number of notifications)
- **Maximum queued notifications** - Maximum number of notifications in the queue (to limit the number of notifications)
- **Enable notification sounds** - Enable sounds when displaying notifications
- **Enable encryption** - Enable encryption of data between the API and client

#### History Settings

- **Enable history tracking** - Enable tracking of notification history
- **Days to retain history** - Number of days to retain notification history
- **Database type** - Type of database for storing history (SQLite or SQL Server)
- **Connection string** - Connection string for SQL Server (only if SQL Server is selected)

### Notification History (Windows)

Click on "View History" in the main window or in the context menu in the system tray to open the notification history window.

#### History Features

- **Search** - Search notification history by content
- **Filter by date** - Filter notifications by date
- **Filter by event type** - Filter notifications by event type
- **Export to CSV** - Export history to a CSV file
- **Export to JSON** - Export history to a JSON file

## WebHookNotifierMaui (Android)

WebHookNotifierMaui is a cross-platform application for Windows, Android, iOS, and macOS that displays notifications based on received webhooks.

### Installation and Launch (Android Application)

1. Download the latest version from [GitHub Releases](https://github.com/Michal1609/WebHookReceiver/releases)
2. Install the APK file `WebHookNotifierMaui-[version].apk` on your Android device
3. Launch the "WebHook Notifier" application

### Connecting to the API (Android)

1. On the main screen, enter the API URL (e.g., `http://192.168.1.100:5017/notificationHub`)
   - Note: Use the IP address of the computer running the API instead of `localhost`
2. Enter the SignalR key
3. Click the "Connect" button

After successful connection, the message "Connected to [URL]" will be displayed, and the application will start receiving notifications.

### Android Application Settings

Click on the settings icon in the navigation menu to open the settings screen.

#### Notification Settings

- **Minimum seconds between notifications** - Minimum number of seconds between displaying notifications
- **Maximum queued notifications** - Maximum number of notifications in the queue
- **Enable notification sounds** - Enable sounds when displaying notifications
- **Enable encryption** - Enable encryption of data between the API and client

#### Connection Settings

- **Use direct WebSockets on Android** - Use direct WebSockets instead of SignalR on Android (may improve performance)

#### History Settings

- **Enable history tracking** - Enable tracking of notification history
- **Days to retain history** - Number of days to retain notification history

### Notification History (Android)

Click on "History" in the navigation menu to open the notification history screen.

#### History Features

- **Search** - Search notification history by content
- **Filter by date** - Filter notifications by date
- **Filter by event type** - Filter notifications by event type
- **Share** - Share selected notification

## ApiKeyGenerator

ApiKeyGenerator is a tool for generating API keys for API security.

### Installation and Launch (ApiKeyGenerator)

1. Download the latest version from [GitHub Releases](https://github.com/Michal1609/WebHookReceiver/releases)
2. Extract the `ApiKeyGenerator-[version].zip` file
3. Run the `ApiKeyGenerator.exe` application

### ApiKeyGenerator Usage

1. Enter the path to the `appsettings.json` file (default is `../WebHookReceiverApi/appsettings.json`)
2. Click the "Generate API Key" button
3. A new API key will be generated and saved to the `appsettings.json` file
4. The key will also be saved to the `apikey.txt` file for later use

## Frequently Asked Questions

### How do I change the API port?

You can change the API port in the `WebHookReceiverApi/Properties/launchSettings.json` file in the `profiles.WebHookReceiverApi.applicationUrl` section.

### How do I secure communication using HTTPS?

To secure communication using HTTPS, you need to:

1. Generate an SSL certificate
2. Configure the API to use HTTPS in the `WebHookReceiverApi/Properties/launchSettings.json` file
3. Update the URL in client applications to `https://`

### How do I troubleshoot connection issues on Android?

1. Make sure you are using the correct IP address of the computer running the API
2. Check if the API port (default 5017) is open in the firewall
3. Try enabling "Use direct WebSockets on Android" in the application settings
4. Check if you have the correct SignalR key

### How do I add a custom notification type?

To add a custom notification type:

1. Modify the data structure in the API request as needed
2. Modify the notification processing in client applications as needed

### How do I change the appearance of notifications?

You can modify the appearance of notifications:

1. In the Windows application, in the `WebHookNotifier/MainWindow.xaml.cs` file in the `FormatNotificationMessage` method
2. In the Android application, in the `WebHookNotifierMaui/Views/NotificationPage.xaml` file
