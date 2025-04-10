using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using WebHookReceiverApi.Hubs;
using WebHookReceiverApi.Models;

namespace WebHookReceiverApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WebhookController : ControllerBase
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<WebhookController> _logger;

        public WebhookController(IHubContext<NotificationHub> hubContext, ILogger<WebhookController> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
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

                // Send notification to all connected clients
                await _hubContext.Clients.All.SendAsync("ReceiveNotification", webhookData);

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
