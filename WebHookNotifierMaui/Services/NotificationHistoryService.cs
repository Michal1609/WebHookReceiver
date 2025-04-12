using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebHookNotifierMaui.Data;
using WebHookNotifierMaui.Models;

namespace WebHookNotifierMaui.Services
{
    /// <summary>
    /// Service for managing notification history
    /// </summary>
    public class NotificationHistoryService
    {
        private readonly NotificationDbContext _dbContext;
        private readonly NotificationSettings _settings;
        
        public NotificationHistoryService(NotificationDbContext dbContext, NotificationSettings settings)
        {
            _dbContext = dbContext;
            _settings = settings;
        }
        
        /// <summary>
        /// Adds a notification to the history database
        /// </summary>
        public async Task AddNotificationAsync(WebhookData notification)
        {
            if (!_settings.EnableHistoryTracking)
                return;
                
            var historyItem = NotificationHistory.FromWebhookData(notification);
            
            _dbContext.Notifications.Add(historyItem);
            await _dbContext.SaveChangesAsync();
        }
        
        /// <summary>
        /// Gets all notifications from the history database
        /// </summary>
        public async Task<List<NotificationHistory>> GetAllNotificationsAsync()
        {
            return await _dbContext.Notifications
                .OrderByDescending(n => n.Timestamp)
                .ToListAsync();
        }
        
        /// <summary>
        /// Gets notifications filtered by search criteria
        /// </summary>
        public async Task<List<NotificationHistory>> SearchNotificationsAsync(
            string? searchText = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            string? eventType = null)
        {
            IQueryable<NotificationHistory> query = _dbContext.Notifications;
            
            // Apply filters
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                string search = searchText.ToLower();
                query = query.Where(n => 
                    (n.Event != null && n.Event.ToLower().Contains(search)) ||
                    (n.Message != null && n.Message.ToLower().Contains(search)) ||
                    (n.AdditionalDataJson != null && n.AdditionalDataJson.ToLower().Contains(search)));
            }
            
            if (fromDate.HasValue)
            {
                query = query.Where(n => n.Timestamp >= fromDate.Value);
            }
            
            if (toDate.HasValue)
            {
                // Add one day to include the entire end date
                DateTime endDate = toDate.Value.AddDays(1).AddSeconds(-1);
                query = query.Where(n => n.Timestamp <= endDate);
            }
            
            if (!string.IsNullOrWhiteSpace(eventType))
            {
                query = query.Where(n => n.Event == eventType);
            }
            
            return await query
                .OrderByDescending(n => n.Timestamp)
                .ToListAsync();
        }
        
        /// <summary>
        /// Gets a list of all unique event types in the database
        /// </summary>
        public async Task<List<string>> GetEventTypesAsync()
        {
            return await _dbContext.Notifications
                .Select(n => n.Event)
                .Distinct()
                .OrderBy(e => e)
                .ToListAsync();
        }
        
        /// <summary>
        /// Exports notifications to a CSV file
        /// </summary>
        public async Task<string> ExportToCsvAsync(List<NotificationHistory>? notifications = null)
        {
            if (notifications == null)
            {
                notifications = await GetAllNotificationsAsync();
            }
            
            StringBuilder csv = new StringBuilder();
            
            // Add header
            csv.AppendLine("Id,Event,Message,Timestamp,AdditionalData");
            
            // Add data rows
            foreach (var notification in notifications)
            {
                string message = notification.Message?.Replace("\"", "\"\"") ?? "";
                string additionalData = notification.AdditionalDataJson?.Replace("\"", "\"\"") ?? "";
                
                csv.AppendLine($"{notification.Id},\"{notification.Event}\",\"{message}\",\"{notification.Timestamp:yyyy-MM-dd HH:mm:ss}\",\"{additionalData}\"");
            }
            
            return csv.ToString();
        }
        
        /// <summary>
        /// Exports notifications to a JSON file
        /// </summary>
        public async Task<string> ExportToJsonAsync(List<NotificationHistory>? notifications = null)
        {
            if (notifications == null)
            {
                notifications = await GetAllNotificationsAsync();
            }
            
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            
            return JsonSerializer.Serialize(notifications, options);
        }
        
        /// <summary>
        /// Deletes notifications older than the specified number of days
        /// </summary>
        public async Task CleanupOldNotificationsAsync(int olderThanDays)
        {
            if (olderThanDays <= 0)
                return;
                
            DateTime cutoffDate = DateTime.Now.AddDays(-olderThanDays);
            
            var oldNotifications = await _dbContext.Notifications
                .Where(n => n.Timestamp < cutoffDate)
                .ToListAsync();
                
            if (oldNotifications.Any())
            {
                _dbContext.Notifications.RemoveRange(oldNotifications);
                await _dbContext.SaveChangesAsync();
            }
        }
        
        /// <summary>
        /// Saves a notification to a file
        /// </summary>
        public async Task<string> SaveToFileAsync(string format, List<NotificationHistory>? notifications = null)
        {
            string content;
            string extension;
            
            if (format.Equals("csv", StringComparison.OrdinalIgnoreCase))
            {
                content = await ExportToCsvAsync(notifications);
                extension = "csv";
            }
            else
            {
                content = await ExportToJsonAsync(notifications);
                extension = "json";
            }
            
            string fileName = $"notifications_{DateTime.Now:yyyyMMdd_HHmmss}.{extension}";
            string filePath = Path.Combine(FileSystem.CacheDirectory, fileName);
            
            await File.WriteAllTextAsync(filePath, content);
            
            return filePath;
        }
    }
}
