using System;
using System.Security.Cryptography;
using System.Text;

namespace WebHookNotifierMaui.Security
{
    /// <summary>
    /// Provides methods to securely encrypt and decrypt connection strings
    /// </summary>
    public static class ConnectionStringProtection
    {
        /// <summary>
        /// Encrypts a connection string using platform-specific protection
        /// </summary>
        public static string EncryptConnectionString(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                return string.Empty;
                
            try
            {
                // For MAUI, we'll use our own encryption since ProtectedData
                // is not available on all platforms in the same way
                return EncryptionService.Encrypt(connectionString);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error encrypting connection string: {ex.Message}");
                return string.Empty;
            }
        }
        
        /// <summary>
        /// Decrypts a connection string that was encrypted using EncryptConnectionString
        /// </summary>
        public static string DecryptConnectionString(string encryptedConnectionString)
        {
            if (string.IsNullOrEmpty(encryptedConnectionString))
                return string.Empty;
                
            try
            {
                // For MAUI, we'll use our own decryption since ProtectedData
                // is not available on all platforms in the same way
                return EncryptionService.Decrypt(encryptedConnectionString);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error decrypting connection string: {ex.Message}");
                return string.Empty;
            }
        }
    }
}
