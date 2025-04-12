# WebHook Receiver Configuration Guide

This document provides detailed information about configuring all components of the WebHook Receiver system.

## API Configuration

### Basic Configuration

The WebHookReceiverApi is configured through several files:

#### appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "AppSettings": {
    "ApiKey": "your-api-key-here",
    "EnableEncryption": true
  }
}
```

Key settings:
- `ApiKey`: The API key used for authentication
- `EnableEncryption`: Whether to encrypt sensitive data

#### launchSettings.json

Located in `WebHookReceiverApi/Properties/launchSettings.json`, this file controls how the API runs:

```json
{
  "$schema": "https://json.schemastore.org/launchsettings.json",
  "profiles": {
    "http": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": false,
      "applicationUrl": "http://localhost:5017",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    },
    "https": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": false,
      "applicationUrl": "https://localhost:7259;http://localhost:5017",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

Key settings:
- `applicationUrl`: The URL(s) the API will listen on
- `environmentVariables`: Environment-specific settings

### API Key Generation

The API key is used to secure the webhook endpoint. To generate a new API key:

1. Run the ApiKeyGenerator tool:
   ```bash
   cd ApiKeyGenerator
   dotnet run
   ```

2. The tool will:
   - Generate a secure random API key
   - Update the API configuration file
   - Save the key to `apikey.txt`

3. Use this API key in the `X-API-Key` header when sending webhooks

### CORS Configuration

Cross-Origin Resource Sharing is configured in `Program.cs`:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
```

Modify this policy to restrict access as needed for production environments.

## Windows Application Configuration

The WebHookNotifier Windows application stores its settings in:
`%AppData%\WebHookNotifier\settings.json`

### Settings Structure

```json
{
  "ApiUrl": "http://localhost:5017/notificationHub",
  "MinSecondsBetweenNotifications": 2,
  "MaxQueuedNotifications": 5,
  "EnableNotificationSounds": true,
  "EnableEncryption": true,
  "EnableHistoryTracking": true,
  "HistoryRetentionDays": 30,
  "DatabaseType": 0,
  "SqlServerConnectionString": ""
}
```

Key settings:
- `ApiUrl`: The URL of the SignalR hub
- `MinSecondsBetweenNotifications`: Minimum time between notifications (seconds)
- `MaxQueuedNotifications`: Maximum number of queued notifications
- `EnableNotificationSounds`: Whether to play sounds with notifications
- `EnableEncryption`: Whether to encrypt sensitive data
- `EnableHistoryTracking`: Whether to save notifications to history
- `HistoryRetentionDays`: How long to keep history before automatic cleanup
- `DatabaseType`: 0 for SQLite, 1 for SQL Server
- `SqlServerConnectionString`: Connection string for SQL Server (if used)

### Database Configuration

#### SQLite (Default)

SQLite database is stored at:
`%AppData%\WebHookNotifier\notifications.db`

No additional configuration is needed.

#### SQL Server

To use SQL Server:
1. Set `DatabaseType` to 1
2. Provide a valid SQL Server connection string
3. The connection string is encrypted using Windows Data Protection API

Example connection string:
```
Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=myPassword;
```

## MAUI Application Configuration

The WebHookNotifierMaui application stores its settings in platform-specific locations:

- **Windows**: `%LocalAppData%\WebHookNotifierMaui\settings.json`
- **Android**: Application's private storage
- **iOS**: Application's private storage
- **macOS**: Application's private storage

### Settings Structure

The settings structure is similar to the Windows application:

```json
{
  "ApiUrl": "http://localhost:5017/notificationHub",
  "MinSecondsBetweenNotifications": 2,
  "MaxQueuedNotifications": 5,
  "EnableNotificationSounds": true,
  "EnableEncryption": true,
  "EnableHistoryTracking": true,
  "HistoryRetentionDays": 30,
  "DatabaseType": 0,
  "SqlServerConnectionString": "",
  "UseDirectWebSocketsOnAndroid": true
}
```

Additional settings for MAUI:
- `UseDirectWebSocketsOnAndroid`: Whether to use direct WebSockets instead of SignalR on Android

### Platform-Specific Considerations

#### Android

- When connecting to a local API from an Android emulator, use `10.0.2.2` instead of `localhost`
- The application will automatically replace `localhost` with `10.0.2.2` when running on Android emulator
- For physical devices, use the actual IP address of the server
- Direct WebSockets may provide better performance than SignalR on some Android devices

#### iOS

- Due to iOS security restrictions, the application must use HTTPS for production
- For development, add appropriate entitlements for local network access

#### Windows

- The application uses Windows notification system for displaying notifications
- Ensure notifications are enabled in Windows settings

#### macOS

- The application uses macOS notification center for displaying notifications
- Ensure notifications are allowed for the application in macOS settings

## Advanced Configuration

### SignalR Configuration

SignalR connection is configured in the client applications:

```csharp
var hubConnection = new HubConnectionBuilder()
    .WithUrl(hubUrl)
    .WithAutomaticReconnect()
    .Build();
```

Customize the reconnection policy by replacing `.WithAutomaticReconnect()` with a custom implementation:

```csharp
.WithAutomaticReconnect(new[] { TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(30) })
```

### Notification Customization

Customize notification appearance by modifying:

- `FormatNotificationMessage` method in the client applications
- Platform-specific notification managers

### Security Considerations

- API keys should be rotated periodically
- In production, use HTTPS for all communication
- Consider implementing additional authentication for multi-user scenarios
- SQL Server connection strings are encrypted, but ensure the database server is properly secured

## Troubleshooting

### API Issues

- Verify the API is running with `dotnet run` in the WebHookReceiverApi directory
- Check the API logs for errors
- Ensure the API key is correctly set in both the API and client requests

### Client Connection Issues

- Verify the API URL is correct
- Check for firewalls or network restrictions
- For Android, ensure the correct IP address is used

### Database Issues

- For SQLite: Ensure the application has write permissions to the storage location
- For SQL Server: Verify the connection string and server accessibility
- Use the "Test Connection" feature in the settings to validate SQL Server connections
