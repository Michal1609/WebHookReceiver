package com.webhooknotifier.util

import com.google.gson.GsonBuilder
import com.webhooknotifier.data.model.WebhookData
import javax.inject.Inject
import javax.inject.Singleton

@Singleton
class NotificationFormatter @Inject constructor() {
    
    private val gson = GsonBuilder().setPrettyPrinting().create()
    
    fun formatMessage(webhookData: WebhookData): String {
        val message = webhookData.message ?: "No message"
        val builder = StringBuilder(message)
        
        if (!webhookData.additionalData.isNullOrEmpty()) {
            builder.append("\n\nAdditional data:")
            
            // Format JSON data for better readability
            val jsonString = gson.toJson(webhookData.additionalData)
            builder.append("\n").append(jsonString)
        }
        
        return formatLongText(builder.toString())
    }
    
    private fun formatLongText(text: String): String {
        // Break long lines for better readability in notifications
        if (text.length <= 40) return text
        
        val result = StringBuilder()
        var lineLength = 0
        
        for (i in text.indices) {
            val c = text[i]
            result.append(c)
            lineLength++
            
            // If we've reached a good breaking point and the line is getting long
            if (lineLength > 40 && (c == ' ' || c == '.' || c == ',' || c == ';' || c == ':' || c == '\n')) {
                result.append('\n')
                lineLength = 0
            }
        }
        
        return result.toString()
    }
}
