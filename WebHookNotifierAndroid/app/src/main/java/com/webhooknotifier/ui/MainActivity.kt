package com.webhooknotifier.ui

import android.content.ComponentName
import android.content.Context
import android.content.Intent
import android.content.ServiceConnection
import android.os.Bundle
import android.os.IBinder
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Surface
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.ui.Modifier
import androidx.hilt.navigation.compose.hiltViewModel
import androidx.navigation.compose.NavHost
import androidx.navigation.compose.composable
import androidx.navigation.compose.rememberNavController
import com.webhooknotifier.service.NotificationService
import com.webhooknotifier.ui.screens.HistoryScreen
import com.webhooknotifier.ui.screens.HomeScreen
import com.webhooknotifier.ui.screens.SettingsScreen
import com.webhooknotifier.ui.theme.WebHookNotifierTheme
import dagger.hilt.android.AndroidEntryPoint

@AndroidEntryPoint
class MainActivity : ComponentActivity() {
    
    private var notificationService: NotificationService? = null
    private var bound = false
    
    private val connection = object : ServiceConnection {
        override fun onServiceConnected(name: ComponentName?, service: IBinder?) {
            val binder = service as NotificationService.LocalBinder
            notificationService = binder.getService()
            bound = true
        }
        
        override fun onServiceDisconnected(name: ComponentName?) {
            bound = false
        }
    }
    
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        
        // Start the notification service
        val serviceIntent = Intent(this, NotificationService::class.java)
        startForegroundService(serviceIntent)
        bindService(serviceIntent, connection, Context.BIND_AUTO_CREATE)
        
        setContent {
            WebHookNotifierTheme {
                Surface(
                    modifier = Modifier.fillMaxSize(),
                    color = MaterialTheme.colorScheme.background
                ) {
                    val navController = rememberNavController()
                    
                    NavHost(navController = navController, startDestination = "home") {
                        composable("home") {
                            val viewModel = hiltViewModel<MainViewModel>()
                            val serviceState by viewModel.serviceState.collectAsState()
                            
                            HomeScreen(
                                serviceState = serviceState,
                                onNavigateToSettings = { navController.navigate("settings") },
                                onNavigateToHistory = { navController.navigate("history") }
                            )
                        }
                        
                        composable("settings") {
                            SettingsScreen(
                                onNavigateBack = { navController.popBackStack() }
                            )
                        }
                        
                        composable("history") {
                            HistoryScreen(
                                onNavigateBack = { navController.popBackStack() }
                            )
                        }
                    }
                }
            }
        }
    }
    
    override fun onDestroy() {
        if (bound) {
            unbindService(connection)
            bound = false
        }
        super.onDestroy()
    }
}
