using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace ApiKeyGenerator;

public class Program
{
    private const int API_KEY_LENGTH = 32; // 256 bits
    private const string CONFIG_FILE_PATH = "../WebHookReceiverApi/appsettings.json";
    private const string API_KEY_FILE = "apikey.txt";

    public static void Main(string[] args)
    {
        Console.WriteLine("=== WebHook API Key Generator ===");
        Console.WriteLine("This tool will generate a new API key for WebHook Receiver API.");
        Console.WriteLine();

        // Generate new API key
        string apiKey = GenerateApiKey();
        Console.WriteLine($"Generated new API key: {apiKey}");

        // Save API key to file
        SaveApiKeyToFile(apiKey);

        // Update API configuration file
        if (UpdateApiConfig(apiKey))
        {
            Console.WriteLine($"API configuration file has been updated.");
        }
        else
        {
            Console.WriteLine($"API configuration file not found at path: {CONFIG_FILE_PATH}");
            Console.WriteLine($"Please add the following line to the 'AppSettings' section in the API configuration file:");
            Console.WriteLine($"\"ApiKey\": \"{apiKey}\"");
        }

        Console.WriteLine();
        Console.WriteLine("To use this API key in webhook requests, add the following header:");
        Console.WriteLine($"X-API-Key: {apiKey}");
        Console.WriteLine();
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }

    private static string GenerateApiKey()
    {
        byte[] randomBytes = new byte[API_KEY_LENGTH];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }

        return Convert.ToBase64String(randomBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "")
            .Substring(0, API_KEY_LENGTH);
    }

    private static void SaveApiKeyToFile(string apiKey)
    {
        try
        {
            File.WriteAllText(API_KEY_FILE, apiKey);
            Console.WriteLine($"API key has been saved to file: {Path.GetFullPath(API_KEY_FILE)}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving API key to file: {ex.Message}");
        }
    }

    private static bool UpdateApiConfig(string apiKey)
    {
        if (!File.Exists(CONFIG_FILE_PATH))
        {
            return false;
        }

        try
        {
            string configContent = File.ReadAllText(CONFIG_FILE_PATH);

            // Simple replacement - in a real project it would be better to use a JSON parser
            if (configContent.Contains("\"ApiKey\":"))
            {
                // Replace existing key
                configContent = System.Text.RegularExpressions.Regex.Replace(
                    configContent,
                    "\"ApiKey\":\\s*\"[^\"]*\"",
                    $"\"ApiKey\": \"{apiKey}\"");
            }
            else
            {
                // Add new key to AppSettings section
                int appSettingsIndex = configContent.IndexOf("\"AppSettings\": {");
                if (appSettingsIndex >= 0)
                {
                    int openBraceIndex = configContent.IndexOf('{', appSettingsIndex);
                    if (openBraceIndex >= 0)
                    {
                        configContent = configContent.Insert(openBraceIndex + 1, $"\r\n    \"ApiKey\": \"{apiKey}\",");
                    }
                }
                else
                {
                    // Add new AppSettings section
                    int rootOpenBraceIndex = configContent.IndexOf('{');
                    if (rootOpenBraceIndex >= 0)
                    {
                        configContent = configContent.Insert(rootOpenBraceIndex + 1, $"\r\n  \"AppSettings\": {{\r\n    \"ApiKey\": \"{apiKey}\"\r\n  }},");
                    }
                }
            }

            File.WriteAllText(CONFIG_FILE_PATH, configContent);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating configuration file: {ex.Message}");
            return false;
        }
    }
}
