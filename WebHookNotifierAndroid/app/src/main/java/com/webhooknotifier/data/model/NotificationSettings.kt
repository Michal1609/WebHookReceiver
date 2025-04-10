package com.webhooknotifier.data.model

data class NotificationSettings(
    // Connection settings
    var serverUrl: String = "",
    
    // Rate limiting settings
    var minSecondsBetweenNotifications: Int = 2,
    var maxQueuedNotifications: Int = 5,
    var enableNotificationSounds: Boolean = true,
    
    // Security settings
    var enableEncryption: Boolean = true,
    
    // History settings
    var enableHistoryTracking: Boolean = true,
    var historyRetentionDays: Int = 30,
    
    // Database settings
    var databaseType: DatabaseType = DatabaseType.SQLITE,
    var sqlServerConnectionString: String = ""
)

enum class DatabaseType {
    SQLITE,
    SQL_SERVER
}
