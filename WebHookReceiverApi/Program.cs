using WebHookReceiverApi.Middleware;
using WebHookReceiverApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers();

// Přidání SignalR
builder.Services.AddSignalR();

// Konfigurace API klíče
builder.Services.Configure<ApiKeySettings>(builder.Configuration.GetSection("AppSettings"));

// Přidání CORS
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

// Použití CORS
app.UseCors("CorsPolicy");

// Povolení statických souborů
app.UseStaticFiles();

// Použití API klíče pro autentizaci
app.UseApiKeyAuth();

// Mapování controllerů
app.MapControllers();

// Mapování SignalR hubu
app.MapHub<WebHookReceiverApi.Hubs.NotificationHub>("/notificationHub");

app.Run();
