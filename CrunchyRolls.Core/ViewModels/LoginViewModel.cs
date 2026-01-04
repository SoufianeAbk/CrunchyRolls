using CrunchyRolls.Core.Authentication.Interfaces;
using CrunchyRolls.Core.Authentication.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using System.Windows.Input;

namespace CrunchyRolls.Core.ViewModels
{
    /// <summary>
    /// ViewModel voor inlogpagina
    /// Behandelt inloglogica en formuliervalidatie
    /// ✅ Refactored naar CommunityToolkit.Mvvm
    /// </summary>
    public partial class LoginViewModel : BaseViewModel
    {
        private readonly IAuthService _authService;

        [ObservableProperty]
        private string email = string.Empty;

        [ObservableProperty]
        private string password = string.Empty;

        [ObservableProperty]
        private bool rememberMe = false;

        [ObservableProperty]
        private string errorMessage = string.Empty;

        [ObservableProperty]
        private bool isErrorVisible = false;

        public LoginViewModel(IAuthService authService)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));

            Title = "Inloggen";

            _authService.LoginSucceeded += OnLoginSucceeded;
            _authService.AuthenticationFailed += OnAuthenticationFailed;

            Debug.WriteLine("📋 LoginViewModel geïnitialiseerd");
        }

        // ===== COMMANDS =====

        [RelayCommand]
        private async Task OnLoginAsync()
        {
            Debug.WriteLine("🔐 LoginCommand triggered!");

            if (!ValidateInput())
                return;

            try
            {
                IsBusy = true;
                ErrorMessage = string.Empty;

                var response = await _authService.LoginAsync(Email, Password);

                if (!response.Success)
                    ErrorMessage = response.Message ?? "Inloggen mislukt";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Fout: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task OnRegisterAsync()
        {
            await DisplayAlert(
                "Registratie",
                "Registratie functie wordt binnenkort toegevoegd!",
                "OK");
        }

        [RelayCommand]
        private async Task TestApiConnectionAsync()
        {
            try
            {
                Debug.WriteLine("🔧 TEST: Controleren API verbinding...");
                IsBusy = true;
                ErrorMessage = "Testing API connection...";

                using var client = new HttpClient
                {
                    Timeout = TimeSpan.FromSeconds(5)
                };

                var url = "http://localhost:5000/api/categories";
                Debug.WriteLine($"🔧 TEST: Contacting {url}");

                var response = await client.GetAsync(url);

                ErrorMessage = response.IsSuccessStatusCode
                    ? $"✅ API succesvol bereikt ({(int)response.StatusCode})"
                    : $"❌ API fout ({(int)response.StatusCode}) - {response.ReasonPhrase}";
            }
            catch (TaskCanceledException)
            {
                ErrorMessage = "❌ Timeout: API reageert niet";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"❌ API onbereikbaar: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        // ===== PRIVATE METHODS =====

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(Email))
            {
                ErrorMessage = "Vul je email in";
                return false;
            }

            if (!Email.Contains("@"))
            {
                ErrorMessage = "Voer een geldig email adres in";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Voer je wachtwoord in";
                return false;
            }

            if (Password.Length < 6)
            {
                ErrorMessage = "Wachtwoord moet minstens 6 tekens zijn";
                return false;
            }

            return true;
        }

        private async void OnLoginSucceeded(object? sender, AuthUser user)
        {
            Debug.WriteLine("✅ LoginSucceeded - navigating to orders");
            ClearForm();

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    await Task.Delay(200);

                    // Navigate to orders - TabBar will automatically show
                    await Shell.Current.GoToAsync("orders");
                    Debug.WriteLine("✅ Navigated to orders successfully");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"❌ Navigation error: {ex.Message}");
                    Debug.WriteLine($"❌ Exception type: {ex.GetType().Name}");
                }
            });
        }

        private void OnAuthenticationFailed(object? sender, string errorMessage)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                ErrorMessage = errorMessage ?? "Authenticatie mislukt";
            });
        }

        private void ClearForm()
        {
            Email = string.Empty;
            Password = string.Empty;
            RememberMe = false;
            ErrorMessage = string.Empty;
        }

        // ===== PUBLIC METHODS =====

        public Task OnAppearingAsync()
        {
            Debug.WriteLine("📱 LoginPage OnAppearing");
            return Task.CompletedTask;
        }

        public void FillTestData()
        {
            Email = "test@example.com";
            Password = "Password123";
        }

        // ===== CLEANUP =====

        public override void Dispose()
        {
            base.Dispose();

            _authService.LoginSucceeded -= OnLoginSucceeded;
            _authService.AuthenticationFailed -= OnAuthenticationFailed;

            ClearForm();
        }

        // ===== HELPER METHODS =====

        private Task DisplayAlert(string title, string message, string cancel)
        {
            return Application.Current?.MainPage != null
                ? Application.Current.MainPage.DisplayAlert(title, message, cancel)
                : Task.CompletedTask;
        }
    }
}