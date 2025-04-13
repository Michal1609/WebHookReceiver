using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using System.Text.Json;
using WebHookReceiverApi.Hubs;
using WebHookReceiverApi.Models;
using WebHookReceiverApi.Security;

namespace WebHookReceiverApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WebhookController : ControllerBase
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<WebhookController> _logger;
        private readonly AppSettings _appSettings;

        public WebhookController(IHubContext<NotificationHub> hubContext, ILogger<WebhookController> logger, IOptions<AppSettings> appSettings)
        {
            _hubContext = hubContext;
            _logger = logger;
            _appSettings = appSettings.Value;
        }

        [HttpPost]
        public async Task<IActionResult> ReceiveWebhook([FromBody] WebhookData webhookData)
        {
            try
            {
                // Check if data is not null
                if (webhookData == null)
                {
                    _logger.LogWarning("Received null webhook data");
                    return BadRequest(new { success = false, message = "Webhook data cannot be null" });
                }

                _logger.LogInformation("Received webhook: {Event}", webhookData.Event);

                // Add timestamp if not set
                if (webhookData.Timestamp == default)
                {
                    webhookData.Timestamp = DateTime.UtcNow;
                }

                // Prepare data for sending
                string dataToSend;

                // Check if encryption is enabled in settings
                if (_appSettings.EnableEncryption)
                {
                    // Encrypt webhook data before sending
                    string jsonData = JsonSerializer.Serialize(webhookData);
                    dataToSend = EncryptionService.Encrypt(jsonData);
                    _logger.LogInformation("Sending encrypted notification");
                }
                else
                {
                    // Send data without encryption
                    dataToSend = JsonSerializer.Serialize(webhookData);
                    _logger.LogInformation("Sending unencrypted notification");
                }

                // Send notification to all connected clients
                await _hubContext.Clients.All.SendAsync("ReceiveNotification", dataToSend);

                return Ok(new { success = true, message = "Webhook received and processed" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing webhook");
                return StatusCode(500, new { success = false, message = "Error processing webhook" });
            }
        }

        [HttpGet]
        public IActionResult Test()
        {
            return Ok(new { status = "Webhook receiver is running" });
        }
    }
}
