# WebHookNotifierMaui - Cross-Platform Notification Client

WebHookNotifierMaui is a cross-platform application built with .NET MAUI that provides real-time notifications from webhooks across Windows, Android, iOS, and macOS platforms. This document provides detailed information about the application's features, setup, and usage.

## Features

- **Real-time Notifications**: Receive and display notifications from webhooks in real-time
- **Cross-Platform Support**: Works on Windows, Android, iOS, and macOS
- **Notification History**: View, search, and filter your notification history
- **Data Export**: Export notification history to CSV or JSON formats
- **Database Options**: Choose between SQLite (local) or SQL Server (centralized) storage
- **Secure Storage**: Encrypted storage of sensitive information like connection strings
- **Customizable Settings**: Configure notification behavior, history retention, and more

## Installation

### Requirements

- .NET 9 SDK
- Visual Studio 2022 or later with .NET MAUI workload
- For Android: Android SDK with API level 24 or higher
- For iOS: macOS with Xcode 13 or later
- For macOS: macOS 11 or later

### Building the Application

From the command line:

```bash
cd WebHookNotifierMaui
dotnet build
```

Or open the solution in Visual Studio and build the WebHookNotifierMaui project.

### Running the Application

#### Windows

```bash
dotnet build -t:Run -f net9.0-windows10.0.19041.0
```

#### Android

```bash
dotnet build -t:Run -f net9.0-android
```

#### iOS

```bash
dotnet build -t:Run -f net9.0-ios
```

#### macOS

```bash
dotnet build -t:Run -f net9.0-maccatalyst
```

## Configuration

### Connecting to the API

1. Launch the application
2. Enter the API URL in the format: `http://server-address:port/notificationHub`
   - For local testing: `http://localhost:5017/notificationHub`
   - For Android emulator, use `10.0.2.2` instead of `localhost`
3. Click "Connect"

### Settings

Access the settings page by clicking the "Settings" button on the main screen. The settings page allows you to configure:

#### Rate Limiting

- **Minimum seconds between notifications**: Controls how frequently notifications can appear (default: 2 seconds)
- **Maximum queued notifications**: Limits the number of notifications that can be queued (default: 5)
- **Enable notification sounds**: Toggle notification sounds on/off

#### Security

- **Enable encryption**: Toggle encryption for sensitive data

#### History

- **Enable history tracking**: Toggle whether notifications are saved to history
- **History retention days**: Set how long notifications are kept before automatic cleanup (default: 30 days)

#### Database

- **Database type**: Choose between SQLite (local) or SQL Server
- **SQL Server connection string**: If using SQL Server, enter your connection string
- **Test connection**: Test the SQL Server connection

### Android-Specific Settings

On Android devices, you have an additional option:

- **Use direct WebSockets**: Toggle between using direct WebSockets or SignalR for communication
  - Direct WebSockets may provide better performance on some Android devices

## Using the History Feature

1. Click the "View History" button on the main screen
2. Use the search box to find specific notifications
3. Filter by date range using the date pickers
4. Filter by event type using the dropdown
5. Click on any notification to view its details
6. Use the export buttons to save history to CSV or JSON format

## Architecture

WebHookNotifierMaui follows a clean architecture pattern with these main components:

### Models

- **WebhookData**: Represents the data received from webhooks
- **NotificationHistory**: Represents stored notification history
- **NotificationSettings**: Manages application settings

### Services

- **NotificationService**: Handles receiving and displaying notifications
- **DatabaseService**: Manages database connections and operations
- **NotificationHistoryService**: Handles history operations (search, export, etc.)
- **SignalRService**: Manages SignalR connection to the API
- **DirectWebSocketService**: Alternative to SignalR for Android

### Views

- **MainPage**: The main application interface
- **SettingsPage**: Configuration interface
- **HistoryPage**: History viewing and management interface

### Platform-Specific Implementations

- **AndroidNotificationManager**: Handles notifications on Android
- **WindowsNotificationManager**: Handles notifications on Windows
- Platform-specific implementations for iOS and macOS

## Troubleshooting

### Connection Issues

- Verify the API is running and accessible
- Check for firewalls or network restrictions
- On Android, ensure the correct IP address is used (10.0.2.2 for emulator)
- Try toggling "Use direct WebSockets" on Android

### Notification Issues

- Ensure notifications are enabled in your device settings
- Check the application logs for errors
- Verify the API key is correct

### Database Issues

- For SQLite: Ensure the application has write permissions
- For SQL Server: Verify the connection string and server accessibility
- Try the "Test Connection" button in settings

## Development

### Adding New Features

The application is designed to be easily extensible:

1. Add new models in the `Models` folder
2. Implement new services in the `Services` folder
3. Create new views in the `Views` folder
4. Add platform-specific implementations in the `Platforms` folder

### Customizing the UI

The UI is built with XAML and follows MVVM patterns:

1. Modify the XAML files in the `Views` folder
2. Update the code-behind files for behavior changes
3. Use MAUI's styling system for consistent appearance

## License

This project is licensed under the MIT License - see the [LICENSE](../LICENSE) file for details.
