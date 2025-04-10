package com.webhooknotifier.data.repository

import android.content.Context
import androidx.datastore.core.DataStore
import androidx.datastore.preferences.core.Preferences
import androidx.datastore.preferences.core.booleanPreferencesKey
import androidx.datastore.preferences.core.edit
import androidx.datastore.preferences.core.intPreferencesKey
import androidx.datastore.preferences.core.stringPreferencesKey
import androidx.datastore.preferences.preferencesDataStore
import androidx.security.crypto.EncryptedSharedPreferences
import androidx.security.crypto.MasterKey
import com.webhooknotifier.data.model.DatabaseType
import com.webhooknotifier.data.model.NotificationSettings
import dagger.hilt.android.qualifiers.ApplicationContext
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.map
import javax.inject.Inject
import javax.inject.Singleton

private val Context.dataStore: DataStore<Preferences> by preferencesDataStore(name = "settings")

@Singleton
class SettingsRepository @Inject constructor(
    @ApplicationContext private val context: Context
) {
    private val dataStore = context.dataStore
    
    // Preference keys
    private object PreferenceKeys {
        val SERVER_URL = stringPreferencesKey("server_url")
        val MIN_SECONDS_BETWEEN = intPreferencesKey("min_seconds_between")
        val MAX_QUEUED = intPreferencesKey("max_queued")
        val ENABLE_SOUNDS = booleanPreferencesKey("enable_sounds")
        val ENABLE_ENCRYPTION = booleanPreferencesKey("enable_encryption")
        val ENABLE_HISTORY = booleanPreferencesKey("enable_history")
        val HISTORY_RETENTION = intPreferencesKey("history_retention")
        val DATABASE_TYPE = stringPreferencesKey("database_type")
    }
    
    // Get settings as a flow
    val settingsFlow: Flow<NotificationSettings> = dataStore.data.map { preferences ->
        NotificationSettings(
            serverUrl = preferences[PreferenceKeys.SERVER_URL] ?: "",
            minSecondsBetweenNotifications = preferences[PreferenceKeys.MIN_SECONDS_BETWEEN] ?: 2,
            maxQueuedNotifications = preferences[PreferenceKeys.MAX_QUEUED] ?: 5,
            enableNotificationSounds = preferences[PreferenceKeys.ENABLE_SOUNDS] ?: true,
            enableEncryption = preferences[PreferenceKeys.ENABLE_ENCRYPTION] ?: true,
            enableHistoryTracking = preferences[PreferenceKeys.ENABLE_HISTORY] ?: true,
            historyRetentionDays = preferences[PreferenceKeys.HISTORY_RETENTION] ?: 30,
            databaseType = when (preferences[PreferenceKeys.DATABASE_TYPE]) {
                "SQL_SERVER" -> DatabaseType.SQL_SERVER
                else -> DatabaseType.SQLITE
            },
            sqlServerConnectionString = getEncryptedConnectionString()
        )
    }
    
    // Update settings
    suspend fun updateSettings(settings: NotificationSettings) {
        dataStore.edit { preferences ->
            preferences[PreferenceKeys.SERVER_URL] = settings.serverUrl
            preferences[PreferenceKeys.MIN_SECONDS_BETWEEN] = settings.minSecondsBetweenNotifications
            preferences[PreferenceKeys.MAX_QUEUED] = settings.maxQueuedNotifications
            preferences[PreferenceKeys.ENABLE_SOUNDS] = settings.enableNotificationSounds
            preferences[PreferenceKeys.ENABLE_ENCRYPTION] = settings.enableEncryption
            preferences[PreferenceKeys.ENABLE_HISTORY] = settings.enableHistoryTracking
            preferences[PreferenceKeys.HISTORY_RETENTION] = settings.historyRetentionDays
            preferences[PreferenceKeys.DATABASE_TYPE] = settings.databaseType.name
        }
        
        // Save connection string in encrypted shared preferences
        saveEncryptedConnectionString(settings.sqlServerConnectionString)
    }
    
    // Get encrypted connection string
    private fun getEncryptedConnectionString(): String {
        return try {
            val masterKey = MasterKey.Builder(context)
                .setKeyScheme(MasterKey.KeyScheme.AES256_GCM)
                .build()
                
            val sharedPreferences = EncryptedSharedPreferences.create(
                context,
                "encrypted_settings",
                masterKey,
                EncryptedSharedPreferences.PrefKeyEncryptionScheme.AES256_SIV,
                EncryptedSharedPreferences.PrefValueEncryptionScheme.AES256_GCM
            )
            
            sharedPreferences.getString("sql_connection_string", "") ?: ""
        } catch (e: Exception) {
            ""
        }
    }
    
    // Save encrypted connection string
    private fun saveEncryptedConnectionString(connectionString: String) {
        try {
            val masterKey = MasterKey.Builder(context)
                .setKeyScheme(MasterKey.KeyScheme.AES256_GCM)
                .build()
                
            val sharedPreferences = EncryptedSharedPreferences.create(
                context,
                "encrypted_settings",
                masterKey,
                EncryptedSharedPreferences.PrefKeyEncryptionScheme.AES256_SIV,
                EncryptedSharedPreferences.PrefValueEncryptionScheme.AES256_GCM
            )
            
            sharedPreferences.edit().putString("sql_connection_string", connectionString).apply()
        } catch (e: Exception) {
            // Handle encryption error
        }
    }
}
