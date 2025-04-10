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
            // If it's a SignalR hub or test endpoint, skip verification
            if (context.Request.Path.StartsWithSegments("/notificationHub") ||
                context.Request.Path.StartsWithSegments("/test.html") ||
                context.Request.Method == "GET")
            {
                await _next(context);
                return;
            }

            // Check API key in header
            if (!context.Request.Headers.TryGetValue(API_KEY_HEADER_NAME, out var extractedApiKey))
            {
                _logger.LogWarning("API key missing in request");
                context.Response.StatusCode = 401; // Unauthorized
                await context.Response.WriteAsJsonAsync(new { message = "API key is required" });
                return;
            }

            // Verify API key
            if (!_apiKeySettings.ApiKey.Equals(extractedApiKey))
            {
                _logger.LogWarning("Invalid API key");
                context.Response.StatusCode = 401; // Unauthorized
                await context.Response.WriteAsJsonAsync(new { message = "Invalid API key" });
                return;
            }

            // If API key is valid, continue
            await _next(context);
        }
    }

    // Extension method for easier middleware registration
    public static class ApiKeyAuthMiddlewareExtensions
    {
        public static IApplicationBuilder UseApiKeyAuth(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ApiKeyAuthMiddleware>();
        }
    }
}
