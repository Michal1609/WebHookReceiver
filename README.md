# 🔔 WebHook Receiver

![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![SignalR](https://img.shields.io/badge/SignalR-2C2D72?style=for-the-badge&logo=microsoft&logoColor=white)
![License](https://img.shields.io/badge/License-MIT-yellow.svg?style=for-the-badge)

## 📋 Overview

WebHook Receiver is a comprehensive solution for collecting, processing, and displaying notifications from various webhook sources on Windows systems. It provides a secure API endpoint for receiving webhooks from any service, and a user-friendly system tray application that displays real-time notifications to users.

**Perfect for:**
- Monitoring CI/CD pipelines
- Tracking application events and alerts
- Receiving notifications from IoT devices
- Integrating with third-party services that support webhooks (GitHub, Slack, etc.)
- Creating custom notification systems for your applications

This project consists of three applications that communicate in real-time:

1. 🌐 **WebHookReceiverApi** - ASP.NET Core Web API application that receives webhooks and forwards them to clients using SignalR
2. 💻 **WebHookNotifier** - Windows application that displays notifications based on received webhooks
3. 🔑 **ApiKeyGenerator** - Tool for generating API keys for API security

## ✨ Features

- 🔔 Real-time notifications for webhooks
- 🔒 Secure API with API key authentication
- 💻 System tray integration with custom icon
- 💯 Comprehensive test coverage
- 📚 Detailed documentation

## 💻 Technologies

- 🔥 .NET 9
- 🌐 ASP.NET Core Web API
- 💬 SignalR for real-time communication
- 💻 WPF for Windows application
- 🔔 Hardcodet.NotifyIcon.Wpf for system tray integration

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
  - **Resources/** - Icons and other resources

- 🔑 **ApiKeyGenerator/** - API key generator tool
  - Generates secure API keys
  - Updates API configuration file
  - Saves generated key to a file

## 📍 Installation and Running

### 📚 Requirements

- .NET 9 SDK
- Windows (for client application)

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

### ⚙️ Configuration

#### API

- Ports and other settings can be modified in the `WebHookReceiverApi/Properties/launchSettings.json` file
- The API key is stored in the `WebHookReceiverApi/appsettings.json` file in the `AppSettings.ApiKey` section

#### Windows Application

- The API URL can be set in the application

## 🛠️ Development

### ➕ Adding New Webhook Types

1. Modify the `WebhookData` model as needed
2. Implement processing of new types in `WebhookController`

### 🔔 Customizing Notifications

Notification display customizations can be made in the `NotificationService` class in the Windows application project.

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
