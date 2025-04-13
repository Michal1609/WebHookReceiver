using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using WebHookReceiverApi.Models;

namespace WebHookReceiverApi.Hubs
{
    public class NotificationHub : Hub
    {
        private readonly ILogger<NotificationHub> _logger;
        private readonly AppSettings _appSettings;

        public NotificationHub(ILogger<NotificationHub> logger, IOptions<AppSettings> appSettings)
        {
            _logger = logger;
            _appSettings = appSettings.Value;
        }

        public override async Task OnConnectedAsync()
        {
            try
            {
                // Get the SignalR key from query string
                var httpContext = Context.GetHttpContext();
                if (httpContext == null)
                {
                    _logger.LogWarning("HTTP context is null during SignalR connection");
                    Context.Abort();
                    return;
                }

                string? signalRKey = httpContext.Request.Query["signalRKey"];

                // Validate the key
                if (string.IsNullOrEmpty(signalRKey))
                {
                    _logger.LogWarning("SignalR connection attempt without key");
                    Context.Abort();
                    return;
                }

                if (signalRKey != _appSettings.SignalRKey)
                {
                    _logger.LogWarning("SignalR connection attempt with invalid key");
                    Context.Abort();
                    return;
                }

                _logger.LogInformation($"Client connected with valid key: {Context.ConnectionId}");
                await base.OnConnectedAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during SignalR connection");
                Context.Abort();
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogInformation($"Client disconnected: {Context.ConnectionId}, Exception: {exception?.Message}");
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendNotification(WebhookData data)
        {
            _logger.LogInformation($"Sending notification to all clients: {data.Event}");
            await Clients.All.SendAsync("ReceiveNotification", data);
        }
    }
}
