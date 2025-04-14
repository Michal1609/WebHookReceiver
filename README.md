# 🔔 WebHook Receiver

![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![SignalR](https://img.shields.io/badge/SignalR-2C2D72?style=for-the-badge&logo=microsoft&logoColor=white)
![MAUI](https://img.shields.io/badge/MAUI-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![License](https://img.shields.io/badge/License-AGPL--3.0-blue.svg?style=for-the-badge)

## 📋 Overview

WebHook Receiver is a comprehensive solution for collecting, processing, and displaying notifications from various webhook sources on Windows, Android, iOS, and macOS systems. It provides a secure API endpoint for receiving webhooks from any service, and user-friendly applications that display real-time notifications to users across multiple platforms.

**Perfect for:**
- Monitoring CI/CD pipelines
- Tracking application events and alerts
- Receiving notifications from IoT devices
- Integrating with third-party services that support webhooks (GitHub, Slack, etc.)
- Creating custom notification systems for your applications

This project consists of these applications that communicate in real-time:

1. 🌐 **WebHookReceiverApi** - ASP.NET Core Web API application that receives webhooks and forwards them to clients using SignalR
2. 💻 **WebHookNotifier** - Windows desktop application (WPF) that displays notifications based on received webhooks
3. 📱 **WebHookNotifierMaui** - Cross-platform application (.NET MAUI) for Windows, Android, iOS, and macOS
4. 🔑 **ApiKeyGenerator** - Tool for generating API keys for API security

## ✨ Features

- 🔔 Real-time notifications for webhooks
- 🔒 Secure API with API key authentication
- 💻 System tray integration with custom icon
- 📊 Notification history with search and filtering
- 📁 Export history to CSV and JSON formats
- 🗄️ Support for SQLite and SQL Server databases
- 🔐 Secure connection string storage with encryption
- 💯 Comprehensive test coverage
- 📚 Detailed documentation

## 📚 Detailed Documentation

For more detailed information, please refer to the following documentation:

- [Usage Guide](docs/Usage.md) - Comprehensive guide with examples and detailed instructions for all applications
- [WebHookNotifierMaui Documentation](docs/WebHookNotifierMaui.md) - Detailed guide for the MAUI cross-platform application
- [Configuration Guide](docs/Configuration.md) - Complete configuration options for all components
- [Development Guide](docs/Development.md) - Information for developers who want to extend or modify the system

## 💻 Technologies

- 🔥 .NET 9
- 🌐 ASP.NET Core Web API
- 💬 SignalR for real-time communication
- 💻 WPF for Windows application
- 🔔 Hardcodet.NotifyIcon.Wpf for system tray integration
- 📱 Jetpack Compose for Android UI
- 💬 Microsoft SignalR Java client for Android
- 🗄️ Entity Framework Core and Room for database access
- 📊 SQLite and SQL Server database support

## 📎 Project Structure

- 🌐 **WebHookReceiverApi/** - API project
  - **Controllers/** - API controllers
  - **Hubs/** - SignalR hubs
  - **Middleware/** - API key authentication middleware
  - **Models/** - Data models
  - **wwwroot/** - Static files (including test HTML page)

- 💻 **WebHookNotifier/** - Windows application
  - **Models/** - Data models
  - **Services/** - Communication and notification services
  - **Data/** - Database context and repositories
  - **Security/** - Encryption and security services
  - **Resources/** - Icons and other resources

- 📱 **WebHookNotifierMaui/** - Cross-platform application (.NET MAUI)
  - **Models/** - Data models
  - **Services/** - Communication and notification services
  - **Data/** - Database context and repositories
  - **Views/** - User interface pages
  - **Platforms/** - Platform-specific implementations
  - **Resources/** - Icons, images, and other resources

- 🔑 **ApiKeyGenerator/** - API key generator tool
  - Generates secure API keys
  - Updates API configuration file
  - Saves generated key to a file

## 📍 Installation and Running

### 📚 Requirements

- .NET 9 SDK
- Windows (for Windows client application)
- Android 7.0+ (API level 24+), iOS 15.0+, or macOS 11.0+ for MAUI application

### 🌐 Running the API

```bash
cd WebHookReceiverApi
dotnet run
```

The API will be available at `http://localhost:5017`.

### 💻 Running the Windows Application

```bash
cd WebHookNotifier
dotnet run
```

### 📱 Running the MAUI Application

For Windows:
```bash
cd WebHookNotifierMaui
dotnet build -t:Run -f net9.0-windows10.0.19041.0
```

For Android:
```bash
cd WebHookNotifierMaui
dotnet build -t:Run -f net9.0-android
```

For iOS:
```bash
cd WebHookNotifierMaui
dotnet build -t:Run -f net9.0-ios
```

For macOS:
```bash
cd WebHookNotifierMaui
dotnet build -t:Run -f net9.0-maccatalyst
```

## 📝 Usage

### 🔑 Security

#### API Key Security

The API is secured using API keys. To generate a new API key, use the ApiKeyGenerator tool:

```bash
cd ApiKeyGenerator
dotnet run
```

This tool generates a new API key and updates the API configuration file. The generated key is also saved to the `apikey.txt` file.

When calling the API, you need to add the `X-API-Key` header with a valid API key value.

#### SignalR Key Security

In addition to the API key, a SignalR key is required for clients to connect to the SignalR hub. This key is configured in the API's `appsettings.json` file in the `AppSettings.SignalRKey` section.

When connecting to the SignalR hub, clients need to provide this key as a query parameter:

```
http://localhost:5017/notificationHub?signalRKey=your-signalr-key-here
```

This ensures that only authorized clients can connect to the notification hub.

#### Example cURL Request

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

For more examples and detailed information about the API structure, see the [Usage Guide](docs/Usage.md).

### 💬 Testing Webhooks

1. Start the API project
2. Open `http://localhost:5017/test.html` in your browser
3. Fill out the form including the API key and submit the webhook
4. Start the Windows application and connect to the API
5. After sending webhooks, notifications will appear in the system tray

### 📊 Notification History

The application includes a comprehensive notification history system that allows you to:

- View all received notifications in a searchable, filterable list
- See detailed information about each notification
- Export history to CSV or JSON formats
- Configure database storage options

#### Using the History Feature

1. Click the "View History" button in the main window or select "View History" from the system tray menu
2. Use the search box to find specific notifications by content
3. Filter by date range using the date pickers
4. Filter by event type using the dropdown
5. Click on any notification to view its details
6. Use the export buttons to save history to CSV or JSON format

#### Database Configuration

The history system supports two database types:

- **SQLite** (default): Local file-based database, perfect for individual users
- **SQL Server**: Enterprise-grade database for multi-user or centralized deployments

To configure the database:

1. Open Settings from the main window or system tray menu
2. Navigate to the History section
3. Enable or disable history tracking
4. Set the number of days to retain history
5. Select your preferred database type
6. For SQL Server, enter and test your connection string

### ⚙️ Configuration

#### API

- Ports and other settings can be modified in the `WebHookReceiverApi/Properties/launchSettings.json` file
- The API key is stored in the `WebHookReceiverApi/appsettings.json` file in the `AppSettings.ApiKey` section

#### Windows Application

- The API URL can be set in the application
- Notification settings are stored in `%AppData%\WebHookNotifier\settings.json`
- SQLite database is stored in `%AppData%\WebHookNotifier\notifications.db`
- SQL Server connection strings are encrypted using Windows Data Protection API

#### MAUI Application

- The API URL can be set in the application
- Notification settings are stored in the application's local storage
- SQLite database is stored in the application's local storage
- SQL Server connection strings are encrypted using platform-specific encryption
- On Android, direct WebSockets can be used instead of SignalR for better performance

## 🛠️ Development

### ➕ Adding New Webhook Types

1. Modify the `WebhookData` model as needed
2. Implement processing of new types in `WebhookController`

### 🔔 Customizing Notifications

The application offers several ways to customize notifications:

#### Notification Display

- Notification display customizations can be made in the `NotificationService` class
- Format and styling of notifications can be modified in the `FormatNotificationMessage` method in `MainWindow.xaml.cs`

#### Rate Limiting

To prevent notification overload, you can configure rate limiting in the Settings window:

- Set minimum seconds between notifications
- Set maximum number of queued notifications
- Enable or disable notification sounds

#### Security

- Enable or disable encryption for data transmission between API and client

## 🚨 Testing

The project includes unit tests for the API. To run the tests:

```bash
cd WebHookReceiverApiTests
dotnet test
```

## 👮 License

This project is licensed under the GNU Affero General Public License v3.0 (AGPL-3.0) - see the [LICENSE](LICENSE) file for details.

## 👨‍💻 Author

This project was developed by Michal Grznár (michal@grznar.eu).

Project URL: https://github.com/Michal1609/WebHookReceiver
