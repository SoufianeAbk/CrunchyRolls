using CrunchyRolls.Core.Authentication.Interfaces;
using CrunchyRolls.Core.Authentication.Services;
using CrunchyRolls.Core.Services;
using CrunchyRolls.Core.ViewModels;
using CrunchyRolls.Views;
using System.Diagnostics;

namespace CrunchyRolls
{
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

           
            builder.Services.AddSingleton<ApiService>();
            builder.Services.AddSingleton<TokenService>();

            
            builder.Services.AddSingleton<SecureStorageService>();

            
            builder.Services.AddSingleton<IAuthService, AuthService>();

            
            builder.Services.AddSingleton<ProductService>();

            
            builder.Services.AddSingleton<OrderService>();

            
            builder.Services.AddSingleton<LoginViewModel>();

            
            builder.Services.AddSingleton<ProductsViewModel>();

            
            builder.Services.AddSingleton<OrderViewModel>();

            
            builder.Services.AddSingleton<OrderHistoryViewModel>();

            
            builder.Services.AddSingleton<ProductDetailViewModel>();

            
            builder.Services.AddTransient<LoginPage>();

            
            builder.Services.AddSingleton<ProductsPage>();
            builder.Services.AddSingleton<OrderPage>();

            
            builder.Services.AddSingleton<OrderHistoryPage>();

            builder.Services.AddSingleton<ProductDetailPage>();

            

            var mauiApp = builder.Build();

            
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    var authService = mauiApp.Services.GetRequiredService<IAuthService>();

                    Debug.WriteLine("🔐 Authenticatie initialiseren bij app start...");
                    await authService.InitializeAsync();

                    Debug.WriteLine("✅ App gefinaliseerd");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"❌ Fout bij app initialisatie: {ex.Message}");
                }
            });

            return mauiApp;
        }
    }
}