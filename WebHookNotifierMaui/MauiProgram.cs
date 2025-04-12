using Microsoft.Extensions.Logging;
using WebHookNotifierMaui.Data;
using WebHookNotifierMaui.Models;
using WebHookNotifierMaui.Services;
using WebHookNotifierMaui.Views;

namespace WebHookNotifierMaui;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		// Register services
		builder.Services.AddSingleton<MainPage>();
		builder.Services.AddSingleton<NotificationSettings>(NotificationSettings.Instance);
		builder.Services.AddSingleton<DatabaseService>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
