using WebHookReceiverApi.Middleware;
using WebHookReceiverApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers();

// Add SignalR
builder.Services.AddSignalR();

// API key configuration
builder.Services.Configure<ApiKeySettings>(builder.Configuration.GetSection("AppSettings"));

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", builder =>
    {
        builder
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()
            .SetIsOriginAllowed(_ => true);
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Use CORS
app.UseCors("CorsPolicy");

// Enable static files
app.UseStaticFiles();

// Use API key authentication
app.UseApiKeyAuth();

// Map controllers
app.MapControllers();

// Map SignalR hub
app.MapHub<WebHookReceiverApi.Hubs.NotificationHub>("/notificationHub");

app.Run();
