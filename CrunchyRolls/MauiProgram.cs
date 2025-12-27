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

        // Register Services (uit Core)
        builder.Services.AddSingleton<ApiService>();
        builder.Services.AddSingleton<ProductService>();
        builder.Services.AddSingleton<OrderService>();

        // Register ViewModels (uit Core)
        builder.Services.AddTransient<ProductsViewModel>();
        builder.Services.AddTransient<ProductDetailViewModel>();
        builder.Services.AddTransient<OrderViewModel>();
        builder.Services.AddTransient<OrderHistoryViewModel>();

        // Register Views (in MAUI project)
        builder.Services.AddSingleton<MainPage>();
        builder.Services.AddTransient<ProductsPage>();
        builder.Services.AddTransient<ProductDetailPage>();
        builder.Services.AddTransient<OrderPage>();
        builder.Services.AddTransient<OrderHistoryPage>();

        // Register AppShell (in MAUI project)
        builder.Services.AddSingleton<AppShell>();

#if DEBUG
        object debugLogger = builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}