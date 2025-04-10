using Microsoft.Extensions.Options;
using WebHookReceiverApi.Models;

namespace WebHookReceiverApi.Middleware
{
    public class ApiKeyAuthMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ApiKeyAuthMiddleware> _logger;
        private readonly ApiKeySettings _apiKeySettings;
        private const string API_KEY_HEADER_NAME = "X-API-Key";

        public ApiKeyAuthMiddleware(
            RequestDelegate next,
            ILogger<ApiKeyAuthMiddleware> logger,
            IOptions<ApiKeySettings> apiKeySettings)
        {
            _next = next;
            _logger = logger;
            _apiKeySettings = apiKeySettings.Value;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Pokud jde o SignalR hub nebo testovací endpoint, přeskočíme ověření
            if (context.Request.Path.StartsWithSegments("/notificationHub") ||
                context.Request.Path.StartsWithSegments("/test.html") ||
                context.Request.Method == "GET")
            {
                await _next(context);
                return;
            }

            // Kontrola API klíče v hlavičce
            if (!context.Request.Headers.TryGetValue(API_KEY_HEADER_NAME, out var extractedApiKey))
            {
                _logger.LogWarning("API klíč chybí v požadavku");
                context.Response.StatusCode = 401; // Unauthorized
                await context.Response.WriteAsJsonAsync(new { message = "API klíč je vyžadován" });
                return;
            }

            // Ověření API klíče
            if (!_apiKeySettings.ApiKey.Equals(extractedApiKey))
            {
                _logger.LogWarning("Neplatný API klíč");
                context.Response.StatusCode = 401; // Unauthorized
                await context.Response.WriteAsJsonAsync(new { message = "Neplatný API klíč" });
                return;
            }

            // Pokud je API klíč platný, pokračujeme dál
            await _next(context);
        }
    }

    // Extension metoda pro snadnější registraci middleware
    public static class ApiKeyAuthMiddlewareExtensions
    {
        public static IApplicationBuilder UseApiKeyAuth(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ApiKeyAuthMiddleware>();
        }
    }
}
