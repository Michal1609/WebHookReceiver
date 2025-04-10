using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using WebHookReceiverApi.Controllers;
using WebHookReceiverApi.Hubs;
using WebHookReceiverApi.Middleware;
using WebHookReceiverApi.Models;

namespace WebHookReceiverApiTests;

public class WebhookControllerTests
{
    private readonly Mock<IHubContext<NotificationHub>> _mockHubContext;
    private readonly Mock<ILogger<WebhookController>> _mockLogger;
    private readonly Mock<IOptions<ApiKeySettings>> _mockApiKeySettings;
    private readonly WebhookController _controller;

    public WebhookControllerTests()
    {
        _mockHubContext = new Mock<IHubContext<NotificationHub>>();
        _mockLogger = new Mock<ILogger<WebhookController>>();

        // Setup API key settings with encryption enabled
        _mockApiKeySettings = new Mock<IOptions<ApiKeySettings>>();
        _mockApiKeySettings.Setup(x => x.Value).Returns(new ApiKeySettings
        {
            ApiKey = "test-api-key",
            EnableEncryption = true
        });

        _controller = new WebhookController(_mockHubContext.Object, _mockLogger.Object, _mockApiKeySettings.Object);
    }

    [Fact]
    public async Task ReceiveWebhook_ValidData_ReturnsOk()
    {
        // Arrange
        var webhookData = new WebhookData
        {
            Id = "test-id",
            Event = "test-event",
            Message = "Test message",
            Timestamp = DateTime.UtcNow,
            AdditionalData = new Dictionary<string, object>
            {
                { "key1", "value1" },
                { "key2", 123 }
            }
        };

        var mockClients = new Mock<IHubClients>();
        var mockClientProxy = new Mock<IClientProxy>();

        _mockHubContext.Setup(h => h.Clients.All).Returns(mockClientProxy.Object);

        // Act
        var result = await _controller.ReceiveWebhook(webhookData);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);

        // Verify that the hub context was used
        _mockHubContext.Verify(h => h.Clients.All, Times.Once);
    }

    [Fact]
    public async Task ReceiveWebhook_NullData_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.ReceiveWebhook(null!);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public void Test_Endpoint_ReturnsOk()
    {
        // Act
        var result = _controller.Test();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);

        var value = okResult.Value as object;
        var properties = value.GetType().GetProperties();
        var statusProperty = properties.FirstOrDefault(p => p.Name == "status");
        Assert.NotNull(statusProperty);
        Assert.Equal("Webhook receiver is running", statusProperty.GetValue(value));
    }
}
