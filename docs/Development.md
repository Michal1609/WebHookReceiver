# WebHook Receiver Development Guide

This document provides detailed information for developers who want to extend or modify the WebHook Receiver system.

## Architecture Overview

The WebHook Receiver system follows a clean architecture pattern with these main components:

1. **WebHookReceiverApi**: ASP.NET Core Web API that receives webhooks and forwards them to clients
2. **WebHookNotifier**: Windows desktop application (WPF) that displays notifications
3. **WebHookNotifierMaui**: Cross-platform application (.NET MAUI) for multiple platforms
4. **ApiKeyGenerator**: Utility for generating API keys

### Communication Flow

```
External Service → WebHookReceiverApi → SignalR Hub → Client Applications
```

1. External services send HTTP POST requests to the webhook endpoint
2. The API validates the API key and processes the webhook data
3. The SignalR hub broadcasts the notification to all connected clients
4. Client applications receive and display the notification

## WebHookReceiverApi

### Key Components

- **WebhookController**: Handles incoming webhook requests
- **NotificationHub**: SignalR hub for real-time communication
- **ApiKeyAuthMiddleware**: Middleware for API key authentication
- **WebhookData**: Model representing webhook data

### Adding New Webhook Types

To add support for new webhook types:

1. Modify the `WebhookData` model if needed:

```csharp
public class WebhookData
{
    public string? Id { get; set; }
    public string? Event { get; set; }
    public string? Message { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object>? AdditionalData { get; set; }
    
    // Add new properties as needed
    public string? Priority { get; set; }
}
```

2. Update the `WebhookController` to handle the new properties:

```csharp
[HttpPost]
public async Task<IActionResult> ReceiveWebhook([FromBody] WebhookData data)
{
    // Validate and process the webhook data
    if (data == null)
    {
        return BadRequest(new { status = "error", message = "Invalid webhook data" });
    }

    // Process new properties
    if (!string.IsNullOrEmpty(data.Priority))
    {
        _logger.LogInformation($"Received webhook with priority: {data.Priority}");
    }

    // Broadcast to clients
    await _hubContext.Clients.All.SendAsync("ReceiveNotification", data);

    return Ok(new { status = "success", message = "Webhook received" });
}
```

### Adding Custom Processing

To add custom processing for specific webhook types:

```csharp
[HttpPost]
public async Task<IActionResult> ReceiveWebhook([FromBody] WebhookData data)
{
    // ... existing code ...

    // Custom processing based on event type
    switch (data.Event?.ToLower())
    {
        case "deployment":
            await ProcessDeploymentWebhook(data);
            break;
        case "alert":
            await ProcessAlertWebhook(data);
            break;
        default:
            await _hubContext.Clients.All.SendAsync("ReceiveNotification", data);
            break;
    }

    return Ok(new { status = "success", message = "Webhook received" });
}

private async Task ProcessDeploymentWebhook(WebhookData data)
{
    // Custom processing for deployment webhooks
    // For example, add additional data or modify the message
    
    data.Message = $"Deployment: {data.Message}";
    
    await _hubContext.Clients.All.SendAsync("ReceiveNotification", data);
}
```

## WebHookNotifier (Windows Application)

### Key Components

- **MainWindow**: Main application UI
- **NotificationService**: Handles receiving and displaying notifications
- **DatabaseService**: Manages database connections and operations
- **NotificationHistoryService**: Handles history operations
- **SignalRService**: Manages SignalR connection to the API

### Customizing Notifications

To customize the appearance of notifications:

1. Modify the `ShowNotification` method in `MainWindow.xaml.cs`:

```csharp
private void ShowNotification(WebhookData data, bool hasMoreItems = false)
{
    string title = $"Webhook: {data.Event}";
    string message = data.Message ?? "No message";
    
    // Customize based on properties
    if (data.AdditionalData?.ContainsKey("Priority") == true)
    {
        string priority = data.AdditionalData["Priority"].ToString();
        if (priority == "High")
        {
            title = $"🔴 HIGH PRIORITY: {title}";
        }
    }
    
    // Format message
    message = FormatNotificationMessage(message);
    
    // Show notification
    _notifyIcon.ShowBalloonTip(title, message, BalloonIcon.Info);
    
    // Update UI
    LastNotificationText.Text = $"{title}: {message}";
}
```

