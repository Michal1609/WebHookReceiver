namespace WebHookReceiverApi.Models
{
    public class ApiKeySettings
    {
        public string ApiKey { get; set; } = string.Empty;
        public bool EnableEncryption { get; set; } = true;
    }
}
