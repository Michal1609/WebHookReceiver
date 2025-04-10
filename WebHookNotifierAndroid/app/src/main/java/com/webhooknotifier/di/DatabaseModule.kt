package com.webhooknotifier.di

import android.content.Context
import androidx.room.Room
import com.webhooknotifier.data.db.NotificationDao
import com.webhooknotifier.data.db.NotificationDatabase
import dagger.Module
import dagger.Provides
import dagger.hilt.InstallIn
import dagger.hilt.android.qualifiers.ApplicationContext
import dagger.hilt.components.SingletonComponent
import javax.inject.Singleton

@Module
@InstallIn(SingletonComponent::class)
object DatabaseModule {
    
    @Provides
    @Singleton
    fun provideNotificationDatabase(@ApplicationContext context: Context): NotificationDatabase {
        return Room.databaseBuilder(
            context,
            NotificationDatabase::class.java,
            "notification_database"
        ).build()
    }
    
    @Provides
    fun provideNotificationDao(database: NotificationDatabase): NotificationDao {
        return database.notificationDao()
    }
}
