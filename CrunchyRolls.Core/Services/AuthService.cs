using CrunchyRolls.Core.Authentication.Interfaces;
using CrunchyRolls.Core.Authentication.Models;
using CrunchyRolls.Core.Services;
using System.Diagnostics;

namespace CrunchyRolls.Core.Authentication.Services
{
    /// <summary>
    /// Hoofdservice voor authenticatie
    /// Behandelt inloggen, uitloggen, token beheer en sessie
    /// 
    /// TEMPORARILY: Bypasses login and goes directly to products page
    /// TODO: Re-enable proper authentication when needed
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly ApiService _apiService;
        private readonly TokenService _tokenService;
        private readonly SecureStorageService _secureStorage;

        // Interne huige staat
        private AuthUser? _currentUser;
        private string? _currentToken;
        private bool _isAuthenticated;

        // Events
        public event EventHandler<AuthUser>? LoginSucceeded;
        public event EventHandler? LogoutCompleted;
        public event EventHandler<string>? AuthenticationFailed;

        // Properties
        public bool IsAuthenticated => _isAuthenticated;
        public AuthUser? CurrentUser => _currentUser;
        public string? CurrentToken => _currentToken;

        public AuthService(
            ApiService apiService,
            TokenService tokenService,
            SecureStorageService secureStorage)
        {
            _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _secureStorage = secureStorage ?? throw new ArgumentNullException(nameof(secureStorage));

            Debug.WriteLine("🔐 AuthService geïnitialiseerd");
        }

        /// <summary>
        /// Authenticatie initialiseren bij app start
        /// BYPASS: Automatically authenticates user and triggers LoginSucceeded event
        /// This allows the app to go directly to products page without login screen
        /// </summary>
        public async Task InitializeAsync()
        {
            try
            {
                Debug.WriteLine("🔐 Authenticatie initialiseren...");
                Debug.WriteLine("⚠️  BYPASS MODE: Skipping login, going directly to app");

                // Create a dummy/demo user for development
                var demoUser = new AuthUser
                {
                    Id = 1,
                    Email = "demo@localhost"
                };

                // Create a dummy token (doesn't validate against real API in demo mode)
                var demoToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwibmFtZSI6IkRlbW8gVXNlciIsImlhdCI6MTUxNjIzOTAyMn0.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";

                // Set as authenticated
                _isAuthenticated = true;
                _currentUser = demoUser;
                _currentToken = demoToken;

                // Set authorization token in API
                _apiService.SetAuthorizationToken(demoToken);

                Debug.WriteLine("✅ Demo user authenticated");
                Debug.WriteLine($"   → Email: {demoUser.Email}");
                Debug.WriteLine($"   → User ID: {demoUser.Id}");
                Debug.WriteLine("✅ Triggering LoginSucceeded event to show products page");

                // Trigger login succeeded event - this will show the products page
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    LoginSucceeded?.Invoke(this, demoUser);
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ KRITISCHE FOUT bij initialiseren authenticatie:");
                Debug.WriteLine($"   Type: {ex.GetType().Name}");
                Debug.WriteLine($"   Message: {ex.Message}");

                _isAuthenticated = false;
                _currentUser = null;
                _currentToken = null;
            }
        }

        /// <summary>
        /// Inloggen met email en wachtwoord
        /// </summary>
        public async Task<LoginResponse> LoginAsync(string email, string password)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                {
                    var response = new LoginResponse
                    {
                        Success = false,
                        Message = "Email en wachtwoord zijn verplicht"
                    };

                    AuthenticationFailed?.Invoke(this, response.Message);
                    return response;
                }

                Debug.WriteLine($"🔐 Poging inloggen voor {email}...");

                // Stuur login verzoek naar API
                var loginRequest = new LoginRequest { Email = email, Password = password };
                var loginResponse = await _apiService.PostAsync<LoginRequest, LoginResponse>(
                    "auth/login",
                    loginRequest);

                if (loginResponse == null || !loginResponse.Success)
                {
                    var errorMsg = loginResponse?.Message ?? "Inloggen mislukt";
                    Debug.WriteLine($"❌ Inloggen mislukt: {errorMsg}");
                    AuthenticationFailed?.Invoke(this, errorMsg);

                    return new LoginResponse
                    {
                        Success = false,
                        Message = errorMsg
                    };
                }

                // Inloggen succesvol - sla token en gebruiker op
                if (string.IsNullOrWhiteSpace(loginResponse.Token) || loginResponse.User == null)
                {
                    AuthenticationFailed?.Invoke(this, "Ongeldig antwoord van server");
                    return new LoginResponse
                    {
                        Success = false,
                        Message = "Ongeldig antwoord van server"
                    };
                }

                // Sla token en gebruikergegevens op
                _currentToken = loginResponse.Token;
                _currentUser = loginResponse.User;
                _isAuthenticated = true;

