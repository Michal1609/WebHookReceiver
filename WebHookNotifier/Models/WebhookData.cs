using System;
using System.Collections.Generic;

namespace WebHookNotifier.Models
{
    public class WebhookData
    {
        public string? Id { get; set; }
        public string? Event { get; set; }
        public string? Message { get; set; }
        public DateTime Timestamp { get; set; }
        public Dictionary<string, object>? AdditionalData { get; set; }
    }
}
