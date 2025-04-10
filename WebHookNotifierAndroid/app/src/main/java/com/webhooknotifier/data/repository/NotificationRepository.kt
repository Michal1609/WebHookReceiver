package com.webhooknotifier.data.repository

import com.webhooknotifier.data.db.NotificationDao
import com.webhooknotifier.data.model.NotificationHistory
import com.webhooknotifier.data.model.WebhookData
import kotlinx.coroutines.flow.Flow
import java.util.Calendar
import java.util.Date
import javax.inject.Inject
import javax.inject.Singleton

@Singleton
class NotificationRepository @Inject constructor(
    private val notificationDao: NotificationDao
) {
    suspend fun saveNotification(webhookData: WebhookData) {
        val notificationHistory = NotificationHistory.fromWebhookData(webhookData)
        notificationDao.insertNotification(notificationHistory)
    }
    
    fun getAllNotifications(): Flow<List<NotificationHistory>> {
        return notificationDao.getAllNotifications()
    }
    
    fun getNotificationsByEventType(eventType: String): Flow<List<NotificationHistory>> {
        return notificationDao.getNotificationsByEventType(eventType)
    }
    
    fun getNotificationsByDateRange(fromDate: Date, toDate: Date): Flow<List<NotificationHistory>> {
        return notificationDao.getNotificationsByDateRange(fromDate, toDate)
    }
    
    fun searchNotifications(
        searchQuery: String = "",
        eventType: String? = null,
        fromDate: Date? = null,
        toDate: Date? = null
    ): Flow<List<NotificationHistory>> {
        val hasEventType = !eventType.isNullOrEmpty() && eventType != "all"
        val hasDateRange = fromDate != null && toDate != null
        
        return when {
            hasEventType && hasDateRange -> {
                notificationDao.searchNotificationsWithEventTypeAndDateRange(
                    searchQuery, eventType!!, fromDate!!, toDate!!
                )
            }
            hasEventType -> {
                notificationDao.searchNotificationsWithEventType(searchQuery, eventType!!)
            }
            hasDateRange -> {
                notificationDao.searchNotificationsWithDateRange(searchQuery, fromDate!!, toDate!!)
            }
            else -> {
                notificationDao.searchNotifications(searchQuery)
            }
        }
    }
    
    fun getAllEventTypes(): Flow<List<String>> {
        return notificationDao.getAllEventTypes()
    }
    
    suspend fun cleanupOldNotifications(retentionDays: Int) {
        val calendar = Calendar.getInstance()
        calendar.add(Calendar.DAY_OF_YEAR, -retentionDays)
        val cutoffDate = calendar.time
        
        notificationDao.deleteOldNotifications(cutoffDate)
    }
    
    suspend fun clearAllNotifications(): Int {
        return notificationDao.clearAllNotifications()
    }
}