2. Customize the `FormatNotificationMessage` method:

```csharp
private string FormatNotificationMessage(string message)
{
    // Process JSON data to make it more readable
    if (message.Contains("{") && message.Contains("}"))
    {
        try
        {
            // Try to format JSON more nicely
            message = message.Replace("{\":", "{\r\n  \"")
                           .Replace(",\":", ",\r\n  \"")
                           .Replace("}", "\r\n}");
        }
        catch
        {
            // If formatting fails, continue with original message
        }
    }
    
    // Add custom formatting
    // ...
    
    return message;
}
```

### Adding New Features

To add new features to the Windows application:

1. Create new UI elements in XAML
2. Implement the corresponding logic in the code-behind
3. Add new services as needed
4. Update the settings model if required

## WebHookNotifierMaui (Cross-Platform Application)

### Key Components

- **MainPage**: Main application UI
- **SettingsPage**: Configuration interface
- **HistoryPage**: History viewing and management interface
- **NotificationService**: Handles receiving and displaying notifications
- **DatabaseService**: Manages database connections and operations
- **NotificationHistoryService**: Handles history operations
- **Platform-specific notification managers**: Handle platform-specific notification display

### Adding Platform-Specific Code

MAUI uses a platform-specific folder structure:

- **Platforms/Android**: Android-specific code
- **Platforms/iOS**: iOS-specific code
- **Platforms/MacCatalyst**: macOS-specific code
- **Platforms/Windows**: Windows-specific code

To add platform-specific functionality:

1. Create a common interface:

```csharp
public interface IPlatformNotificationManager
{
    void SendNotification(string title, string message);
    void Initialize();
}
```

2. Implement the interface for each platform:

```csharp
// In Platforms/Android/AndroidNotificationManager.cs
public class AndroidNotificationManager : IPlatformNotificationManager
{
    // Implementation for Android
}

// In Platforms/iOS/iOSNotificationManager.cs
public class iOSNotificationManager : IPlatformNotificationManager
{
    // Implementation for iOS
}

// And so on for other platforms
```

3. Use conditional compilation to call the appropriate implementation:

```csharp
private void ShowPlatformNotification(string title, string message)
{
#if ANDROID
    AndroidNotificationManager.Instance.SendNotification(title, message);
#elif IOS
    iOSNotificationManager.Instance.SendNotification(title, message);
#elif WINDOWS
    WindowsNotificationManager.Instance.SendNotification(title, message);
#elif MACCATALYST
    MacNotificationManager.Instance.SendNotification(title, message);
#else
    // Fallback
    Console.WriteLine($"Notification: {title} - {message}");
#endif
}
```

### Adding New Pages

To add a new page to the MAUI application:

1. Create a new XAML file and code-behind:

```xaml
<!-- NewFeaturePage.xaml -->
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="WebHookNotifierMaui.Views.NewFeaturePage"
             Title="New Feature">
    <VerticalStackLayout Padding="20">
        <Label Text="New Feature" FontSize="24" FontAttributes="Bold" />
        <!-- Add your UI elements here -->
    </VerticalStackLayout>
</ContentPage>
```

```csharp
// NewFeaturePage.xaml.cs
using System;
namespace WebHookNotifierMaui.Views
{
    public partial class NewFeaturePage : ContentPage
    {
        public NewFeaturePage()
        {
            InitializeComponent();
        }
        
        // Add your code here
    }
}
```

2. Register the page in AppShell.xaml:

```xaml
<FlyoutItem Title="New Feature">
    <ShellContent
        Title="New Feature"
        ContentTemplate="{DataTemplate views:NewFeaturePage}"
        Route="NewFeaturePage" />
</FlyoutItem>
```

3. Register the route in AppShell.xaml.cs:

