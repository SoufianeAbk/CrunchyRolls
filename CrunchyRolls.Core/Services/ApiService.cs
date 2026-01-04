using System.Diagnostics;
using System.Text.Json;
using System.Net;

namespace CrunchyRolls.Core.Services
{
    /// <summary>
    /// API service voor communicatie met backend
    /// Inclusief JWT token authenticatie
    /// 🔧 FIXED: Localhost address, proper HTTP client config, better error handling
    /// </summary>
    public class ApiService
    {
        private readonly HttpClient _httpClient;

        // JWT token voor authenticatie
        private string? _authToken;

        // ✅ FIXED: Gebruik localhost in plaats van 127.0.0.1
        // Op Windows kunnen deze anders werken met firewall/binding
        private const string BaseUrl = "http://localhost:5291/api";

        public ApiService()
        {
            // ✅ FIXED: Proper HttpClientHandler configuration
            var handler = new HttpClientHandler();

            // ✅ FIXED: Only disable SSL validation in DEBUG mode
#if DEBUG
            handler.ServerCertificateCustomValidationCallback =
                (message, cert, chain, errors) => true;
            Debug.WriteLine("⚠️  SSL certificate validation DISABLED (DEBUG mode only)");
#endif

            _httpClient = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(10)  // Korter timeout
            };

            // Zorg dat SingleHttpClientInstancePerHost enabled is
            ServicePointManager.DefaultConnectionLimit = 10;

            Debug.WriteLine("📡 ApiService geïnitialiseerd");
            Debug.WriteLine($"📡 BaseUrl: {BaseUrl}");
            Debug.WriteLine($"📡 Timeout: {_httpClient.Timeout.TotalSeconds}s");
            Debug.WriteLine($"📡 Connection Pool Size: {ServicePointManager.DefaultConnectionLimit}");
        }