                // Bewaar in veilige opslag
                try
                {
                    await _secureStorage.SaveTokenAsync(_currentToken);
                    await _secureStorage.SaveUserDataAsync(
                        _currentUser.Id,
                        _currentUser.Email,
                        _currentUser.FullName);
                    Debug.WriteLine("✅ Gebruikersgegevens opgeslagen");
                }
                catch (Exception storageEx)
                {
                    Debug.WriteLine($"⚠️  Kon gebruikersgegevens niet opslaan: {storageEx.Message}");
                    // Don't fail - still authenticated
                }

                // Zet API token voor toekomstige verzoeken
                _apiService.SetAuthorizationToken(_currentToken);

                Debug.WriteLine($"✅ Inloggen succesvol voor {email}");
                LoginSucceeded?.Invoke(this, _currentUser);

                return loginResponse;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Fout tijdens inloggen: {ex.Message}");
                AuthenticationFailed?.Invoke(this, $"Inlogfout: {ex.Message}");

                return new LoginResponse
                {
                    Success = false,
                    Message = $"Inlogfout: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Huidge gebruiker uitloggen
        /// </summary>
        public async Task<bool> LogoutAsync()
        {
            try
            {
                Debug.WriteLine("🔓 Aan het uitloggen...");

                // Wis interne gegevens
                _currentToken = null;
                _currentUser = null;
                _isAuthenticated = false;

                // Wis API token
                _apiService.SetAuthorizationToken(null);

                // Wis veilige opslag
                _secureStorage.DeleteAllUserData();

                Debug.WriteLine("✅ Uitloggen succesvol");
                LogoutCompleted?.Invoke(this, EventArgs.Empty);

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Fout tijdens uitloggen: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Controleren of gebruiker nog ingelogd is
        /// Vernieuw sessie indien nodig
        /// </summary>
        public async Task<bool> CheckAuthenticationStatusAsync()
        {
            try
            {
                if (!_isAuthenticated || string.IsNullOrWhiteSpace(_currentToken))
                {
                    Debug.WriteLine("⚠️ Gebruiker niet ingelogd");
                    return false;
                }

                // Controleer of token nog geldig is
                if (!_tokenService.IsTokenValid(_currentToken))
                {
                    Debug.WriteLine("⚠️ Token verlopen");

                    // Probeer te vernieuwen
                    if (!await RefreshTokenAsync())
                    {
                        // Kan niet vernieuwen - moet opnieuw inloggen
                        await LogoutAsync();
                        AuthenticationFailed?.Invoke(this, "Sessie verlopen");
                        return false;
                    }
                }

                // Controleer of token binnenkort verloopt
                if (_tokenService.IsTokenExpiringSoon(_currentToken, minutesThreshold: 5))
                {
                    Debug.WriteLine("⚠️ Token verloopt binnenkort - probeer te vernieuwen");
                    await RefreshTokenAsync();
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Fout bij controle authenticatiestatus: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// JWT token vernieuwen als verlopen
        /// </summary>
        public async Task<bool> RefreshTokenAsync()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_currentToken))
                {
                    Debug.WriteLine("⚠️ Geen token om te vernieuwen");
                    return false;
                }

                Debug.WriteLine("🔄 Poging token te vernieuwen...");

                // Roep vernieuwings endpoint aan
                var response = await _apiService.PostAsync<object, LoginResponse>(
                    "auth/refresh",
                    new { token = _currentToken });

                if (response != null && !string.IsNullOrWhiteSpace(response.Token))
                {
                    // Update token
                    _currentToken = response.Token;

                    // Sla nieuwe token op
                    try
                    {
                        await _secureStorage.SaveTokenAsync(_currentToken);
                    }
                    catch { }

                    // Update API token
                    _apiService.SetAuthorizationToken(_currentToken);

                    Debug.WriteLine("✅ Token succesvol vernieuwd");
                    return true;
                }

                Debug.WriteLine("❌ Token vernieuwen mislukt");
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Fout bij vernieuwen token: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Controleren of huidge token geldig is
        /// </summary>
        public bool IsTokenValid()
        {
            if (string.IsNullOrWhiteSpace(_currentToken))
                return false;

            return _tokenService.IsTokenValid(_currentToken);
        }

        // ===== HULPMETHODES =====

        /// <summary>
        /// Resterende tijd voor huidge token ophalen
        /// </summary>
        public TimeSpan? GetTokenExpirationTime()
        {
            if (string.IsNullOrWhiteSpace(_currentToken))
                return null;

            return _tokenService.GetTimeUntilExpiration(_currentToken);
        }

        /// <summary>
        /// Rol van gebruiker ophalen
        /// </summary>
        public string? GetUserRole()
        {
            if (string.IsNullOrWhiteSpace(_currentToken))
                return null;

            return _tokenService.GetUserRole(_currentToken);
        }

        /// <summary>
        /// Controleren of gebruiker specifieke rol heeft
        /// </summary>
        public bool HasRole(string role)
        {
            var userRole = GetUserRole();
            return !string.IsNullOrWhiteSpace(userRole) &&
                   userRole.Equals(role, StringComparison.OrdinalIgnoreCase);
        }
    }
}