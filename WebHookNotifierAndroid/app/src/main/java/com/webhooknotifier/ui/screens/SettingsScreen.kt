package com.webhooknotifier.ui.screens

import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.verticalScroll
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.ArrowBack
import androidx.compose.material3.Button
import androidx.compose.material3.Card
import androidx.compose.material3.Checkbox
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedTextField
import androidx.compose.material3.RadioButton
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.material3.TopAppBar
import androidx.compose.runtime.Composable
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.unit.dp
import androidx.hilt.navigation.compose.hiltViewModel
import com.webhooknotifier.R
import com.webhooknotifier.data.model.DatabaseType
import com.webhooknotifier.data.model.NotificationSettings
import com.webhooknotifier.ui.MainViewModel

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun SettingsScreen(
    onNavigateBack: () -> Unit,
    viewModel: MainViewModel = hiltViewModel()
) {
    val settings by viewModel.settings.collectAsState()
    
    var minSecondsBetween by remember { mutableStateOf(settings.minSecondsBetweenNotifications.toString()) }
    var maxQueued by remember { mutableStateOf(settings.maxQueuedNotifications.toString()) }
    var enableSounds by remember { mutableStateOf(settings.enableNotificationSounds) }
    var enableEncryption by remember { mutableStateOf(settings.enableEncryption) }
    var enableHistory by remember { mutableStateOf(settings.enableHistoryTracking) }
    var historyRetention by remember { mutableStateOf(settings.historyRetentionDays.toString()) }
    var databaseType by remember { mutableStateOf(settings.databaseType) }
    var sqlServerConnectionString by remember { mutableStateOf(settings.sqlServerConnectionString) }
    
    Scaffold(
        topBar = {
            TopAppBar(
                title = { Text(stringResource(R.string.settings)) },
                navigationIcon = {
                    IconButton(onClick = onNavigateBack) {
                        Icon(Icons.Default.ArrowBack, contentDescription = "Back")
                    }
                }
            )
        }
    ) { paddingValues ->
        Column(
            modifier = Modifier
                .fillMaxSize()
                .padding(paddingValues)
                .padding(16.dp)
                .verticalScroll(rememberScrollState())
        ) {
            // Rate Limiting Section
            Card(
                modifier = Modifier
                    .fillMaxWidth()
                    .padding(bottom = 16.dp)
            ) {
                Column(
                    modifier = Modifier.padding(16.dp)
                ) {
                    Text(
                        text = stringResource(R.string.rate_limiting),
                        style = MaterialTheme.typography.titleMedium
                    )
                    
                    Spacer(modifier = Modifier.height(8.dp))
                    
                    OutlinedTextField(
                        value = minSecondsBetween,
                        onValueChange = { minSecondsBetween = it },
                        label = { Text(stringResource(R.string.min_seconds_between)) },
                        modifier = Modifier.fillMaxWidth()
                    )
                    
                    Spacer(modifier = Modifier.height(8.dp))
                    
                    OutlinedTextField(
                        value = maxQueued,
                        onValueChange = { maxQueued = it },
                        label = { Text(stringResource(R.string.max_queued)) },
                        modifier = Modifier.fillMaxWidth()
                    )
                    
                    Spacer(modifier = Modifier.height(8.dp))
                    
                    Row(
                        verticalAlignment = Alignment.CenterVertically
                    ) {
                        Checkbox(
                            checked = enableSounds,
                            onCheckedChange = { enableSounds = it }
                        )
                        Text(stringResource(R.string.enable_sounds))
                    }
                }
            }
            
            // Security Section
            Card(
                modifier = Modifier
                    .fillMaxWidth()
                    .padding(bottom = 16.dp)
            ) {
                Column(
                    modifier = Modifier.padding(16.dp)
                ) {
                    Text(
                        text = "Security",
                        style = MaterialTheme.typography.titleMedium
                    )
                    
                    Spacer(modifier = Modifier.height(8.dp))
                    
                    Row(
                        verticalAlignment = Alignment.CenterVertically
                    ) {
                        Checkbox(
                            checked = enableEncryption,
                            onCheckedChange = { enableEncryption = it }
                        )
                        Text(stringResource(R.string.enable_encryption))
                    }
                }
            }
            
            // History Section
            Card(
                modifier = Modifier
                    .fillMaxWidth()
                    .padding(bottom = 16.dp)
            ) {
                Column(
                    modifier = Modifier.padding(16.dp)
                ) {
                    Text(
                        text = stringResource(R.string.history),
                        style = MaterialTheme.typography.titleMedium
                    )
                    
                    Spacer(modifier = Modifier.height(8.dp))
                    
                    Row(
                        verticalAlignment = Alignment.CenterVertically
                    ) {
                        Checkbox(
                            checked = enableHistory,
                            onCheckedChange = { enableHistory = it }
                        )
                        Text(stringResource(R.string.enable_history))
                    }
                    
                    Spacer(modifier = Modifier.height(8.dp))
                    
                    OutlinedTextField(
                        value = historyRetention,
                        onValueChange = { historyRetention = it },
                        label = { Text(stringResource(R.string.history_retention)) },
                        modifier = Modifier.fillMaxWidth()
                    )
                    
                    Spacer(modifier = Modifier.height(16.dp))
                    
                    Text(
                        text = stringResource(R.string.database_type),
                        style = MaterialTheme.typography.bodyMedium
                    )
                    
                    Row(
                        verticalAlignment = Alignment.CenterVertically
                    ) {
                        RadioButton(
                            selected = databaseType == DatabaseType.SQLITE,
                            onClick = { databaseType = DatabaseType.SQLITE }
                        )
                        Text(stringResource(R.string.sqlite))
                    }
                    
                    Row(
                        verticalAlignment = Alignment.CenterVertically
                    ) {
                        RadioButton(
                            selected = databaseType == DatabaseType.SQL_SERVER,
                            onClick = { databaseType = DatabaseType.SQL_SERVER }
                        )
                        Text(stringResource(R.string.sql_server))
                    }
                    
                    if (databaseType == DatabaseType.SQL_SERVER) {
                        Spacer(modifier = Modifier.height(8.dp))
                        
                        OutlinedTextField(
                            value = sqlServerConnectionString,
                            onValueChange = { sqlServerConnectionString = it },
                            label = { Text(stringResource(R.string.connection_string)) },
                            modifier = Modifier.fillMaxWidth()
                        )
                        
                        Spacer(modifier = Modifier.height(8.dp))
                        
                        Button(
                            onClick = { /* Test connection */ },
                            modifier = Modifier.align(Alignment.End)
                        ) {
                            Text(stringResource(R.string.test_connection))
                        }
                    }
                }
            }
            
            Button(
                onClick = {
                    val updatedSettings = NotificationSettings(
                        serverUrl = settings.serverUrl,
                        minSecondsBetweenNotifications = minSecondsBetween.toIntOrNull() ?: 2,
                        maxQueuedNotifications = maxQueued.toIntOrNull() ?: 5,
                        enableNotificationSounds = enableSounds,
                        enableEncryption = enableEncryption,
                        enableHistoryTracking = enableHistory,
                        historyRetentionDays = historyRetention.toIntOrNull() ?: 30,
                        databaseType = databaseType,
                        sqlServerConnectionString = sqlServerConnectionString
                    )
                    viewModel.updateSettings(updatedSettings)
                    onNavigateBack()
                },
                modifier = Modifier.fillMaxWidth()
            ) {
                Text(stringResource(R.string.save))
            }
        }
    }
}
