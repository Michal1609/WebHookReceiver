package com.webhooknotifier.data.model

import java.util.Date

data class WebhookData(
    val event: String,
    val message: String? = null,
    val timestamp: Date = Date(),
    val additionalData: Map<String, Any>? = null
)
