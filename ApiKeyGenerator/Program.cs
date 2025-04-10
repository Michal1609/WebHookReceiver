using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace ApiKeyGenerator;

public class Program
{
    private const int API_KEY_LENGTH = 32; // 256 bitů
    private const string CONFIG_FILE_PATH = "../WebHookReceiverApi/appsettings.json";
    private const string API_KEY_FILE = "apikey.txt";

    public static void Main(string[] args)
    {
        Console.WriteLine("=== WebHook API Key Generator ===");
        Console.WriteLine("Tento nástroj vygeneruje nový API klíč pro WebHook Receiver API.");
        Console.WriteLine();

        // Generování nového API klíče
        string apiKey = GenerateApiKey();
        Console.WriteLine($"Vygenerován nový API klíč: {apiKey}");

        // Uložení API klíče do souboru
        SaveApiKeyToFile(apiKey);

        // Aktualizace konfiguračního souboru API
        if (UpdateApiConfig(apiKey))
        {
            Console.WriteLine($"Konfigurační soubor API byl aktualizován.");
        }
        else
        {
            Console.WriteLine($"Konfigurační soubor API nebyl nalezen na cestě: {CONFIG_FILE_PATH}");
            Console.WriteLine($"Prosím, přidejte následující řádek do sekce 'AppSettings' v konfiguračním souboru API:");
            Console.WriteLine($"\"ApiKey\": \"{apiKey}\"");
        }

        Console.WriteLine();
        Console.WriteLine("Pro použití tohoto API klíče v požadavcích na webhook přidejte následující hlavičku:");
        Console.WriteLine($"X-API-Key: {apiKey}");
        Console.WriteLine();
        Console.WriteLine("Stiskněte libovolnou klávesu pro ukončení...");
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
            Console.WriteLine($"API klíč byl uložen do souboru: {Path.GetFullPath(API_KEY_FILE)}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Chyba při ukládání API klíče do souboru: {ex.Message}");
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

            // Jednoduchá náhrada - v reálném projektu by bylo lepší použít JSON parser
            if (configContent.Contains("\"ApiKey\":"))
            {
                // Nahrazení existujícího klíče
                configContent = System.Text.RegularExpressions.Regex.Replace(
                    configContent,
                    "\"ApiKey\":\\s*\"[^\"]*\"",
                    $"\"ApiKey\": \"{apiKey}\"");
            }
            else
            {
                // Přidání nového klíče do sekce AppSettings
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
                    // Přidání nové sekce AppSettings
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
            Console.WriteLine($"Chyba při aktualizaci konfiguračního souboru: {ex.Message}");
            return false;
        }
    }
}
