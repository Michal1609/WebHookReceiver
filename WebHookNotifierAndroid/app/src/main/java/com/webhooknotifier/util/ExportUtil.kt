package com.webhooknotifier.util

import android.content.Context
import android.net.Uri
import com.google.gson.GsonBuilder
import com.opencsv.CSVWriter
import com.webhooknotifier.data.model.NotificationHistory
import java.io.FileOutputStream
import java.io.OutputStreamWriter
import java.text.SimpleDateFormat
import java.util.Locale
import javax.inject.Inject
import javax.inject.Singleton

@Singleton
class ExportUtil @Inject constructor() {
    
    private val dateFormat = SimpleDateFormat("yyyy-MM-dd HH:mm:ss", Locale.getDefault())
    private val gson = GsonBuilder().setPrettyPrinting().create()
    
    fun exportToCsv(context: Context, uri: Uri, notifications: List<NotificationHistory>): Boolean {
        return try {
            context.contentResolver.openFileDescriptor(uri, "w")?.use { parcelFileDescriptor ->
                FileOutputStream(parcelFileDescriptor.fileDescriptor).use { fileOutputStream ->
                    OutputStreamWriter(fileOutputStream).use { outputStreamWriter ->
                        CSVWriter(outputStreamWriter).use { csvWriter ->
                            // Write header
                            csvWriter.writeNext(arrayOf("ID", "Event", "Message", "Timestamp", "AdditionalData"))
                            
                            // Write data rows
                            notifications.forEach { notification ->
                                csvWriter.writeNext(arrayOf(
                                    notification.id.toString(),
                                    notification.event,
                                    notification.message ?: "",
                                    dateFormat.format(notification.timestamp),
                                    notification.additionalData ?: ""
                                ))
                            }
                        }
                    }
                }
            }
            true
        } catch (e: Exception) {
            e.printStackTrace()
            false
        }
    }
    
    fun exportToJson(context: Context, uri: Uri, notifications: List<NotificationHistory>): Boolean {
        return try {
            context.contentResolver.openFileDescriptor(uri, "w")?.use { parcelFileDescriptor ->
                FileOutputStream(parcelFileDescriptor.fileDescriptor).use { fileOutputStream ->
                    OutputStreamWriter(fileOutputStream).use { outputStreamWriter ->
                        val json = gson.toJson(notifications)
                        outputStreamWriter.write(json)
                    }
                }
            }
            true
        } catch (e: Exception) {
            e.printStackTrace()
            false
        }
    }
}
