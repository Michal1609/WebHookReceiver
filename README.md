# 🔔 WebHook Receiver

![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![SignalR](https://img.shields.io/badge/SignalR-2C2D72?style=for-the-badge&logo=microsoft&logoColor=white)
![License](https://img.shields.io/badge/License-MIT-yellow.svg?style=for-the-badge)

## 📋 Overview

WebHook Receiver is a comprehensive solution for collecting, processing, and displaying notifications from various webhook sources on Windows and Android systems. It provides a secure API endpoint for receiving webhooks from any service, and user-friendly applications for both Windows and Android that display real-time notifications to users.

**Perfect for:**
- Monitoring CI/CD pipelines
- Tracking application events and alerts
- Receiving notifications from IoT devices
- Integrating with third-party services that support webhooks (GitHub, Slack, etc.)
- Creating custom notification systems for your applications

This project consists of three applications that communicate in real-time:

1. 🌐 **WebHookReceiverApi** - ASP.NET Core Web API application that receives webhooks and forwards them to clients using SignalR
2. 💻 **WebHookNotifier** - Windows application that displays notifications based on received webhooks
3. 📱 **WebHookNotifierAndroid** - Android application that displays notifications based on received webhooks
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

- 📱 **WebHookNotifierAndroid/** - Android application
  - **data/** - Data models, repositories and database access
  - **service/** - SignalR and notification services
  - **ui/** - User interface components using Jetpack Compose
  - **util/** - Utility classes for encryption, formatting, and export

- 🔑 **ApiKeyGenerator/** - API key generator tool
  - Generates secure API keys
  - Updates API configuration file
  - Saves generated key to a file

## 📍 Installation and Running

### 📚 Requirements

- .NET 9 SDK
- Windows (for Windows client application)
- Android 7.0+ (API level 24+) for Android application

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

### 📱 Building the Android Application

```bash
cd WebHookNotifierAndroid
./gradlew assembleDebug
```

The APK will be available at `WebHookNotifierAndroid/app/build/outputs/apk/debug/app-debug.apk`

## 📝 Usage

### 🔑 API Key Security

The API is secured using API keys. To generate a new API key, use the ApiKeyGenerator tool:

```bash
cd ApiKeyGenerator
dotnet run
```

This tool generates a new API key and updates the API configuration file. The generated key is also saved to the `apikey.txt` file.

When calling the API, you need to add the `X-API-Key` header with a valid API key value.

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

#### Android Application

- The API URL can be set in the application
- Notification settings are stored in Android's DataStore
- SQLite database is stored in the app's private storage
- SQL Server connection strings are encrypted using Android's EncryptedSharedPreferences

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

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 👨‍💻 Credits

This project was developed by [Augment](https://augmentcode.com/) - an AI-powered coding assistant.
