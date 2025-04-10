package com.webhooknotifier.ui.screens

import android.app.DatePickerDialog
import android.content.Context
import android.content.Intent
import android.net.Uri
import android.widget.Toast
import androidx.activity.compose.rememberLauncherForActivityResult
import androidx.activity.result.contract.ActivityResultContracts
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.width
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.ArrowBack
import androidx.compose.material.icons.filled.ArrowDropDown
import androidx.compose.material.icons.filled.Clear
import androidx.compose.material.icons.filled.Delete
import androidx.compose.material.icons.filled.FileDownload
import androidx.compose.material.icons.filled.Search
import androidx.compose.material3.AlertDialog
import androidx.compose.material3.Button
import androidx.compose.material3.Card
import androidx.compose.material3.CircularProgressIndicator
import androidx.compose.material3.DropdownMenu
import androidx.compose.material3.DropdownMenuItem
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedTextField
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.material3.TextButton
import androidx.compose.material3.TopAppBar
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextOverflow
import androidx.compose.ui.unit.dp
import androidx.hilt.navigation.compose.hiltViewModel
import com.webhooknotifier.R
import com.webhooknotifier.data.model.NotificationHistory
import com.webhooknotifier.ui.viewmodels.HistoryViewModel
import java.text.SimpleDateFormat
import java.util.Calendar
import java.util.Date
import java.util.Locale

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun HistoryScreen(
    onNavigateBack: () -> Unit,
    viewModel: HistoryViewModel = hiltViewModel()
) {
    val context = LocalContext.current
    val notifications by viewModel.notifications.collectAsState()
    val eventTypes by viewModel.eventTypes.collectAsState()
    val isLoading by viewModel.isLoading.collectAsState()
    
    var searchQuery by remember { mutableStateOf("") }
    var selectedEventType by remember { mutableStateOf<String?>(null) }
    var fromDate by remember { mutableStateOf<Date?>(null) }
    var toDate by remember { mutableStateOf<Date?>(null) }
    var showFilterOptions by remember { mutableStateOf(false) }
    var showExportOptions by remember { mutableStateOf(false) }
    var showClearConfirmation by remember { mutableStateOf(false) }
    
    var selectedNotification by remember { mutableStateOf<NotificationHistory?>(null) }
    var showNotificationDetails by remember { mutableStateOf(false) }
    
    val dateFormat = SimpleDateFormat("yyyy-MM-dd", Locale.getDefault())
    val timeFormat = SimpleDateFormat("HH:mm:ss", Locale.getDefault())
    
    // Create date picker dialogs
    val fromDatePicker = remember {
        val calendar = Calendar.getInstance()
        DatePickerDialog(
            context,
            { _, year, month, day ->
                val newCalendar = Calendar.getInstance()
                newCalendar.set(year, month, day, 0, 0, 0)
                fromDate = newCalendar.time
                viewModel.searchNotifications(searchQuery, selectedEventType, fromDate, toDate)
            },
            calendar.get(Calendar.YEAR),
            calendar.get(Calendar.MONTH),
            calendar.get(Calendar.DAY_OF_MONTH)
        )
    }
    
    val toDatePicker = remember {
        val calendar = Calendar.getInstance()
        DatePickerDialog(
            context,
            { _, year, month, day ->
                val newCalendar = Calendar.getInstance()
                newCalendar.set(year, month, day, 23, 59, 59)
                toDate = newCalendar.time
                viewModel.searchNotifications(searchQuery, selectedEventType, fromDate, toDate)
            },
            calendar.get(Calendar.YEAR),
            calendar.get(Calendar.MONTH),
            calendar.get(Calendar.DAY_OF_MONTH)
        )
    }
    
    // Export file launchers
    val csvExportLauncher = rememberLauncherForActivityResult(
        contract = ActivityResultContracts.CreateDocument("text/csv")
    ) { uri ->
        uri?.let {
            val success = viewModel.exportToCsv(context, uri, notifications)
            val message = if (success) {
                context.getString(R.string.export_success, "CSV")
            } else {
                context.getString(R.string.export_failed, "CSV")
            }
            Toast.makeText(context, message, Toast.LENGTH_SHORT).show()
        }
    }
    
    val jsonExportLauncher = rememberLauncherForActivityResult(
        contract = ActivityResultContracts.CreateDocument("application/json")
    ) { uri ->
        uri?.let {
            val success = viewModel.exportToJson(context, uri, notifications)
            val message = if (success) {
                context.getString(R.string.export_success, "JSON")
            } else {
                context.getString(R.string.export_failed, "JSON")
            }
            Toast.makeText(context, message, Toast.LENGTH_SHORT).show()
        }
    }
    
    // Load notifications on first launch
    LaunchedEffect(Unit) {
        viewModel.loadNotifications()
    }
    
    Scaffold(
        topBar = {
            TopAppBar(
                title = { Text(stringResource(R.string.history)) },
                navigationIcon = {
                    IconButton(onClick = onNavigateBack) {
                        Icon(Icons.Default.ArrowBack, contentDescription = "Back")
                    }
                },
                actions = {
                    IconButton(onClick = { showFilterOptions = true }) {
                        Icon(Icons.Default.Search, contentDescription = "Filter")
                    }
                    
                    IconButton(onClick = { showExportOptions = true }) {
                        Icon(Icons.Default.FileDownload, contentDescription = "Export")
                    }
                    
                    IconButton(onClick = { showClearConfirmation = true }) {
                        Icon(Icons.Default.Delete, contentDescription = "Clear History")
                    }
                    
                    // Filter dropdown menu
                    DropdownMenu(
                        expanded = showFilterOptions,
                        onDismissRequest = { showFilterOptions = false }
                    ) {
                        Text(
                            text = "Filter Options",
                            fontWeight = FontWeight.Bold,
                            modifier = Modifier.padding(horizontal = 16.dp, vertical = 8.dp)
                        )
                        
                        OutlinedTextField(
                            value = searchQuery,
                            onValueChange = { 
                                searchQuery = it
                                viewModel.searchNotifications(it, selectedEventType, fromDate, toDate)
                            },
                            label = { Text("Search") },
                            modifier = Modifier
                                .fillMaxWidth()
                                .padding(horizontal = 16.dp, vertical = 8.dp),
                            trailingIcon = {
                                if (searchQuery.isNotEmpty()) {
                                    IconButton(onClick = { 
                                        searchQuery = ""
                                        viewModel.searchNotifications("", selectedEventType, fromDate, toDate)
                                    }) {
                                        Icon(Icons.Default.Clear, contentDescription = "Clear")
                                    }
                                }
                            }
                        )
                        
                        Text(
                            text = "Event Type",
                            modifier = Modifier.padding(horizontal = 16.dp, vertical = 8.dp)
                        )
                        
                        DropdownMenuItem(
                            text = { Text("All Events") },
                            onClick = {
                                selectedEventType = null
                                viewModel.searchNotifications(searchQuery, null, fromDate, toDate)
                            }
                        )
                        
                        eventTypes.forEach { eventType ->
                            DropdownMenuItem(
                                text = { Text(eventType) },
                                onClick = {
                                    selectedEventType = eventType
                                    viewModel.searchNotifications(searchQuery, eventType, fromDate, toDate)
                                }
                            )
                        }
                        
                        Text(
                            text = "Date Range",
                            modifier = Modifier.padding(horizontal = 16.dp, vertical = 8.dp)
                        )
                        
                        Row(
                            modifier = Modifier
                                .fillMaxWidth()
                                .padding(horizontal = 16.dp, vertical = 8.dp),
                            horizontalArrangement = Arrangement.SpaceBetween
                        ) {
                            Button(
                                onClick = { fromDatePicker.show() },
                                modifier = Modifier.weight(1f)
                            ) {
                                Text(fromDate?.let { dateFormat.format(it) } ?: "From Date")
                            }
                            
                            Spacer(modifier = Modifier.width(8.dp))
                            
                            Button(
                                onClick = { toDatePicker.show() },
                                modifier = Modifier.weight(1f)
                            ) {
                                Text(toDate?.let { dateFormat.format(it) } ?: "To Date")
                            }
                        }
                        
                        Button(
                            onClick = {
                                fromDate = null
                                toDate = null
                                viewModel.searchNotifications(searchQuery, selectedEventType, null, null)
                            },
                            modifier = Modifier
                                .fillMaxWidth()
                                .padding(horizontal = 16.dp, vertical = 8.dp)
                        ) {
                            Text("Clear Filters")
                        }
                    }
                    
                    // Export dropdown menu
                    DropdownMenu(
                        expanded = showExportOptions,
                        onDismissRequest = { showExportOptions = false }
                    ) {
                        DropdownMenuItem(
                            text = { Text(stringResource(R.string.export_csv)) },
                            onClick = {
                                showExportOptions = false
                                csvExportLauncher.launch("webhook_notifications_${System.currentTimeMillis()}.csv")
                            }
                        )
                        
                        DropdownMenuItem(
                            text = { Text(stringResource(R.string.export_json)) },
                            onClick = {
                                showExportOptions = false
                                jsonExportLauncher.launch("webhook_notifications_${System.currentTimeMillis()}.json")
                            }
                        )
                    }
                }
            )
        }
    ) { paddingValues ->
        Box(
            modifier = Modifier
                .fillMaxSize()
                .padding(paddingValues)
        ) {
            if (isLoading) {
                CircularProgressIndicator(
                    modifier = Modifier.align(Alignment.Center)
                )
            } else if (notifications.isEmpty()) {
                Text(
                    text = stringResource(R.string.no_notifications),
                    modifier = Modifier.align(Alignment.Center)
                )
            } else {
                LazyColumn(
                    modifier = Modifier.fillMaxSize()
                ) {
                    items(notifications) { notification ->
                        NotificationItem(
                            notification = notification,
                            dateFormat = dateFormat,
                            timeFormat = timeFormat,
                            onClick = {
                                selectedNotification = notification
                                showNotificationDetails = true
                            }
                        )
                    }
                }
            }
        }
    }
    
    // Notification details dialog
    if (showNotificationDetails && selectedNotification != null) {
        AlertDialog(
            onDismissRequest = { showNotificationDetails = false },
            title = { Text(stringResource(R.string.notification_details)) },
            text = {
                Column {
                    Text("Event: ${selectedNotification?.event}")
                    Spacer(modifier = Modifier.height(8.dp))
                    
                    Text("Time: ${selectedNotification?.timestamp?.let { 
                        "${dateFormat.format(it)} ${timeFormat.format(it)}" 
                    }}")
                    Spacer(modifier = Modifier.height(8.dp))
                    
                    Text("Message:")
                    Text(selectedNotification?.message ?: "No message")
                    Spacer(modifier = Modifier.height(8.dp))
                    
                    if (!selectedNotification?.additionalData.isNullOrEmpty()) {
                        Text("Additional Data:")
                        Text(selectedNotification?.additionalData ?: "")
                    }
                }
            },
            confirmButton = {
                Button(onClick = { showNotificationDetails = false }) {
                    Text("Close")
                }
            }
        )
    }
    
    // Clear history confirmation dialog
    if (showClearConfirmation) {
        AlertDialog(
            onDismissRequest = { showClearConfirmation = false },
            title = { Text(stringResource(R.string.clear_history)) },
            text = { Text(stringResource(R.string.clear_history_confirm)) },
            confirmButton = {
                Button(
                    onClick = {
                        viewModel.clearAllNotifications()
                        showClearConfirmation = false
                    }
                ) {
                    Text(stringResource(R.string.yes))
                }
            },
            dismissButton = {
                TextButton(
                    onClick = { showClearConfirmation = false }
                ) {
                    Text(stringResource(R.string.no))
                }
            }
        )
    }
}

