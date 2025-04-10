using System;
using System.Security.Cryptography;
using System.Text;

namespace WebHookNotifier.Security
{
    /// <summary>
    /// Provides methods to securely encrypt and decrypt connection strings
    /// </summary>
    public static class ConnectionStringProtection
    {
        /// <summary>
        /// Encrypts a connection string using Windows Data Protection API
        /// </summary>
        public static string EncryptConnectionString(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                return string.Empty;
                
            try
            {
                // Convert the string to a byte array
                byte[] dataToEncrypt = Encoding.UTF8.GetBytes(connectionString);
                
                // Encrypt the data using the Windows Data Protection API
                byte[] encryptedData = ProtectedData.Protect(
                    dataToEncrypt, 
                    null, 
                    DataProtectionScope.CurrentUser);
                
                // Convert the encrypted data to a Base64 string for storage
                return Convert.ToBase64String(encryptedData);
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
                // Convert the Base64 string back to a byte array
                byte[] encryptedData = Convert.FromBase64String(encryptedConnectionString);
                
                // Decrypt the data using the Windows Data Protection API
                byte[] decryptedData = ProtectedData.Unprotect(
                    encryptedData, 
                    null, 
                    DataProtectionScope.CurrentUser);
                
                // Convert the decrypted byte array back to a string
                return Encoding.UTF8.GetString(decryptedData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error decrypting connection string: {ex.Message}");
                return string.Empty;
            }
        }
    }
}
