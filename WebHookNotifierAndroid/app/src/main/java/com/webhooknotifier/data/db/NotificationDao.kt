package com.webhooknotifier.data.db

import androidx.room.Dao
import androidx.room.Insert
import androidx.room.Query
import com.webhooknotifier.data.model.NotificationHistory
import kotlinx.coroutines.flow.Flow
import java.util.Date

@Dao
interface NotificationDao {
    @Insert
    suspend fun insertNotification(notification: NotificationHistory): Long
    
    @Query("SELECT * FROM notification_history ORDER BY timestamp DESC")
    fun getAllNotifications(): Flow<List<NotificationHistory>>
    
    @Query("SELECT * FROM notification_history WHERE id = :id")
    suspend fun getNotificationById(id: Long): NotificationHistory?
    
    @Query("SELECT * FROM notification_history WHERE event = :eventType ORDER BY timestamp DESC")
    fun getNotificationsByEventType(eventType: String): Flow<List<NotificationHistory>>
    
    @Query("SELECT * FROM notification_history WHERE timestamp BETWEEN :fromDate AND :toDate ORDER BY timestamp DESC")
    fun getNotificationsByDateRange(fromDate: Date, toDate: Date): Flow<List<NotificationHistory>>
    
    @Query("SELECT * FROM notification_history WHERE (event LIKE '%' || :searchQuery || '%' OR message LIKE '%' || :searchQuery || '%' OR additionalData LIKE '%' || :searchQuery || '%') ORDER BY timestamp DESC")
    fun searchNotifications(searchQuery: String): Flow<List<NotificationHistory>>
    
    @Query("SELECT * FROM notification_history WHERE (event LIKE '%' || :searchQuery || '%' OR message LIKE '%' || :searchQuery || '%' OR additionalData LIKE '%' || :searchQuery || '%') AND timestamp BETWEEN :fromDate AND :toDate ORDER BY timestamp DESC")
    fun searchNotificationsWithDateRange(searchQuery: String, fromDate: Date, toDate: Date): Flow<List<NotificationHistory>>
    
    @Query("SELECT * FROM notification_history WHERE (event LIKE '%' || :searchQuery || '%' OR message LIKE '%' || :searchQuery || '%' OR additionalData LIKE '%' || :searchQuery || '%') AND event = :eventType ORDER BY timestamp DESC")
    fun searchNotificationsWithEventType(searchQuery: String, eventType: String): Flow<List<NotificationHistory>>
    
    @Query("SELECT * FROM notification_history WHERE (event LIKE '%' || :searchQuery || '%' OR message LIKE '%' || :searchQuery || '%' OR additionalData LIKE '%' || :searchQuery || '%') AND event = :eventType AND timestamp BETWEEN :fromDate AND :toDate ORDER BY timestamp DESC")
    fun searchNotificationsWithEventTypeAndDateRange(searchQuery: String, eventType: String, fromDate: Date, toDate: Date): Flow<List<NotificationHistory>>
    
    @Query("SELECT DISTINCT event FROM notification_history ORDER BY event")
    fun getAllEventTypes(): Flow<List<String>>
    
    @Query("DELETE FROM notification_history WHERE timestamp < :cutoffDate")
    suspend fun deleteOldNotifications(cutoffDate: Date): Int
    
    @Query("DELETE FROM notification_history")
    suspend fun clearAllNotifications(): Int
}
