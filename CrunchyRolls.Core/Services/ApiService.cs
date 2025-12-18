using System.Net.Http.Json;
using System.Text.Json;

namespace CrunchyRolls.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public ApiService()
        {
            _httpClient = new HttpClient();
            // Pas deze URL aan naar jouw API endpoint
            _baseUrl = "https://your-api-url.com/api";

            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        public async Task<T?> GetAsync<T>(string endpoint)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/{endpoint}");
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<T>();
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"Error in GetAsync: {ex.Message}");
                return default;
            }
        }

        public async Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest data)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/{endpoint}", data);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<TResponse>();
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"Error in PostAsync: {ex.Message}");
                return default;
            }
        }

        public async Task<bool> PutAsync<T>(string endpoint, T data)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"{_baseUrl}/{endpoint}", data);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"Error in PutAsync: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteAsync(string endpoint)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{_baseUrl}/{endpoint}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"Error in DeleteAsync: {ex.Message}");
                return false;
            }
        }
    }
}
