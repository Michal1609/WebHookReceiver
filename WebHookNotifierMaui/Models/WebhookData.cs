using System;
using System.Collections.Generic;

namespace WebHookNotifierMaui.Models
{
    /// <summary>
    /// Represents data received from a webhook
    /// </summary>
    public class WebhookData
    {
        /// <summary>
        /// Unique identifier for the webhook notification
        /// </summary>
        public string? Id { get; set; }
        
        /// <summary>
        /// Event type or name
        /// </summary>
        public string? Event { get; set; }
        
        /// <summary>
        /// Main message content
        /// </summary>
        public string? Message { get; set; }
        
        /// <summary>
        /// When the webhook was received
        /// </summary>
        public DateTime Timestamp { get; set; }
        
        /// <summary>
        /// Additional data provided with the webhook
        /// </summary>
        public Dictionary<string, object>? AdditionalData { get; set; }
    }
}
