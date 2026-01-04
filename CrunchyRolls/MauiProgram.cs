using CrunchyRolls.Core.Authentication.Interfaces;
using CrunchyRolls.Core.Authentication.Services;
using CrunchyRolls.Core.Data.Seeders;
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

            // Services
            builder.Services.AddSingleton<ApiService>();
            builder.Services.AddSingleton<TokenService>();
            builder.Services.AddSingleton<SecureStorageService>();
            builder.Services.AddSingleton<IAuthService, AuthService>();

            builder.Services.AddSingleton<HybridProductService>();
            builder.Services.AddSingleton<HybridOrderService>();

            // ViewModels
            builder.Services.AddSingleton<LoginViewModel>();
            builder.Services.AddSingleton<ProductsViewModel>();
            builder.Services.AddSingleton<OrderViewModel>();
            builder.Services.AddSingleton<OrderHistoryViewModel>();
            builder.Services.AddSingleton<ProductDetailViewModel>();

            // Pages
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddSingleton<ProductsPage>();
            builder.Services.AddSingleton<OrderPage>();
            builder.Services.AddSingleton<OrderHistoryPage>();
            builder.Services.AddSingleton<ProductDetailPage>();

            var mauiApp = builder.Build();

            // ✅ CRITICAL: Initialize database SYNCHRONOUSLY before showing UI
            // This ensures products are loaded when ProductsPage.OnAppearing() is called
            Debug.WriteLine("🚀 Starting app initialization...");

            try
            {
                Debug.WriteLine("🗄️ [1/3] Initializing local database...");
                LocalDatabaseInitializer.InitializeAsync().Wait(); // ⚠️ BLOCKING - must complete before UI
                Debug.WriteLine("✅ [1/3] Local database ready - products seeded");

                Debug.WriteLine("🔐 [2/3] Initializing authentication...");
                var authService = mauiApp.Services.GetRequiredService<IAuthService>();
                authService.InitializeAsync().Wait(); // ⚠️ BLOCKING - must complete before UI
                Debug.WriteLine("✅ [2/3] Authentication initialized");

                Debug.WriteLine("📊 [3/3] App ready to show UI");
                Debug.WriteLine("✅ ALL INITIALIZATION COMPLETE - UI safe to load\n");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ CRITICAL ERROR during app initialization: {ex.Message}");
                Debug.WriteLine($"   Type: {ex.GetType().Name}");
                Debug.WriteLine($"   Stack: {ex.StackTrace}");
                throw; // Re-throw to see the error
            }

            return mauiApp;
        }
    }
}