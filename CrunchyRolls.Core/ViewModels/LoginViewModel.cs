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
    /// VERBETERD: Beter error handling en debugging
    /// </summary>
    public class LoginViewModel : BaseViewModel
    {
        private readonly IAuthService _authService;

        // Properties
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
                    // Refresh CanExecute wanneer Email verandert
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
                    // Refresh CanExecute wanneer Password verandert
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

            // Commands initialiseren
            LoginCommand = new AsyncRelayCommand(OnLoginAsync, CanLogin);
            RegisterCommand = new Command(OnRegisterAsync);
            TestApiCommand = new Command(async () => await TestApiConnectionAsync());

            // Events abonneren
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

                // Probeer een simpele GET naar categories
                using (var client = new HttpClient())
                {
                    var url = "http://localhost:5000/api/categories";
                    Debug.WriteLine($"🔧 TEST: Contacting {url}");

                    var response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        ErrorMessage = $"✅ API Succesvol bereikt! Status: {response.StatusCode}";
                        Debug.WriteLine("✅ API is bereikbaar!");
                    }
                    else
                    {
                        ErrorMessage = $"❌ API Error: {response.StatusCode} {response.ReasonPhrase}";
                        Debug.WriteLine($"❌ API Error: {response.StatusCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"❌ API Onbereikbaar: {ex.Message}";
                Debug.WriteLine($"❌ TEST Exception: {ex.Message}\n{ex.StackTrace}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// Wordt aangeroepen wanneer pagina zichtbaar wordt
        /// </summary>
        public async Task OnAppearingAsync()
        {
            try
            {
                Debug.WriteLine("📱 LoginPage OnAppearing - checking auth status");

                // Controleer of al ingelogd
                if (_authService.IsAuthenticated)
                {
                    Debug.WriteLine("✅ Gebruiker al ingelogd - navigeer naar producten");
                    await Shell.Current.GoToAsync("//producten");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Fout in OnAppearing: {ex.Message}");
            }
        }

        // ===== INLOG LOGICA =====

        /// <summary>
        /// Inloggen uitvoeren
        /// </summary>
        private async Task OnLoginAsync()
        {
            Debug.WriteLine("🔐 LoginCommand triggered!");

            if (!ValidateInput())
            {
                Debug.WriteLine("❌ Input validation failed");
                return;
            }

            try
            {
                IsBusy = true;
                ErrorMessage = string.Empty;

                Debug.WriteLine($"🔐 Poging inloggen voor {Email}...");
                Debug.WriteLine($"🔐 AuthService IsAuthenticated: {_authService.IsAuthenticated}");

                var response = await _authService.LoginAsync(Email, Password);

                Debug.WriteLine($"🔐 LoginAsync response: Success={response.Success}, Message={response.Message}");

                if (response.Success)
                {
                    Debug.WriteLine($"✅ Inloggen succesvol");
                    // Navigatie wordt afgehandeld door LoginSucceeded event
                }
                else
                {
                    ErrorMessage = response.Message ?? "Inloggen mislukt";
                    Debug.WriteLine($"❌ Inloggen mislukt: {ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Fout: {ex.Message}";
                Debug.WriteLine($"❌ Uitzondering tijdens inloggen: {ex.Message}");
                Debug.WriteLine($"❌ Stack trace: {ex.StackTrace}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// Valideer invoergegevens
        /// </summary>
        private bool ValidateInput()
        {
            Debug.WriteLine("🔍 Validating input...");

            if (string.IsNullOrWhiteSpace(Email))
            {
                ErrorMessage = "Vul je email in";
                Debug.WriteLine("❌ Email is empty");
                return false;
            }

            if (!Email.Contains("@"))
            {
                ErrorMessage = "Voer een geldig email adres in";
                Debug.WriteLine("❌ Email format invalid");
                return false;
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Voer je wachtwoord in";
                Debug.WriteLine("❌ Password is empty");
                return false;
            }

            if (Password.Length < 6)
            {
                ErrorMessage = "Wachtwoord moet minstens 6 tekens zijn";
                Debug.WriteLine("❌ Password too short");
                return false;
            }

            Debug.WriteLine("✅ Input validation passed");
            return true;
        }

        /// <summary>
        /// Controleren of inloggen mogelijk is
        /// </summary>
        private bool CanLogin()
        {
            bool canLogin = !IsBusy && !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(Password);
            return canLogin;
        }

        // ===== REGISTRATIE LOGICA =====

        /// <summary>
        /// Registratie afhandelen
        /// </summary>
        private async void OnRegisterAsync()
        {
            try
            {
                await DisplayAlert(
                    "Registratie",
                    "Registratie functie wordt binnenkort toegevoegd!",
                    "OK");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Fout: {ex.Message}");
            }
        }

        // ===== HULPMETHODES =====

        /// <summary>
        /// Toon alert dialog
        /// </summary>
        private Task DisplayAlert(string title, string message, string cancel)
        {
            if (Application.Current?.MainPage != null)
            {
                return Application.Current.MainPage.DisplayAlert(title, message, cancel);
            }
            return Task.CompletedTask;
        }

        // ===== EVENT HANDLERS =====

        /// <summary>
        /// Afgehandeld wanneer inloggen succesvol is
        /// </summary>
        private void OnLoginSucceeded(object? sender, AuthUser user)
        {
            try
            {
                Debug.WriteLine($"🎉 Inloggen succesvol voor {user.Email}");

                // Navigeer naar hoofdapp
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    try
                    {
                        await Shell.Current.GoToAsync("//producten");
                        ClearForm();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"❌ Fout bij navigatie na inloggen: {ex.Message}");
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Fout in OnLoginSucceeded: {ex.Message}");
            }
        }

        /// <summary>
        /// Afgehandeld wanneer authenticatie faalt
        /// </summary>
        private void OnAuthenticationFailed(object? sender, string errorMessage)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                ErrorMessage = errorMessage ?? "Authenticatie mislukt";
                Debug.WriteLine($"❌ Auth mislukt: {ErrorMessage}");
            });
        }

        // ===== OPSCHONING =====

        /// <summary>
        /// Formulier wissen
        /// </summary>
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

            // Events afmelden
            _authService.LoginSucceeded -= OnLoginSucceeded;
            _authService.AuthenticationFailed -= OnAuthenticationFailed;

            ClearForm();

            Debug.WriteLine("🗑️ LoginViewModel verwijderd");
        }

        /// <summary>
        /// Formulier invullen met test gegevens (voor ontwikkeling)
        /// </summary>
        public void FillTestData()
        {
            Email = "test@example.com";
            Password = "Password123";
        }
    }
}