package com.webhooknotifier.ui

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.webhooknotifier.data.model.NotificationSettings
import com.webhooknotifier.data.repository.SettingsRepository
import com.webhooknotifier.service.NotificationService
import com.webhooknotifier.service.SignalRService
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.SharingStarted
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.stateIn
import kotlinx.coroutines.launch
import javax.inject.Inject

@HiltViewModel
class MainViewModel @Inject constructor(
    private val signalRService: SignalRService,
    private val settingsRepository: SettingsRepository
) : ViewModel() {
    
    val settings: StateFlow<NotificationSettings> = settingsRepository.settingsFlow
        .stateIn(
            viewModelScope,
            SharingStarted.WhileSubscribed(5000),
            NotificationSettings()
        )
    
    val connectionState: StateFlow<SignalRService.ConnectionState> = signalRService.connectionState
        .stateIn(
            viewModelScope,
            SharingStarted.WhileSubscribed(5000),
            SignalRService.ConnectionState.DISCONNECTED
        )
    
    val serviceState: StateFlow<NotificationService.ServiceState> = signalRService.connectionState
        .stateIn(
            viewModelScope,
            SharingStarted.WhileSubscribed(5000),
            NotificationService.ServiceState.STOPPED
        )
    
    fun connect() {
        viewModelScope.launch {
            val currentSettings = settings.value
            if (currentSettings.serverUrl.isNotEmpty()) {
                signalRService.connect(currentSettings.serverUrl, currentSettings.enableEncryption)
            }
        }
    }
    
    fun disconnect() {
        signalRService.disconnect()
    }
    
    fun updateSettings(settings: NotificationSettings) {
        viewModelScope.launch {
            settingsRepository.updateSettings(settings)
        }
    }
}
