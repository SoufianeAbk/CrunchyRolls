using CrunchyRolls.Core.Authentication.Interfaces;
using CrunchyRolls.Core.Authentication.Services;
using CrunchyRolls.Core.Services;
using CrunchyRolls.Core.ViewModels;
using CrunchyRolls.Views;
using System.Diagnostics;

namespace CrunchyRolls
{
    /// <summary>
    /// MAUI App initialization
    /// Registratie van alle services, viewmodels en views
    /// </summary>
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

            // ===== CORE SERVICES =====

            /// <summary>
            /// API service voor backend communicatie
            /// </summary>
            builder.Services.AddSingleton<ApiService>();

            // ===== AUTHENTICATION SERVICES (Phase 3) =====

            /// <summary>
            /// Token service voor JWT parsing en validatie
            /// </summary>
            builder.Services.AddSingleton<TokenService>();

            /// <summary>
            /// Veilige opslag service (OS-versleuteld)
            /// Slaat tokens, credentials op
            /// </summary>
            builder.Services.AddSingleton<SecureStorageService>();

            /// <summary>
            /// Authenticatie service
            /// Behandelt login, logout, sessie beheer
            /// </summary>
            builder.Services.AddSingleton<IAuthService, AuthService>();

            // ===== BUSINESS SERVICES =====

            /// <summary>
            /// Product service voor producten
            /// </summary>
            builder.Services.AddSingleton<ProductService>();

            /// <summary>
            /// Order service voor bestellingen
            /// </summary>
            builder.Services.AddSingleton<OrderService>();

            // ===== VIEW MODELS =====

            /// <summary>
            /// ViewModel voor LoginPage
            /// </summary>
            builder.Services.AddSingleton<LoginViewModel>();

            /// <summary>
            /// ViewModel voor ProductsPage
            /// </summary>
            builder.Services.AddSingleton<ProductsViewModel>();

            /// <summary>
            /// ViewModel voor OrderPage (Winkelwagen)
            /// </summary>
            builder.Services.AddSingleton<OrderViewModel>();

            /// <summary>
            /// ViewModel voor OrderHistoryPage
            /// </summary>
            builder.Services.AddSingleton<OrderHistoryViewModel>();

            /// <summary>
            /// ViewModel voor ProductDetailPage
            /// </summary>
            builder.Services.AddSingleton<ProductDetailViewModel>();

            // ===== VIEWS / PAGES =====

            /// <summary>
            /// Login pagina
            /// </summary>
            builder.Services.AddSingleton<LoginPage>();

            /// <summary>
            /// Products pagina
            /// </summary>
            builder.Services.AddSingleton<ProductsPage>();

            /// <summary>
            /// Order/Cart pagina
            /// </summary>
            builder.Services.AddSingleton<OrderPage>();

            /// <summary>
            /// Order history pagina
            /// </summary>
            builder.Services.AddSingleton<OrderHistoryPage>();

            /// <summary>
            /// Product detail pagina
            /// </summary>
            builder.Services.AddSingleton<ProductDetailPage>();

            // ===== BUILD APP =====

            var mauiApp = builder.Build();

            // ===== INITIALISATIE =====

            // Initialiseer authenticatie wanneer app start
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