package com.webhooknotifier.data.db

import androidx.room.Database
import androidx.room.RoomDatabase
import androidx.room.TypeConverters
import com.webhooknotifier.data.model.NotificationHistory

@Database(
    entities = [NotificationHistory::class],
    version = 1,
    exportSchema = false
)
@TypeConverters(Converters::class)
abstract class NotificationDatabase : RoomDatabase() {
    abstract fun notificationDao(): NotificationDao
}
