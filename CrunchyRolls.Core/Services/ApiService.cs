using System.Diagnostics;
using System.Text.Json;

namespace CrunchyRolls.Core.Services
{
    /// <summary>
    /// API service voor communicatie met backend
    /// Inclusief JWT token authenticatie (Phase 3)
    /// </summary>
    public class ApiService
    {
        private readonly HttpClient _httpClient;

        // JWT token voor authenticatie
        private string? _authToken;

        private const string BaseUrl = "http://localhost:5000/api"; // Pas aan naar jouw API!

        public ApiService()
        {
            _httpClient = new HttpClient();
            Debug.WriteLine("📡 ApiService geïnitialiseerd");
        }

        // ===== AUTHENTICATIE =====

        /// <summary>
        /// Zet JWT token voor API autorisatie
        /// Voegt automatisch toe aan alle verzoeken
        /// </summary>
        public void SetAuthorizationToken(string? token)
        {
            _authToken = token;

            if (string.IsNullOrWhiteSpace(token))
            {
                // Verwijder Authorization header
                _httpClient.DefaultRequestHeaders.Remove("Authorization");
                Debug.WriteLine("🔓 Autorisatie header verwijderd");
            }
            else
            {
                // Voeg Authorization header toe met JWT token
                _httpClient.DefaultRequestHeaders.Remove("Authorization");
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
                Debug.WriteLine("🔒 Autorisatie header ingesteld");
            }
        }

        // ===== GET REQUESTS =====

        /// <summary>
        /// GET aanvraag met deserialisatie
        /// </summary>
        public async Task<T?> GetAsync<T>(string endpoint)
        {
            try
            {
                var url = $"{BaseUrl}{endpoint}";
                Debug.WriteLine($"📥 GET: {endpoint}");

                using (var response = await _httpClient.GetAsync(url))
                {
                    // Controleer op 401 Unauthorized (token verlopen/ongeldig)
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        Debug.WriteLine("❌ Niet geautoriseerd (401) - Token kan verlopen zijn");
                        throw new UnauthorizedAccessException("API authenticatie mislukt - log opnieuw in");
                    }

                    if (!response.IsSuccessStatusCode)
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        Debug.WriteLine($"❌ GET fout: {response.StatusCode} - {errorContent}");
                        return default;
                    }

                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    Debug.WriteLine($"✅ GET succesvol: {endpoint}");
                    return result;
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Herwerp authentication errors
                throw;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ GET fout ({endpoint}): {ex.Message}");
                return default;
            }
        }

        // ===== POST REQUESTS =====

        /// <summary>
        /// POST aanvraag met request body en deserialisatie
        /// </summary>
        public async Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest request)
        {
            try
            {
                var url = $"{BaseUrl}{endpoint}";
                Debug.WriteLine($"📤 POST: {endpoint}");

                var jsonContent = JsonSerializer.Serialize(request);
                var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

                using (var response = await _httpClient.PostAsync(url, content))
                {
                    // Controleer op 401 Unauthorized
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        Debug.WriteLine("❌ Niet geautoriseerd (401) - Token kan verlopen zijn");
                        throw new UnauthorizedAccessException("API authenticatie mislukt - log opnieuw in");
                    }

                    if (!response.IsSuccessStatusCode)
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        Debug.WriteLine($"❌ POST fout: {response.StatusCode} - {errorContent}");
                        return default;
                    }

                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<TResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    Debug.WriteLine($"✅ POST succesvol: {endpoint}");
                    return result;
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Herwerp authentication errors
                throw;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ POST fout ({endpoint}): {ex.Message}");
                return default;
            }
        }

        // ===== PUT REQUESTS =====

        /// <summary>
        /// PUT aanvraag voor update
        /// </summary>
        public async Task<bool> PutAsync<T>(string endpoint, T request)
        {
            try
            {
                var url = $"{BaseUrl}{endpoint}";
                Debug.WriteLine($"📝 PUT: {endpoint}");

                var jsonContent = JsonSerializer.Serialize(request);
                var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

                using (var response = await _httpClient.PutAsync(url, content))
                {
                    // Controleer op 401 Unauthorized
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        Debug.WriteLine("❌ Niet geautoriseerd (401)");
                        throw new UnauthorizedAccessException("API authenticatie mislukt - log opnieuw in");
                    }

                    if (!response.IsSuccessStatusCode)
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        Debug.WriteLine($"❌ PUT fout: {response.StatusCode} - {errorContent}");
                        return false;
                    }

                    Debug.WriteLine($"✅ PUT succesvol: {endpoint}");
                    return true;
                }
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ PUT fout ({endpoint}): {ex.Message}");
                return false;
            }
        }

        // ===== DELETE REQUESTS =====

        /// <summary>
        /// DELETE aanvraag
        /// </summary>
        public async Task<bool> DeleteAsync(string endpoint)
        {
            try
            {
                var url = $"{BaseUrl}{endpoint}";
                Debug.WriteLine($"🗑️ DELETE: {endpoint}");

                using (var response = await _httpClient.DeleteAsync(url))
                {
                    // Controleer op 401 Unauthorized
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        Debug.WriteLine("❌ Niet geautoriseerd (401)");
                        throw new UnauthorizedAccessException("API authenticatie mislukt - log opnieuw in");
                    }

                    if (!response.IsSuccessStatusCode)
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        Debug.WriteLine($"❌ DELETE fout: {response.StatusCode} - {errorContent}");
                        return false;
                    }

                    Debug.WriteLine($"✅ DELETE succesvol: {endpoint}");
                    return true;
                }
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ DELETE fout ({endpoint}): {ex.Message}");
                return false;
            }
        }

        // ===== HULPMETHODES =====

        /// <summary>
        /// Controleer of API bereikbaar is
        /// </summary>
        public async Task<bool> IsApiAvailableAsync()
        {
            try
            {
                var result = await GetAsync<dynamic>("health");
                return result != null;
            }
            catch
            {
                return false;
            }
        }
    }
}