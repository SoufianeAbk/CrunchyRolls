using CrunchyRolls.Core.Services;
using CrunchyRolls.Core.ViewModels;
using CrunchyRolls.Views;
using Microsoft.Extensions.Logging;

namespace CrunchyRolls;

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

        // ===== LOGGING SETUP =====
#if DEBUG
        builder
            .Services
            .AddLogging(logging =>
            {
                logging.AddDebug();
                logging.SetMinimumLevel(LogLevel.Information);
                logging.AddFilter("CrunchyRolls", LogLevel.Debug);
            });
#endif

        // ===== SERVICE REGISTRATION =====
        // Register ApiService as singleton (will create HttpClient internally)
        builder.Services.AddSingleton<ApiService>();

        builder.Services.AddSingleton<ProductService>();
        builder.Services.AddSingleton<OrderService>();

        // ===== VIEWMODEL REGISTRATION =====
        builder.Services.AddTransient<ProductsViewModel>();
        builder.Services.AddTransient<ProductDetailViewModel>();
        builder.Services.AddTransient<OrderViewModel>();
        builder.Services.AddTransient<OrderHistoryViewModel>();

        // ===== VIEW REGISTRATION =====
        builder.Services.AddSingleton<MainPage>();
        builder.Services.AddTransient<ProductsPage>();
        builder.Services.AddTransient<ProductDetailPage>();
        builder.Services.AddTransient<OrderPage>();
        builder.Services.AddTransient<OrderHistoryPage>();

        // ===== APP SHELL =====
        builder.Services.AddSingleton<AppShell>();

        return builder.Build();
    }
}