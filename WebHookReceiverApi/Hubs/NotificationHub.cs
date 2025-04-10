using Microsoft.AspNetCore.SignalR;
using WebHookReceiverApi.Models;

namespace WebHookReceiverApi.Hubs
{
    public class NotificationHub : Hub
    {
        public async Task SendNotification(WebhookData data)
        {
            await Clients.All.SendAsync("ReceiveNotification", data);
        }
    }
}
