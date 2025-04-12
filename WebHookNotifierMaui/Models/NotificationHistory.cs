using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace WebHookNotifierMaui.Models
{
    /// <summary>
    /// Represents a stored notification in the history database
    /// </summary>
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
        public Dictionary<string, object>? AdditionalData
        {
            get
            {
                if (string.IsNullOrEmpty(AdditionalDataJson))
                    return null;

                try
                {
                    return JsonSerializer.Deserialize<Dictionary<string, object>>(AdditionalDataJson);
                }
                catch
                {
                    return null;
                }
            }
            set
            {
                if (value == null)
                {
                    AdditionalDataJson = null;
                }
                else
                {
                    AdditionalDataJson = JsonSerializer.Serialize(value);
                }
            }
        }

        /// <summary>
        /// Creates a NotificationHistory from a WebhookData
        /// </summary>
        public static NotificationHistory FromWebhookData(WebhookData webhookData)
        {
            return new NotificationHistory
            {
                Event = webhookData.Event ?? string.Empty,
                Message = webhookData.Message,
                Timestamp = webhookData.Timestamp,
                AdditionalData = webhookData.AdditionalData
            };
        }

        /// <summary>
        /// Converts this history item back to a WebhookData object
        /// </summary>
        public WebhookData ToWebhookData()
        {
            return new WebhookData
            {
                Event = Event,
                Message = Message,
                Timestamp = Timestamp,
                AdditionalData = AdditionalData
            };
        }
    }
}