```csharp
Routing.RegisterRoute(nameof(NewFeaturePage), typeof(NewFeaturePage));
```

4. Navigate to the page:

```csharp
await Navigation.PushAsync(new NewFeaturePage());
// or
await Shell.Current.GoToAsync("NewFeaturePage");
```

## Database Schema

The application uses Entity Framework Core with the following schema:

### NotificationHistory

```csharp
public class NotificationHistory
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Event { get; set; } = string.Empty;

    public string? Message { get; set; }

    [Required]
    public DateTime Timestamp { get; set; }

    public string? AdditionalDataJson { get; set; }

    [NotMapped]
    public Dictionary<string, object>? AdditionalData { get; set; }
}
```

To modify the database schema:

1. Update the model classes
2. Update the `OnModelCreating` method in `NotificationDbContext`
3. Consider adding a migration if using SQL Server

## Testing

### API Testing

The project includes unit tests for the API. To add new tests:

1. Create a new test class in the WebHookReceiverApiTests project
2. Use xUnit for testing
3. Use Moq for mocking dependencies

Example test:

```csharp
[Fact]
public async Task ReceiveWebhook_WithNewProperty_ProcessesCorrectly()
{
    // Arrange
    var mockHubContext = new Mock<IHubContext<NotificationHub>>();
    var mockLogger = new Mock<ILogger<WebhookController>>();
    var mockOptions = new Mock<IOptions<ApiKeySettings>>();
    
    mockOptions.Setup(x => x.Value).Returns(new ApiKeySettings
    {
        ApiKey = "test-api-key",
        EnableEncryption = true
    });
    
    var controller = new WebhookController(mockHubContext.Object, mockLogger.Object, mockOptions.Object);
    
    var webhookData = new WebhookData
    {
        Id = "test-id",
        Event = "test-event",
        Message = "Test message",
        Timestamp = DateTime.UtcNow,
        Priority = "High",
        AdditionalData = new Dictionary<string, object>
        {
            { "key1", "value1" }
        }
    };
    
    var mockClients = new Mock<IHubClients>();
    var mockClientProxy = new Mock<IClientProxy>();
    
    mockHubContext.Setup(h => h.Clients.All).Returns(mockClientProxy.Object);
    
    // Act
    var result = await controller.ReceiveWebhook(webhookData);
    
    // Assert
    var okResult = Assert.IsType<OkObjectResult>(result);
    Assert.NotNull(okResult.Value);
    
    mockHubContext.Verify(h => h.Clients.All, Times.Once);
    mockClientProxy.Verify(
        c => c.SendAsync(
            "ReceiveNotification",
            It.Is<WebhookData>(d => d.Priority == "High"),
            It.IsAny<CancellationToken>()),
        Times.Once);
}
```

### Client Testing

For client applications, consider adding:

1. Unit tests for services and utilities
2. UI tests using tools like Appium for MAUI
3. Integration tests for database operations

## Performance Considerations

### API Performance

- Use async/await for all I/O operations
- Consider adding caching for frequently accessed data
- Use minimal logging in production

### Client Performance

- Use virtualization for long lists (history view)
- Implement efficient filtering for history searches
- Consider background processing for database operations

### SignalR Performance

- Use binary protocol for better performance
- Implement reconnection logic
- Consider using direct WebSockets on Android for better performance

## Security Considerations

- Rotate API keys regularly
- Use HTTPS in production
- Encrypt sensitive data (connection strings, etc.)
- Validate all input data
- Consider implementing user authentication for multi-user scenarios

## Deployment

### API Deployment

- Deploy to Azure App Service, AWS Elastic Beanstalk, or similar services
- Use Docker containers for consistent deployment
- Set up proper logging and monitoring

### Client Deployment

- For Windows: Create an installer using WiX, Inno Setup, or similar
- For MAUI: Use the appropriate store for each platform or distribute as a standalone package

## Contributing

When contributing to this project:

1. Follow the existing code style and architecture
2. Add appropriate tests for new features
3. Document your changes
4. Update the README.md and other documentation as needed
