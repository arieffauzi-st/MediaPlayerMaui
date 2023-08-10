using CommunityToolkit.Maui.Markup;
using MauiAppMedia.Pages;
using MauiAppMedia.ViewModels;
using MauiAppMedia.ViewModels.Views;
using CommunityToolkit.Maui.Maps;
using CommunityToolkit.Maui.Storage;
using Microsoft.Extensions.Logging;
using MauiAppMedia.Pages.Views;
using CommunityToolkit.Maui;
//using Polly;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]

namespace MauiAppMedia;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder()
#if DEBUG
								
#else
								.UseMauiCommunityToolkit(options =>
								{
									options.SetShouldSuppressExceptionsInConverters(true);
									options.SetShouldSuppressExceptionsInBehaviors(true);
									options.SetShouldSuppressExceptionsInAnimations(true);
								})
#endif
								.UseMauiCommunityToolkitMediaElement()
                                .UseMauiCommunityToolkitMarkup()
                                .UseMauiApp<App>()
                                .ConfigureFonts(fonts =>
                                {
                                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                                });



        RegisterViewsAndViewModels(builder.Services);

#if DEBUG
		builder.Logging.AddDebug().SetMinimumLevel(LogLevel.Trace);
#endif

		return builder.Build();

		static TimeSpan SleepDurationProvider(int attemptNumber) => TimeSpan.FromSeconds(Math.Pow(2, attemptNumber));
	}

	static void RegisterViewsAndViewModels(in IServiceCollection services)
	{
		//services.AddTransientWithShellRoute<MediaElementPage, MediaElementViewModel>();
        services.AddTransient<MediaElementPage>();
        services.AddTransient<MediaElementViewModel>();
    }
}