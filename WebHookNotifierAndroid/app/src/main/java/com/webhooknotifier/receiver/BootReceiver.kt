package com.webhooknotifier.receiver

import android.content.BroadcastReceiver
import android.content.Context
import android.content.Intent
import com.webhooknotifier.service.NotificationService
import dagger.hilt.android.AndroidEntryPoint

@AndroidEntryPoint
class BootReceiver : BroadcastReceiver() {
    
    override fun onReceive(context: Context, intent: Intent) {
        if (intent.action == Intent.ACTION_BOOT_COMPLETED) {
            val serviceIntent = Intent(context, NotificationService::class.java)
            context.startForegroundService(serviceIntent)
        }
    }
}
