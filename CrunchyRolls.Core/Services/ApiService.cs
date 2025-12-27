using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;

namespace CrunchyRolls.Core.Services
{
    /// <summary>
    /// Centralized API service voor alle HTTP communicatie met de backend.
    /// Voorziet error handling, timeout management, en logging.
    /// </summary>
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public ApiService()
        {
            // ===== CONFIGURATIE =====
            // In appsettings.json of hier configureren
#if DEBUG
            _baseUrl = "http://localhost:5000/api";  // Local API voor development
#else
            _baseUrl = "https://your-production-api.com/api";
#endif

            // ===== HTTPCLIENT SETUP =====
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            _httpClient.Timeout = TimeSpan.FromSeconds(30);

            Debug.WriteLine($"🌐 ApiService initialized with base URL: {_baseUrl}");
        }

        // ===== GET REQUESTS =====

        /// <summary>
        /// GET request met type conversion
        /// </summary>
        public async Task<T?> GetAsync<T>(string endpoint)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(endpoint))
                    throw new ArgumentException("Endpoint cannot be empty", nameof(endpoint));

                var url = $"{_baseUrl}/{endpoint}";
                Debug.WriteLine($"📥 GET Request: {url}");

                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"❌ GET Request failed: {response.StatusCode} - {errorContent}");

                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        throw new UnauthorizedAccessException("API authentication failed");

                    return default;
                }

                var result = await response.Content.ReadFromJsonAsync<T>();
                Debug.WriteLine($"✅ GET Request successful: {endpoint}");
                return result;
            }
            catch (TaskCanceledException ex)
            {
                Debug.WriteLine($"⏱️ GET Request timeout: {endpoint}");
                throw new Exception("Request timed out. Check your internet connection.", ex);
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"🔴 GET Request error: {endpoint} - {ex.Message}");
                throw new Exception("Network error occurred. Please check your connection.", ex);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"⚠️ Unexpected error in GetAsync: {endpoint} - {ex.Message}");
                return default;
            }
        }

        /// <summary>
        /// GET single entity by ID
        /// </summary>
        public async Task<T?> GetByIdAsync<T>(string endpoint, int id)
        {
            return await GetAsync<T>($"{endpoint}/{id}");
        }

        // ===== POST REQUESTS =====

        /// <summary>
        /// POST request met request/response bodies
        /// </summary>
        public async Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest data)
        {
            try
            {
                if (data == null)
                    throw new ArgumentNullException(nameof(data));

                var url = $"{_baseUrl}/{endpoint}";
                Debug.WriteLine($"📤 POST Request: {url}");

                var response = await _httpClient.PostAsJsonAsync(url, data);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"❌ POST Request failed: {response.StatusCode} - {errorContent}");

                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        throw new UnauthorizedAccessException("API authentication failed");

                    throw new Exception($"API Error: {response.StatusCode}");
                }

                var result = await response.Content.ReadFromJsonAsync<TResponse>();
                Debug.WriteLine($"✅ POST Request successful: {endpoint}");
                return result;
            }
            catch (TaskCanceledException ex)
            {
                Debug.WriteLine($"⏱️ POST Request timeout: {endpoint}");
                throw new Exception("Request timed out. Check your internet connection.", ex);
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"🔴 POST Request error: {endpoint} - {ex.Message}");
                throw new Exception("Network error occurred. Please check your connection.", ex);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"⚠️ Unexpected error in PostAsync: {endpoint} - {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// POST request zonder response body
        /// </summary>
        public async Task<bool> PostAsync<TRequest>(string endpoint, TRequest data)
        {
            try
            {
                if (data == null)
                    throw new ArgumentNullException(nameof(data));

                var url = $"{_baseUrl}/{endpoint}";
                Debug.WriteLine($"📤 POST Request: {url}");

                var response = await _httpClient.PostAsJsonAsync(url, data);
                var success = response.IsSuccessStatusCode;

                if (!success)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"❌ POST Request failed: {response.StatusCode} - {errorContent}");
                }
                else
                {
                    Debug.WriteLine($"✅ POST Request successful: {endpoint}");
                }

                return success;
            }
            catch (TaskCanceledException ex)
            {
                Debug.WriteLine($"⏱️ POST Request timeout: {endpoint}");
                throw new Exception("Request timed out. Check your internet connection.", ex);
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"🔴 POST Request error: {endpoint} - {ex.Message}");
                throw new Exception("Network error occurred. Please check your connection.", ex);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"⚠️ Unexpected error in PostAsync: {endpoint} - {ex.Message}");
                throw;
            }
        }

        // ===== PUT REQUESTS =====

        /// <summary>
        /// PUT request voor updates
        /// </summary>
        public async Task<bool> PutAsync<T>(string endpoint, T data)
        {
            try
            {
                if (data == null)
                    throw new ArgumentNullException(nameof(data));

                var url = $"{_baseUrl}/{endpoint}";
                Debug.WriteLine($"📝 PUT Request: {url}");

                var response = await _httpClient.PutAsJsonAsync(url, data);
                var success = response.IsSuccessStatusCode;

                if (!success)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"❌ PUT Request failed: {response.StatusCode} - {errorContent}");
                }
                else
                {
                    Debug.WriteLine($"✅ PUT Request successful: {endpoint}");
                }

                return success;
            }
            catch (TaskCanceledException ex)
            {
                Debug.WriteLine($"⏱️ PUT Request timeout: {endpoint}");
                throw new Exception("Request timed out. Check your internet connection.", ex);
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"🔴 PUT Request error: {endpoint} - {ex.Message}");
                throw new Exception("Network error occurred. Please check your connection.", ex);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"⚠️ Unexpected error in PutAsync: {endpoint} - {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// PUT request met response
        /// </summary>
        public async Task<TResponse?> PutAsync<TRequest, TResponse>(string endpoint, TRequest data)
        {
            try
            {
                if (data == null)
                    throw new ArgumentNullException(nameof(data));

                var url = $"{_baseUrl}/{endpoint}";
                Debug.WriteLine($"📝 PUT Request: {url}");

                var response = await _httpClient.PutAsJsonAsync(url, data);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"❌ PUT Request failed: {response.StatusCode} - {errorContent}");
                    return default;
                }

                var result = await response.Content.ReadFromJsonAsync<TResponse>();
                Debug.WriteLine($"✅ PUT Request successful: {endpoint}");
                return result;
            }
            catch (TaskCanceledException ex)
            {
                Debug.WriteLine($"⏱️ PUT Request timeout: {endpoint}");
                throw new Exception("Request timed out. Check your internet connection.", ex);
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"🔴 PUT Request error: {endpoint} - {ex.Message}");
                throw new Exception("Network error occurred. Please check your connection.", ex);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"⚠️ Unexpected error in PutAsync: {endpoint} - {ex.Message}");
                throw;
            }
        }

        // ===== DELETE REQUESTS =====

        /// <summary>
        /// DELETE request
        /// </summary>
        public async Task<bool> DeleteAsync(string endpoint)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(endpoint))
                    throw new ArgumentException("Endpoint cannot be empty", nameof(endpoint));

                var url = $"{_baseUrl}/{endpoint}";
                Debug.WriteLine($"🗑️ DELETE Request: {url}");

                var response = await _httpClient.DeleteAsync(url);
                var success = response.IsSuccessStatusCode;

                if (!success)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"❌ DELETE Request failed: {response.StatusCode} - {errorContent}");
                }
                else
                {
                    Debug.WriteLine($"✅ DELETE Request successful: {endpoint}");
                }

                return success;
            }
            catch (TaskCanceledException ex)
            {
                Debug.WriteLine($"⏱️ DELETE Request timeout: {endpoint}");
                throw new Exception("Request timed out. Check your internet connection.", ex);
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"🔴 DELETE Request error: {endpoint} - {ex.Message}");
                throw new Exception("Network error occurred. Please check your connection.", ex);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"⚠️ Unexpected error in DeleteAsync: {endpoint} - {ex.Message}");
                throw;
            }
        }

        // ===== UTILITY METHODS =====

        /// <summary>
        /// Set Bearer token for authenticated requests
        /// </summary>
        public void SetAuthorizationToken(string? token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                _httpClient.DefaultRequestHeaders.Remove("Authorization");
                Debug.WriteLine("🔓 Authorization token removed");
            }
            else
            {
                _httpClient.DefaultRequestHeaders.Remove("Authorization");
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
                Debug.WriteLine("🔒 Authorization token set");
            }
        }

        /// <summary>
        /// Check if API is reachable
        /// </summary>
        public async Task<bool> IsApiReachableAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/categories");
                var isReachable = response.IsSuccessStatusCode;
                Debug.WriteLine(isReachable ? "✅ API is reachable" : "❌ API is not reachable");
                return isReachable;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ API check failed: {ex.Message}");
                return false;
            }
        }
    }
}