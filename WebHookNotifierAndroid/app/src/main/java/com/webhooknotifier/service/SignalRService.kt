package com.webhooknotifier.service

import android.util.Log
import com.google.gson.Gson
import com.microsoft.signalr.HubConnection
import com.microsoft.signalr.HubConnectionBuilder
import com.microsoft.signalr.HubConnectionState
import com.webhooknotifier.data.model.WebhookData
import com.webhooknotifier.util.EncryptionUtil
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import javax.inject.Inject
import javax.inject.Singleton

@Singleton
class SignalRService @Inject constructor(
    private val encryptionUtil: EncryptionUtil
) {
    private var hubConnection: HubConnection? = null
    private val gson = Gson()
    
    private val _connectionState = MutableStateFlow(ConnectionState.DISCONNECTED)
    val connectionState: StateFlow<ConnectionState> = _connectionState.asStateFlow()
    
    private val _notifications = MutableStateFlow<WebhookData?>(null)
    val notifications: StateFlow<WebhookData?> = _notifications.asStateFlow()
    
    fun connect(serverUrl: String, useEncryption: Boolean) {
        if (hubConnection?.connectionState == HubConnectionState.CONNECTED) {
            return
        }
        
        try {
            _connectionState.value = ConnectionState.CONNECTING
            
            hubConnection = HubConnectionBuilder.create(serverUrl).build()
            
            hubConnection?.on("ReceiveNotification", { encryptedData: String ->
                try {
                    val webhookData = if (useEncryption) {
                        // Decrypt the data
                        val decryptedJson = encryptionUtil.decrypt(encryptedData)
                        gson.fromJson(decryptedJson, WebhookData::class.java)
                    } else {
                        // Try direct deserialization first
                        try {
                            gson.fromJson(encryptedData, WebhookData::class.java)
                        } catch (e: Exception) {
                            // If direct deserialization fails, try decryption as fallback
                            val decryptedJson = encryptionUtil.decrypt(encryptedData)
                            gson.fromJson(decryptedJson, WebhookData::class.java)
                        }
                    }
                    
                    _notifications.value = webhookData
                } catch (e: Exception) {
                    Log.e(TAG, "Error processing notification: ${e.message}")
                }
            }, String::class.java)
            
            hubConnection?.start()?.subscribe(
                {
                    _connectionState.value = ConnectionState.CONNECTED
                    Log.d(TAG, "SignalR connected to $serverUrl")
                },
                { error ->
                    _connectionState.value = ConnectionState.ERROR
                    Log.e(TAG, "Error connecting to SignalR: ${error.message}")
                }
            )
        } catch (e: Exception) {
            _connectionState.value = ConnectionState.ERROR
            Log.e(TAG, "Error setting up SignalR: ${e.message}")
        }
    }
    
    fun disconnect() {
        try {
            hubConnection?.stop()
            _connectionState.value = ConnectionState.DISCONNECTED
            Log.d(TAG, "SignalR disconnected")
        } catch (e: Exception) {
            Log.e(TAG, "Error disconnecting from SignalR: ${e.message}")
        }
    }
    
    fun isConnected(): Boolean {
        return hubConnection?.connectionState == HubConnectionState.CONNECTED
    }
    
    enum class ConnectionState {
        DISCONNECTED,
        CONNECTING,
        CONNECTED,
        ERROR
    }
    
    companion object {
        private const val TAG = "SignalRService"
    }
}
