package com.webhooknotifier.util

import android.util.Base64
import java.nio.charset.StandardCharsets
import javax.crypto.Cipher
import javax.crypto.spec.IvParameterSpec
import javax.crypto.spec.SecretKeySpec
import javax.inject.Inject
import javax.inject.Singleton

@Singleton
class EncryptionUtil @Inject constructor() {
    
    // Same encryption key as in the C# application
    private val encryptionKey = "YourEncryptionKey123456789012345678"
    
    fun encrypt(plainText: String): String {
        try {
            val key = SecretKeySpec(encryptionKey.toByteArray(StandardCharsets.UTF_8), "AES")
            val iv = ByteArray(16)
            val ivSpec = IvParameterSpec(iv)
            
            val cipher = Cipher.getInstance("AES/CBC/PKCS5Padding")
            cipher.init(Cipher.ENCRYPT_MODE, key, ivSpec)
            
            val encrypted = cipher.doFinal(plainText.toByteArray(StandardCharsets.UTF_8))
            return Base64.encodeToString(encrypted, Base64.DEFAULT)
        } catch (e: Exception) {
            throw RuntimeException("Encryption failed", e)
        }
    }
    
    fun decrypt(encryptedText: String): String {
        try {
            val key = SecretKeySpec(encryptionKey.toByteArray(StandardCharsets.UTF_8), "AES")
            val iv = ByteArray(16)
            val ivSpec = IvParameterSpec(iv)
            
            val cipher = Cipher.getInstance("AES/CBC/PKCS5Padding")
            cipher.init(Cipher.DECRYPT_MODE, key, ivSpec)
            
            val encrypted = Base64.decode(encryptedText, Base64.DEFAULT)
            val decrypted = cipher.doFinal(encrypted)
            return String(decrypted, StandardCharsets.UTF_8)
        } catch (e: Exception) {
            throw RuntimeException("Decryption failed", e)
        }
    }
}
