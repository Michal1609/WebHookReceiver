using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using WebHookNotifier.Data;
using WebHookNotifier.Models;

namespace WebHookNotifier.Services
{
    /// <summary>
    /// Service for initializing and managing the database connection
    /// </summary>
    public class DatabaseService
    {
        private readonly NotificationSettings _settings;
        private NotificationDbContext? _dbContext;
        
        public DatabaseService(NotificationSettings settings)
        {
            _settings = settings;
        }
        
        /// <summary>
        /// Initializes the database context based on the current settings
        /// </summary>
        public NotificationDbContext InitializeDatabase()
        {
            if (_dbContext != null)
            {
                return _dbContext;
            }
            
            var optionsBuilder = new DbContextOptionsBuilder<NotificationDbContext>();
            
            if (_settings.DatabaseType == DatabaseType.SQLite)
            {
                // Use SQLite database
                string dbPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "WebHookNotifier",
                    "notifications.db");
                
                // Ensure directory exists
                string? directory = Path.GetDirectoryName(dbPath);
                if (directory != null && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                optionsBuilder.UseSqlite($"Data Source={dbPath}");
            }
            else if (_settings.DatabaseType == DatabaseType.SQLServer)
            {
                // Use SQL Server database
                string connectionString = _settings.SqlServerConnectionString;
                
                if (string.IsNullOrEmpty(connectionString))
                {
                    // Fallback to SQLite if connection string is empty
                    _settings.DatabaseType = DatabaseType.SQLite;
                    return InitializeDatabase();
                }
                
                optionsBuilder.UseSqlServer(connectionString);
            }
            
            _dbContext = new NotificationDbContext(optionsBuilder.Options);
            
            // Ensure database is created
            _dbContext.Database.EnsureCreated();
            
            return _dbContext;
        }
        
        /// <summary>
        /// Closes the database connection
        /// </summary>
        public void CloseDatabase()
        {
            if (_dbContext != null)
            {
                _dbContext.Dispose();
                _dbContext = null;
            }
        }
    }
}
