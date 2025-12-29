using CrunchyRolls.Core.Authentication.Interfaces;
using CrunchyRolls.Core.Authentication.Models;
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

        // Properties
        private string _email = string.Empty;
        private string _password = string.Empty;
        private bool _rememberMe = false;
        private string _errorMessage = string.Empty;
        private bool _isErrorVisible = false;

        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public bool RememberMe
        {
            get => _rememberMe;
            set => SetProperty(ref _rememberMe, value);
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

        public LoginViewModel(IAuthService authService)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));

            Title = "Inloggen";

            // Commands initialiseren
            LoginCommand = new Command(async () => await OnLoginAsync(), CanLogin);
            RegisterCommand = new Command(OnRegisterAsync);

            // Events abonneren
            _authService.LoginSucceeded += OnLoginSucceeded;
            _authService.AuthenticationFailed += OnAuthenticationFailed;

            Debug.WriteLine("📋 LoginViewModel geïnitialiseerd");
        }

        /// <summary>
        /// Wordt aangeroepen wanneer pagina zichtbaar wordt
        /// </summary>
        public async Task OnAppearingAsync()
        {
            try
            {
                // Controleer of al ingelogd
                if (_authService.IsAuthenticated)
                {
                    Debug.WriteLine("✅ Gebruiker al ingelogd - navigeer naar producten");

                    // Navigeer naar producten
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
            if (!ValidateInput())
                return;

            try
            {
                IsBusy = true;
                ErrorMessage = string.Empty;

                Debug.WriteLine($"🔐 Poging inloggen voor {Email}...");

                var response = await _authService.LoginAsync(Email, Password);

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

        /// <summary>
        /// Controleren of inloggen mogelijk is
        /// </summary>
        private bool CanLogin()
        {
            return !IsBusy && !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(Password);
        }

        // ===== REGISTRATIE LOGICA =====

        /// <summary>
        /// Registratie afhandelen
        /// </summary>
        private async void OnRegisterAsync()
        {
            try
            {
                // Voor nu gewoon een bericht tonen
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
            RememberMe = false;
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

        // ===== TEST GEGEVENS =====

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