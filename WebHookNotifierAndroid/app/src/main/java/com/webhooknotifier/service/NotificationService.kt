package com.webhooknotifier.service

import android.app.Notification
import android.app.PendingIntent
import android.app.Service
import android.content.Intent
import android.os.Binder
import android.os.IBinder
import android.util.Log
import androidx.core.app.NotificationCompat
import androidx.core.app.NotificationManagerCompat
import com.webhooknotifier.R
import com.webhooknotifier.WebHookNotifierApp.Companion.SERVICE_CHANNEL_ID
import com.webhooknotifier.WebHookNotifierApp.Companion.WEBHOOK_CHANNEL_ID
import com.webhooknotifier.data.model.NotificationSettings
import com.webhooknotifier.data.model.WebhookData
import com.webhooknotifier.data.repository.NotificationRepository
import com.webhooknotifier.data.repository.SettingsRepository
import com.webhooknotifier.ui.MainActivity
import com.webhooknotifier.util.NotificationFormatter
import dagger.hilt.android.AndroidEntryPoint
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.Job
import kotlinx.coroutines.SupervisorJob
import kotlinx.coroutines.delay
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.flow.collectLatest
import kotlinx.coroutines.flow.first
import kotlinx.coroutines.launch
import kotlinx.coroutines.withContext
import java.util.LinkedList
import java.util.Queue
import javax.inject.Inject

@AndroidEntryPoint
class NotificationService : Service() {
    
    @Inject
    lateinit var signalRService: SignalRService
    
    @Inject
    lateinit var settingsRepository: SettingsRepository
    
    @Inject
    lateinit var notificationRepository: NotificationRepository
    
    @Inject
    lateinit var notificationFormatter: NotificationFormatter
    
    private val serviceScope = CoroutineScope(SupervisorJob() + Dispatchers.IO)
    private var notificationJob: Job? = null
    
    private val binder = LocalBinder()
    
    // Rate limiting
    private val notificationQueue: Queue<WebhookData> = LinkedList()
    private var lastNotificationTime = 0L
    private var processingQueue = false
    
    // Service state
    private val _serviceState = MutableStateFlow(ServiceState.STOPPED)
    val serviceState: StateFlow<ServiceState> = _serviceState.asStateFlow()
    
    override fun onCreate() {
        super.onCreate()
        startForeground(FOREGROUND_NOTIFICATION_ID, createForegroundNotification())
        
        // Collect notifications from SignalR
        serviceScope.launch {
            signalRService.notifications.collectLatest { webhookData ->
                webhookData?.let { handleNotification(it) }
            }
        }
        
        // Collect connection state changes
        serviceScope.launch {
            signalRService.connectionState.collectLatest { state ->
                when (state) {
                    SignalRService.ConnectionState.CONNECTED -> {
                        _serviceState.value = ServiceState.RUNNING
                    }
                    SignalRService.ConnectionState.DISCONNECTED -> {
                        _serviceState.value = ServiceState.STOPPED
                    }
                    SignalRService.ConnectionState.ERROR -> {
                        _serviceState.value = ServiceState.ERROR
                    }
                    else -> {}
                }
            }
        }
        
        // Cleanup old notifications periodically
        serviceScope.launch {
            while (true) {
                try {
                    val settings = settingsRepository.settingsFlow.first()
                    if (settings.enableHistoryTracking) {
                        notificationRepository.cleanupOldNotifications(settings.historyRetentionDays)
                    }
                } catch (e: Exception) {
                    Log.e(TAG, "Error cleaning up old notifications: ${e.message}")
                }
                
                // Run cleanup once a day
                delay(24 * 60 * 60 * 1000)
            }
        }
    }
    
    override fun onStartCommand(intent: Intent?, flags: Int, startId: Int): Int {
        serviceScope.launch {
            val settings = settingsRepository.settingsFlow.first()
            if (settings.serverUrl.isNotEmpty()) {
                signalRService.connect(settings.serverUrl, settings.enableEncryption)
            }
        }
        
        return START_STICKY
    }
    
    override fun onBind(intent: Intent?): IBinder {
        return binder
    }
    
