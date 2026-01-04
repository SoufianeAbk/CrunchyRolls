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
            Debug.WriteLine("\n" + new string('=', 60));
            Debug.WriteLine("🚀 STARTING APP INITIALIZATION");
            Debug.WriteLine(new string('=', 60) + "\n");

            try
            {
                Debug.WriteLine("📦 [1/3] Initializing local database...");
                try
                {
                    LocalDatabaseInitializer.InitializeAsync().Wait();
                    Debug.WriteLine("✅ [1/3] Database initialized\n");
                }
                catch (Exception dbEx)
                {
                    Debug.WriteLine($"⚠️  [1/3] Database init failed: {dbEx.Message}");
                    Debug.WriteLine($"         {dbEx.InnerException?.Message}\n");
                }

                Debug.WriteLine("🔐 [2/3] Initializing authentication...");
                try
                {
                    var authService = mauiApp.Services.GetRequiredService<IAuthService>();

                    Debug.WriteLine("   → Calling AuthService.InitializeAsync()");
                    authService.InitializeAsync().Wait();
                    Debug.WriteLine("   ✅ AuthService.InitializeAsync() completed successfully");

                    Debug.WriteLine("✅ [2/3] Authentication initialized\n");
                }
                catch (AggregateException aggEx)
                {
                    Debug.WriteLine($"❌ [2/3] AGGREGATE EXCEPTION in AuthService:");
                    foreach (var ex in aggEx.InnerExceptions)
                    {
                        Debug.WriteLine($"   ├─ {ex.GetType().Name}: {ex.Message}");
                        Debug.WriteLine($"   └─ Stack: {ex.StackTrace?.Substring(0, Math.Min(200, ex.StackTrace?.Length ?? 0))}");
                    }
                    Debug.WriteLine("");
                }
                catch (Exception authEx)
                {
                    Debug.WriteLine($"❌ [2/3] Exception in AuthService:");
                    Debug.WriteLine($"   Type: {authEx.GetType().Name}");
                    Debug.WriteLine($"   Message: {authEx.Message}");
                    Debug.WriteLine($"   Stack: {authEx.StackTrace?.Substring(0, Math.Min(300, authEx.StackTrace?.Length ?? 0))}");
                    Debug.WriteLine($"   InnerException: {authEx.InnerException?.Message}\n");
                }

                Debug.WriteLine(new string('=', 60));
                Debug.WriteLine("✅ APP INITIALIZATION COMPLETE - READY TO SHOW UI");
                Debug.WriteLine(new string('=', 60) + "\n");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ CRITICAL ERROR: {ex.GetType().Name}");
                Debug.WriteLine($"   Message: {ex.Message}");
                Debug.WriteLine($"   Stack: {ex.StackTrace}");
            }

            return mauiApp;
        }
    }
}