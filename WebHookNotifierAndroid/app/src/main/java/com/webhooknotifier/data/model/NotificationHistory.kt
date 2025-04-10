package com.webhooknotifier.data.model

import androidx.room.Entity
import androidx.room.PrimaryKey
import androidx.room.TypeConverters
import com.webhooknotifier.data.db.Converters
import java.util.Date

@Entity(tableName = "notification_history")
@TypeConverters(Converters::class)
data class NotificationHistory(
    @PrimaryKey(autoGenerate = true)
    val id: Long = 0,
    val event: String,
    val message: String?,
    val timestamp: Date,
    val additionalData: String? // JSON string
) {
    companion object {
        fun fromWebhookData(webhookData: WebhookData): NotificationHistory {
            return NotificationHistory(
                event = webhookData.event,
                message = webhookData.message,
                timestamp = webhookData.timestamp,
                additionalData = webhookData.additionalData?.let { 
                    com.google.gson.Gson().toJson(it) 
                }
            )
        }
    }
    
    fun toWebhookData(): WebhookData {
        val additionalDataMap = if (additionalData != null) {
            try {
                com.google.gson.Gson().fromJson(
                    additionalData, 
                    object : com.google.gson.reflect.TypeToken<Map<String, Any>>() {}.type
                )
            } catch (e: Exception) {
                null
            }
        } else null
        
        return WebhookData(
            event = event,
            message = message,
            timestamp = timestamp,
            additionalData = additionalDataMap
        )
    }
}
