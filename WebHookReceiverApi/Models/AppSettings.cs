namespace WebHookReceiverApi.Models
{
    public class AppSettings
    {
        public string ApiKey { get; set; } = string.Empty;
        public bool EnableEncryption { get; set; } = true;
    }
}
