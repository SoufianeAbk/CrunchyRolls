using CrunchyRolls.Core.Authentication.Interfaces;
using System.Diagnostics;

namespace CrunchyRolls
{
    /// <summary>
    /// AppShell code-behind
    /// Behandelt navigatie tussen LoginPage en MainApp
    /// </summary>
    public partial class AppShell : Shell
    {
        private IAuthService? _authService;

        public AppShell()
        {
            InitializeComponent();
            Debug.WriteLine("📱 AppShell geïnitialiseerd");
        }

        /// <summary>
        /// Wordt aangeroepen wanneer AppShell zichtbaar wordt
        /// </summary>
        protected override async void OnAppearing()
        {
            base.OnAppearing();

            try
            {
                // Haal AuthService op
                _authService = Application.Current?.Handler.MauiContext?.Services
                    .GetService<IAuthService>();

                if (_authService == null)
                {
                    Debug.WriteLine("❌ AuthService niet beschikbaar");
                    return;
                }

                // Controleer inlogstatus
                UpdateNavigationBasedOnAuth(_authService.IsAuthenticated);

                // Abonneer op logout event
                _authService.LogoutCompleted += OnUserLoggedOut;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Fout in OnAppearing: {ex.Message}");
            }
        }

        /// <summary>
        /// Maak navigatie visible/invisible op basis van authenticatie
        /// </summary>
        private void UpdateNavigationBasedOnAuth(bool isAuthenticated)
        {
            if (isAuthenticated)
            {
                // Gebruiker ingelogd → toon main app tabbladen
                Debug.WriteLine("✅ Gebruiker ingelogd - toon MainTabs");
                MainTabs.IsVisible = true;

                // Navigeer naar producten pagina
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    try
                    {
                        await Shell.Current.GoToAsync("//producten");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"❌ Navigatie fout: {ex.Message}");
                    }
                });
            }
            else
            {
                // Gebruiker niet ingelogd → toon login pagina
                Debug.WriteLine("🔓 Gebruiker niet ingelogd - toon LoginPage");
                MainTabs.IsVisible = false;

                // Navigeer naar login pagina
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    try
                    {
                        await Shell.Current.GoToAsync("login");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"❌ Navigatie fout: {ex.Message}");
                    }
                });
            }
        }

        /// <summary>
        /// Afgehandeld wanneer gebruiker uitlogt
        /// </summary>
        private void OnUserLoggedOut(object? sender, EventArgs e)
        {
            Debug.WriteLine("🔓 Gebruiker uitgelogd - navigeer naar login");

            MainThread.BeginInvokeOnMainThread(() =>
            {
                MainTabs.IsVisible = false;
                UpdateNavigationBasedOnAuth(false);
            });
        }

        /// <summary>
        /// Opschoning bij weergaan
        /// </summary>
        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            // Af-abonneren van events
            if (_authService != null)
            {
                _authService.LogoutCompleted -= OnUserLoggedOut;
            }
        }
    }
}