@Composable
fun NotificationItem(
    notification: NotificationHistory,
    dateFormat: SimpleDateFormat,
    timeFormat: SimpleDateFormat,
    onClick: () -> Unit
) {
    Card(
        modifier = Modifier
            .fillMaxWidth()
            .padding(horizontal = 16.dp, vertical = 8.dp)
            .clickable(onClick = onClick)
    ) {
        Column(
            modifier = Modifier.padding(16.dp)
        ) {
            Row(
                modifier = Modifier.fillMaxWidth(),
                horizontalArrangement = Arrangement.SpaceBetween
            ) {
                Text(
                    text = notification.event,
                    style = MaterialTheme.typography.titleMedium,
                    fontWeight = FontWeight.Bold
                )
                
                Text(
                    text = notification.timestamp.let { 
                        "${dateFormat.format(it)} ${timeFormat.format(it)}" 
                    },
                    style = MaterialTheme.typography.bodySmall
                )
            }
            
            Spacer(modifier = Modifier.height(8.dp))
            
            Text(
                text = notification.message ?: "No message",
                maxLines = 2,
                overflow = TextOverflow.Ellipsis
            )
            
            if (!notification.additionalData.isNullOrEmpty()) {
                Spacer(modifier = Modifier.height(4.dp))
                Text(
                    text = "Additional data available",
                    style = MaterialTheme.typography.bodySmall,
                    color = MaterialTheme.colorScheme.primary
                )
            }
        }
    }
}