    override fun onDestroy() {
        signalRService.disconnect()
        notificationJob?.cancel()
        super.onDestroy()
    }
    
    private suspend fun handleNotification(webhookData: WebhookData) {
        // Save to history if enabled
        val settings = settingsRepository.settingsFlow.first()
        if (settings.enableHistoryTracking) {
            try {
                notificationRepository.saveNotification(webhookData)
            } catch (e: Exception) {
                Log.e(TAG, "Error saving notification to history: ${e.message}")
            }
        }
        
        // Add to queue for rate limiting
        synchronized(notificationQueue) {
            notificationQueue.add(webhookData)
            
            // Trim queue if it gets too large
            while (notificationQueue.size > settings.maxQueuedNotifications) {
                notificationQueue.poll()
            }
            
            // Start processing if not already processing
            if (!processingQueue) {
                processingQueue = true
                notificationJob = serviceScope.launch {
                    processNotificationQueue(settings)
                }
            }
        }
    }
    
    private suspend fun processNotificationQueue(settings: NotificationSettings) {
        while (true) {
            val data: WebhookData?
            
            synchronized(notificationQueue) {
                data = notificationQueue.poll()
                if (data == null) {
                    processingQueue = false
                    break
                }
            }
            
            // Apply rate limiting
            val now = System.currentTimeMillis()
            val timeSinceLastNotification = now - lastNotificationTime
            val minTimeBetween = settings.minSecondsBetweenNotifications * 1000L
            
            if (timeSinceLastNotification < minTimeBetween) {
                delay(minTimeBetween - timeSinceLastNotification)
            }
            
            // Show notification
            showNotification(data)
            lastNotificationTime = System.currentTimeMillis()
        }
    }
    
    private suspend fun showNotification(data: WebhookData) {
        withContext(Dispatchers.Main) {
            try {
                val intent = Intent(this@NotificationService, MainActivity::class.java).apply {
                    flags = Intent.FLAG_ACTIVITY_NEW_TASK or Intent.FLAG_ACTIVITY_CLEAR_TASK
                    putExtra("notification_event", data.event)
                }
                
                val pendingIntent = PendingIntent.getActivity(
                    this@NotificationService, 0, intent,
                    PendingIntent.FLAG_UPDATE_CURRENT or PendingIntent.FLAG_IMMUTABLE
                )
                
                val title = "Webhook: ${data.event}"
                val formattedMessage = notificationFormatter.formatMessage(data)
                
                val notification = NotificationCompat.Builder(this@NotificationService, WEBHOOK_CHANNEL_ID)
                    .setSmallIcon(R.drawable.ic_notification)
                    .setContentTitle(title)
                    .setContentText(formattedMessage)
                    .setStyle(NotificationCompat.BigTextStyle().bigText(formattedMessage))
                    .setPriority(NotificationCompat.PRIORITY_DEFAULT)
                    .setContentIntent(pendingIntent)
                    .setAutoCancel(true)
                    .build()
                
                with(NotificationManagerCompat.from(this@NotificationService)) {
                    notify(data.hashCode(), notification)
                }
            } catch (e: Exception) {
                Log.e(TAG, "Error showing notification: ${e.message}")
            }
        }
    }
    
    private fun createForegroundNotification(): Notification {
        val notificationIntent = Intent(this, MainActivity::class.java)
        val pendingIntent = PendingIntent.getActivity(
            this, 0, notificationIntent,
            PendingIntent.FLAG_IMMUTABLE
        )
        
        return NotificationCompat.Builder(this, SERVICE_CHANNEL_ID)
            .setSmallIcon(R.drawable.ic_notification)
            .setContentTitle(getString(R.string.service_notification_title))
            .setContentText(getString(R.string.service_notification_text))
            .setContentIntent(pendingIntent)
            .setPriority(NotificationCompat.PRIORITY_LOW)
            .build()
    }
    
    inner class LocalBinder : Binder() {
        fun getService(): NotificationService = this@NotificationService
    }
    
    enum class ServiceState {
        STOPPED,
        RUNNING,
        ERROR
    }
    
    companion object {
        private const val TAG = "NotificationService"
        private const val FOREGROUND_NOTIFICATION_ID = 1
    }
}
