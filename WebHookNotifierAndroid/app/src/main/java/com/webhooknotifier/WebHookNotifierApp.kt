package com.webhooknotifier

import android.app.Application
import android.app.NotificationChannel
import android.app.NotificationManager
import android.content.Context
import android.os.Build
import dagger.hilt.android.HiltAndroidApp

@HiltAndroidApp
class WebHookNotifierApp : Application() {
    
    override fun onCreate() {
        super.onCreate()
        createNotificationChannel()
    }
    
    private fun createNotificationChannel() {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
            // Create the notification channel for webhook notifications
            val webhookChannel = NotificationChannel(
                WEBHOOK_CHANNEL_ID,
                getString(R.string.notification_channel_name),
                NotificationManager.IMPORTANCE_DEFAULT
            ).apply {
                description = getString(R.string.notification_channel_description)
                enableVibration(true)
            }
            
            // Create the notification channel for foreground service
            val serviceChannel = NotificationChannel(
                SERVICE_CHANNEL_ID,
                "WebHook Service",
                NotificationManager.IMPORTANCE_LOW
            ).apply {
                description = "Background service notification"
            }
            
            val notificationManager = getSystemService(Context.NOTIFICATION_SERVICE) as NotificationManager
            notificationManager.createNotificationChannel(webhookChannel)
            notificationManager.createNotificationChannel(serviceChannel)
        }
    }
    
    companion object {
        const val WEBHOOK_CHANNEL_ID = "webhook_notifications"
        const val SERVICE_CHANNEL_ID = "webhook_service"
    }
}
