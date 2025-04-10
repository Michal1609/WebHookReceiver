package com.webhooknotifier.ui.viewmodels

import android.content.Context
import android.net.Uri
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.webhooknotifier.data.model.NotificationHistory
import com.webhooknotifier.data.repository.NotificationRepository
import com.webhooknotifier.util.ExportUtil
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.SharingStarted
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.flow.catch
import kotlinx.coroutines.flow.collectLatest
import kotlinx.coroutines.flow.stateIn
import kotlinx.coroutines.launch
import java.util.Date
import javax.inject.Inject

@HiltViewModel
class HistoryViewModel @Inject constructor(
    private val notificationRepository: NotificationRepository,
    private val exportUtil: ExportUtil
) : ViewModel() {
    
    private val _notifications = MutableStateFlow<List<NotificationHistory>>(emptyList())
    val notifications: StateFlow<List<NotificationHistory>> = _notifications.asStateFlow()
    
    private val _isLoading = MutableStateFlow(false)
    val isLoading: StateFlow<Boolean> = _isLoading.asStateFlow()
    
    val eventTypes: StateFlow<List<String>> = notificationRepository.getAllEventTypes()
        .catch { emit(emptyList()) }
        .stateIn(
            viewModelScope,
            SharingStarted.WhileSubscribed(5000),
            emptyList()
        )
    
    fun loadNotifications() {
        _isLoading.value = true
        viewModelScope.launch {
            notificationRepository.getAllNotifications()
                .catch {
                    _isLoading.value = false
                    _notifications.value = emptyList()
                }
                .collectLatest {
                    _notifications.value = it
                    _isLoading.value = false
                }
        }
    }
    
    fun searchNotifications(
        searchQuery: String = "",
        eventType: String? = null,
        fromDate: Date? = null,
        toDate: Date? = null
    ) {
        _isLoading.value = true
        viewModelScope.launch {
            notificationRepository.searchNotifications(searchQuery, eventType, fromDate, toDate)
                .catch {
                    _isLoading.value = false
                    _notifications.value = emptyList()
                }
                .collectLatest {
                    _notifications.value = it
                    _isLoading.value = false
                }
        }
    }
    
    fun exportToCsv(context: Context, uri: Uri, notifications: List<NotificationHistory>): Boolean {
        return exportUtil.exportToCsv(context, uri, notifications)
    }
    
    fun exportToJson(context: Context, uri: Uri, notifications: List<NotificationHistory>): Boolean {
        return exportUtil.exportToJson(context, uri, notifications)
    }
    
    fun clearAllNotifications() {
        viewModelScope.launch {
            notificationRepository.clearAllNotifications()
            loadNotifications()
        }
    }
}
