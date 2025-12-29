using Microsoft.Maui.Storage;
using System.Diagnostics;

namespace CrunchyRolls.Core.Authentication.Services
{
    /// <summary>
    /// Service voor veilige opslag van tokens en credentials
    /// Gebruikt platform-specifieke versleuteling:
    /// iOS: Keychain, Android: Keystore, Windows: Credential Manager
    /// </summary>
    public class SecureStorageService
    {
        // Sleutels voor opslag
        private const string TokenKey = "auth_token";
        private const string UserIdKey = "user_id";
        private const string UserEmailKey = "user_email";
        private const string UserNameKey = "user_name";

        public SecureStorageService()
        {
            Debug.WriteLine("🔐 Veilige opslagservice geïnitialiseerd");
        }

        // ===== TOKEN OPSLAG =====

        /// <summary>
        /// JWT token veilig opslaan (versleuteld door OS)
        /// </summary>
        public async Task<bool> SaveTokenAsync(string token)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token))
                {
                    Debug.WriteLine("⚠️ Kan geen lege token opslaan");
                    return false;
                }

                await SecureStorage.SetAsync(TokenKey, token);
                Debug.WriteLine("🔐 Token veilig opgeslagen");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Fout bij opslaan token: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// JWT token ophalen uit veilige opslag
        /// </summary>
        public async Task<string?> GetTokenAsync()
        {
            try
            {
                var token = await SecureStorage.GetAsync(TokenKey);

                if (!string.IsNullOrWhiteSpace(token))
                {
                    Debug.WriteLine("🔓 Token opgehaald uit veilige opslag");
                    return token;
                }

                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Fout bij ophalen token: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Controleren of token opgeslagen is
        /// </summary>
        public async Task<bool> HasTokenAsync()
        {
            try
            {
                var token = await GetTokenAsync();
                return !string.IsNullOrWhiteSpace(token);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// JWT token verwijderen uit opslag
        /// </summary>
        public bool DeleteToken()
        {
            try
            {
                SecureStorage.Remove(TokenKey);
                Debug.WriteLine("🗑️ Token verwijderd uit veilige opslag");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Fout bij verwijderen token: {ex.Message}");
                return false;
            }
        }

        // ===== GEBRUIKER GEGEVENS OPSLAG =====

        /// <summary>
        /// Gebruikersinformatie veilig opslaan
        /// </summary>
        public async Task<bool> SaveUserDataAsync(int userId, string email, string name)
        {
            try
            {
                await SecureStorage.SetAsync(UserIdKey, userId.ToString());
                await SecureStorage.SetAsync(UserEmailKey, email);
                await SecureStorage.SetAsync(UserNameKey, name);

                Debug.WriteLine($"🔐 Gebruikergegevens veilig opgeslagen: {email}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Fout bij opslaan gebruikergegevens: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Opgeslagen gebruiker ID ophalen
        /// </summary>
        public async Task<int?> GetUserIdAsync()
        {
            try
            {
                var id = await SecureStorage.GetAsync(UserIdKey);

                if (int.TryParse(id, out var userId))
                {
                    return userId;
                }

                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Fout bij ophalen gebruiker ID: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Opgeslagen gebruiker email ophalen
        /// </summary>
        public async Task<string?> GetUserEmailAsync()
        {
            try
            {
                return await SecureStorage.GetAsync(UserEmailKey);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Fout bij ophalen email: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Opgeslagen gebruiker naam ophalen
        /// </summary>
        public async Task<string?> GetUserNameAsync()
        {
            try
            {
                return await SecureStorage.GetAsync(UserNameKey);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Fout bij ophalen naam: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Alle gebruikergegevens uit opslag verwijderen
        /// </summary>
        public bool DeleteAllUserData()
        {
            try
            {
                SecureStorage.Remove(TokenKey);
                SecureStorage.Remove(UserIdKey);
                SecureStorage.Remove(UserEmailKey);
                SecureStorage.Remove(UserNameKey);

                Debug.WriteLine("🗑️ Alle gebruikergegevens verwijderd uit veilige opslag");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Fout bij verwijderen gebruikergegevens: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Controleren of gebruikergegevens opgeslagen zijn
        /// </summary>
        public async Task<bool> HasUserDataAsync()
        {
            try
            {
                var token = await GetTokenAsync();
                return !string.IsNullOrWhiteSpace(token);
            }
            catch
            {
                return false;
            }
        }

        // ===== GENERIEKE OPSLAG =====

        /// <summary>
        /// Willekeurige waarde veilig opslaan
        /// </summary>
        public async Task<bool> SetAsync(string key, string value)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value))
                    return false;

                await SecureStorage.SetAsync(key, value);
                Debug.WriteLine($"🔐 Opgeslagen: {key}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Fout bij opslaan {key}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Willekeurige waarde uit veilige opslag ophalen
        /// </summary>
        public async Task<string?> GetAsync(string key)
        {
            try
            {
                return await SecureStorage.GetAsync(key);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Fout bij ophalen {key}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Waarde uit veilige opslag verwijderen
        /// </summary>
        public bool Remove(string key)
        {
            try
            {
                SecureStorage.Remove(key);
                Debug.WriteLine($"🗑️ Verwijderd: {key}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Fout bij verwijderen {key}: {ex.Message}");
                return false;
            }
        }
    }
}