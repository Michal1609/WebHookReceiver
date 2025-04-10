package com.webhooknotifier.ui.screens

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.History
import androidx.compose.material.icons.filled.Notifications
import androidx.compose.material.icons.filled.Settings
import androidx.compose.material3.Button
import androidx.compose.material3.Card
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.Icon
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedTextField
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
import com.webhooknotifier.service.NotificationService
import com.webhooknotifier.service.SignalRService
import com.webhooknotifier.ui.MainViewModel

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun HomeScreen(
    serviceState: NotificationService.ServiceState,
    onNavigateToSettings: () -> Unit,
    onNavigateToHistory: () -> Unit,
    viewModel: MainViewModel = hiltViewModel()
) {
    val settings by viewModel.settings.collectAsState()
    val connectionState by viewModel.connectionState.collectAsState()
    
    var serverUrl by remember { mutableStateOf(settings.serverUrl) }
    
    Scaffold(
        topBar = {
            TopAppBar(
                title = { Text(text = stringResource(R.string.app_name)) }
            )
        }
    ) { paddingValues ->
        Column(
            modifier = Modifier
                .fillMaxSize()
                .padding(paddingValues)
                .padding(16.dp),
            horizontalAlignment = Alignment.CenterHorizontally
        ) {
            Card(
                modifier = Modifier
                    .fillMaxWidth()
                    .padding(bottom = 16.dp)
            ) {
                Column(
                    modifier = Modifier.padding(16.dp)
                ) {
                    Text(
                        text = stringResource(R.string.status),
                        style = MaterialTheme.typography.titleMedium
                    )
                    
                    Spacer(modifier = Modifier.height(8.dp))
                    
                    val statusText = when (connectionState) {
                        SignalRService.ConnectionState.CONNECTED -> stringResource(R.string.connected)
                        SignalRService.ConnectionState.CONNECTING -> "Connecting..."
                        SignalRService.ConnectionState.ERROR -> "Error"
                        else -> stringResource(R.string.disconnected)
                    }
                    
                    Text(
                        text = statusText,
                        color = when (connectionState) {
                            SignalRService.ConnectionState.CONNECTED -> MaterialTheme.colorScheme.primary
                            SignalRService.ConnectionState.ERROR -> MaterialTheme.colorScheme.error
                            else -> MaterialTheme.colorScheme.onSurface
                        }
                    )
                }
            }
            
            OutlinedTextField(
                value = serverUrl,
                onValueChange = { serverUrl = it },
                label = { Text(stringResource(R.string.server_url)) },
                placeholder = { Text(stringResource(R.string.server_url_hint)) },
                modifier = Modifier.fillMaxWidth()
            )
            
            Spacer(modifier = Modifier.height(16.dp))
            
            Row(
                modifier = Modifier.fillMaxWidth(),
                horizontalArrangement = Arrangement.SpaceBetween
            ) {
                Button(
                    onClick = {
                        if (connectionState == SignalRService.ConnectionState.CONNECTED) {
                            viewModel.disconnect()
                        } else {
                            val updatedSettings = settings.copy(serverUrl = serverUrl)
                            viewModel.updateSettings(updatedSettings)
                            viewModel.connect()
                        }
                    },
                    modifier = Modifier.weight(1f)
                ) {
                    Text(
                        text = if (connectionState == SignalRService.ConnectionState.CONNECTED) {
                            stringResource(R.string.disconnect)
                        } else {
                            stringResource(R.string.connect)
                        }
                    )
                }
            }
            
            Spacer(modifier = Modifier.height(32.dp))
            
            Row(
                modifier = Modifier.fillMaxWidth(),
                horizontalArrangement = Arrangement.SpaceEvenly
            ) {
                Button(
                    onClick = onNavigateToSettings
                ) {
                    Icon(
                        imageVector = Icons.Default.Settings,
                        contentDescription = null
                    )
                    Spacer(modifier = Modifier.padding(4.dp))
                    Text(stringResource(R.string.settings))
                }
                
                Button(
                    onClick = onNavigateToHistory
                ) {
                    Icon(
                        imageVector = Icons.Default.History,
                        contentDescription = null
                    )
                    Spacer(modifier = Modifier.padding(4.dp))
                    Text(stringResource(R.string.history))
                }
            }
        }
    }
}