        // Helper to safely join base URL and endpoint
        private static string BuildUrl(string endpoint)
        {
            return $"{BaseUrl.TrimEnd('/')}/{endpoint.TrimStart('/')}";
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
                _httpClient.DefaultRequestHeaders.Remove("Authorization");
                Debug.WriteLine("🔓 Autorisatie header verwijderd");
            }
            else
            {
                _httpClient.DefaultRequestHeaders.Remove("Authorization");
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
                Debug.WriteLine("🔒 Autorisatie header ingesteld");
            }
        }

        // ===== GET REQUESTS =====

        public async Task<T?> GetAsync<T>(string endpoint)
        {
            try
            {
                var url = BuildUrl(endpoint);
                Debug.WriteLine($"📥 GET: {endpoint}");
                Debug.WriteLine($"📥 Full URL: {url}");

                using (var response = await _httpClient.GetAsync(url))
                {
                    // ✅ FIXED: Better status code handling
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        Debug.WriteLine("❌ Niet geautoriseerd (401)");
                        throw new UnauthorizedAccessException("API authenticatie mislukt - log opnieuw in");
                    }

                    // ✅ FIXED: Log response status
                    Debug.WriteLine($"📥 Response Status: {response.StatusCode}");

                    if (!response.IsSuccessStatusCode)
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        Debug.WriteLine($"❌ GET fout: {response.StatusCode} - {response.ReasonPhrase}");
                        Debug.WriteLine($"❌ Response content: {errorContent}");
                        return default;
                    }

                    var content = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"✅ GET success, content length: {content.Length}");

                    return JsonSerializer.Deserialize<T>(
                        content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"❌ HTTP Request Exception ({endpoint}): {ex.Message}");
                Debug.WriteLine($"❌ Inner Exception: {ex.InnerException?.Message}");
                return default;
            }
            catch (TaskCanceledException ex)
            {
                Debug.WriteLine($"❌ Request Timeout ({endpoint}): {ex.Message}");
                Debug.WriteLine($"❌ API niet bereikbaar binnen {_httpClient.Timeout.TotalSeconds}s");
                return default;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ GET Exception ({endpoint}): {ex.GetType().Name}");
                Debug.WriteLine($"❌ Message: {ex.Message}");
                Debug.WriteLine($"❌ StackTrace: {ex.StackTrace}");
                return default;
            }
        }

        // ===== POST REQUESTS =====

        public async Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest request)
        {
            try
            {
                var url = BuildUrl(endpoint);
                Debug.WriteLine($"📤 POST: {endpoint}");
                Debug.WriteLine($"📤 Full URL: {url}");

                var jsonContent = JsonSerializer.Serialize(request);
                Debug.WriteLine($"📤 Request body: {jsonContent.Substring(0, Math.Min(200, jsonContent.Length))}...");

                var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

                using (var response = await _httpClient.PostAsync(url, content))
                {
                    Debug.WriteLine($"📤 Response Status: {response.StatusCode}");

                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        Debug.WriteLine("❌ Niet geautoriseerd (401)");
                        throw new UnauthorizedAccessException("API authenticatie mislukt - log opnieuw in");
                    }

                    if (!response.IsSuccessStatusCode)
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        Debug.WriteLine($"❌ POST fout: {response.StatusCode} - {response.ReasonPhrase}");
                        Debug.WriteLine($"❌ Response content: {errorContent}");
                        return default;
                    }

                    var responseContent = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"✅ POST success, content length: {responseContent.Length}");

                    return JsonSerializer.Deserialize<TResponse>(
                        responseContent,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"❌ HTTP Request Exception ({endpoint}): {ex.Message}");
                Debug.WriteLine($"❌ Inner Exception: {ex.InnerException?.Message}");
                return default;
            }
            catch (TaskCanceledException ex)
            {
                Debug.WriteLine($"❌ Request Timeout ({endpoint}): {ex.Message}");
                return default;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ POST Exception ({endpoint}): {ex.GetType().Name}");
                Debug.WriteLine($"❌ Message: {ex.Message}");
                return default;
            }
        }

        // ===== PUT REQUESTS =====

        public async Task<bool> PutAsync<T>(string endpoint, T request)
        {
            try
            {
                var url = BuildUrl(endpoint);
                Debug.WriteLine($"📝 PUT: {endpoint}");

                var jsonContent = JsonSerializer.Serialize(request);
                var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

                using (var response = await _httpClient.PutAsync(url, content))
                {
                    Debug.WriteLine($"📝 Response Status: {response.StatusCode}");

                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        throw new UnauthorizedAccessException("API authenticatie mislukt - log opnieuw in");

                    if (!response.IsSuccessStatusCode)
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        Debug.WriteLine($"❌ PUT fout: {response.StatusCode} - {response.ReasonPhrase}");
                        Debug.WriteLine($"❌ Response content: {errorContent}");
                        return false;
                    }

                    Debug.WriteLine($"✅ PUT success");
                    return true;
                }
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"❌ HTTP Request Exception ({endpoint}): {ex.Message}");
                return false;
            }
            catch (TaskCanceledException ex)
            {
                Debug.WriteLine($"❌ Request Timeout ({endpoint}): {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ PUT Exception ({endpoint}): {ex.GetType().Name} - {ex.Message}");
                return false;
            }
        }

        // ===== DELETE REQUESTS =====

        public async Task<bool> DeleteAsync(string endpoint)
        {
            try
            {
                var url = BuildUrl(endpoint);
                Debug.WriteLine($"🗑️ DELETE: {endpoint}");

                using (var response = await _httpClient.DeleteAsync(url))
                {
                    Debug.WriteLine($"🗑️ Response Status: {response.StatusCode}");

                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        throw new UnauthorizedAccessException("API authenticatie mislukt - log opnieuw in");

                    if (!response.IsSuccessStatusCode)
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        Debug.WriteLine($"❌ DELETE fout: {response.StatusCode}");
                        return false;
                    }

                    Debug.WriteLine($"✅ DELETE success");
                    return true;
                }
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"❌ HTTP Request Exception ({endpoint}): {ex.Message}");
                return false;
            }
            catch (TaskCanceledException ex)
            {
                Debug.WriteLine($"❌ Request Timeout ({endpoint}): {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ DELETE Exception ({endpoint}): {ex.GetType().Name}");
                return false;
            }
        }

        // ===== HULPMETHODES =====

        public async Task<bool> IsApiAvailableAsync()
        {
            try
            {
                var result = await GetAsync<dynamic>("/health");
                return result != null;
            }
            catch
            {
                return false;
            }
        }
    }
}