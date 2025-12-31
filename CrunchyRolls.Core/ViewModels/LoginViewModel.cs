using CrunchyRolls.Core.Authentication.Interfaces;
using CrunchyRolls.Core.Authentication.Models;
using CrunchyRolls.Core.Helpers;
using System.Diagnostics;
using System.Windows.Input;

namespace CrunchyRolls.Core.ViewModels
{
    /// <summary>
    /// ViewModel voor inlogpagina
    /// Behandelt inloglogica en formuliervalidatie
    /// </summary>
    public class LoginViewModel : BaseViewModel
    {
        private readonly IAuthService _authService;

        private string _email = string.Empty;
        private string _password = string.Empty;
        private bool _rememberMe = false;
        private string _errorMessage = string.Empty;
        private bool _isErrorVisible = false;

        public string Email
        {
            get => _email;
            set
            {
                if (SetProperty(ref _email, value))
                {
                    if (LoginCommand is AsyncRelayCommand cmd)
                        cmd.RaiseCanExecuteChanged();
                }
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                if (SetProperty(ref _password, value))
                {
                    if (LoginCommand is AsyncRelayCommand cmd)
                        cmd.RaiseCanExecuteChanged();
                }
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                SetProperty(ref _errorMessage, value);
                IsErrorVisible = !string.IsNullOrWhiteSpace(value);
            }
        }

        public bool IsErrorVisible
        {
            get => _isErrorVisible;
            set => SetProperty(ref _isErrorVisible, value);
        }

        // Commands
        public ICommand LoginCommand { get; }
        public ICommand RegisterCommand { get; }
        public ICommand TestApiCommand { get; }

        public LoginViewModel(IAuthService authService)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));

            Title = "Inloggen";

            LoginCommand = new AsyncRelayCommand(OnLoginAsync, CanLogin);
            RegisterCommand = new Command(OnRegisterAsync);
            TestApiCommand = new Command(async () => await TestApiConnectionAsync());

            _authService.LoginSucceeded += OnLoginSucceeded;
            _authService.AuthenticationFailed += OnAuthenticationFailed;

            Debug.WriteLine("📋 LoginViewModel geïnitialiseerd");
        }

        /// <summary>
        /// Test of API bereikbaar is
        /// </summary>
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

                // ✅ CONSISTENT met ApiService.BaseUrl
                var url = "http://localhost:5000/api/categories";
                Debug.WriteLine($"🔧 TEST: Contacting {url}");

                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    ErrorMessage =
                        $"✅ API succesvol bereikt ({(int)response.StatusCode})";
                }
                else
                {
                    ErrorMessage =
                        $"❌ API fout ({(int)response.StatusCode}) - {response.ReasonPhrase}";
                }
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

        public async Task OnAppearingAsync()
        {
            try
            {
                Debug.WriteLine("📱 LoginPage OnAppearing - checking auth status");

                if (_authService.IsAuthenticated)
                {
                    await Shell.Current.GoToAsync("//producten");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Fout in OnAppearing: {ex.Message}");
            }
        }

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

        private bool CanLogin()
        {
            return !IsBusy &&
                   !string.IsNullOrWhiteSpace(Email) &&
                   !string.IsNullOrWhiteSpace(Password);
        }

        private async void OnRegisterAsync()
        {
            await DisplayAlert(
                "Registratie",
                "Registratie functie wordt binnenkort toegevoegd!",
                "OK");
        }

        private Task DisplayAlert(string title, string message, string cancel)
        {
            if (Application.Current?.MainPage != null)
            {
                return Application.Current.MainPage.DisplayAlert(title, message, cancel);
            }
            return Task.CompletedTask;
        }

        private void OnLoginSucceeded(object? sender, AuthUser user)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Shell.Current.GoToAsync("//app/producten");
                ClearForm();
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
            _rememberMe = false;
            ErrorMessage = string.Empty;
        }

        public override void Dispose()
        {
            base.Dispose();

            _authService.LoginSucceeded -= OnLoginSucceeded;
            _authService.AuthenticationFailed -= OnAuthenticationFailed;

            ClearForm();
        }

        public void FillTestData()
        {
            Email = "test@example.com";
            Password = "Password123";
        }
    }
}
