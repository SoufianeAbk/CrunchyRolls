using CrunchyRolls.Core.Data.Repositories;
using CrunchyRolls.Models.Entities;
using System.Diagnostics;

namespace CrunchyRolls.Core.Services
{
    /// <summary>
    /// Hybrid ProductService - API first, lokale fallback
    /// 
    /// Strategie:
    /// 1. Try API
    /// 2. Cache resultaat lokaal
    /// 3. Als API down: gebruik lokale cache
    /// </summary>
    public class HybridProductService
    {
        private readonly ApiService _apiService;
        private readonly ProductLocalRepository _productLocalRepo;
        private readonly CategoryLocalRepository _categoryLocalRepo;

        private DateTime _lastApiSync = DateTime.MinValue;
        private const int SyncIntervalMinutes = 60;

        public HybridProductService(ApiService apiService)
        {
            _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
            _productLocalRepo = new ProductLocalRepository();
            _categoryLocalRepo = new CategoryLocalRepository();

            Debug.WriteLine("🔄 HybridProductService initialized");
        }

        // ===== CATEGORIES =====

        /// <summary>
        /// Get categories: API -> Local Cache -> Mock
        /// </summary>
        public async Task<List<Category>> GetCategoriesAsync(bool forceRefresh = false)
        {
            try
            {
                // Check if we should refresh from API
                var shouldRefreshApi = forceRefresh ||
                    (DateTime.Now - _lastApiSync).TotalMinutes > SyncIntervalMinutes;

                if (shouldRefreshApi)
                {
                    try
                    {
                        Debug.WriteLine("📡 Fetching categories from API...");
                        var apiCategories = await _apiService.GetAsync<List<Category>>("categories");

                        if (apiCategories != null && apiCategories.Any())
                        {
                            // Update local cache
                            await _categoryLocalRepo.ClearAllAsync();
                            await _categoryLocalRepo.AddRangeAsync(apiCategories);
                            _lastApiSync = DateTime.Now;

                            Debug.WriteLine($"✅ Synced {apiCategories.Count} categories from API");
                            return apiCategories;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"⚠️ API fetch failed: {ex.Message} - using local cache");
                    }
                }

                // Fallback: use local cache
                var cachedCategories = await _categoryLocalRepo.GetAllAsync();
                if (cachedCategories.Any())
                {
                    Debug.WriteLine($"💾 Using {cachedCategories.Count()} categories from local cache");
                    return cachedCategories.ToList();
                }

                Debug.WriteLine("❌ No categories available");
                return new List<Category>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ GetCategoriesAsync error: {ex.Message}");
                return new List<Category>();
            }
        }

        // ===== PRODUCTS =====

        /// <summary>
        /// Get all products: API -> Local Cache
        /// </summary>
        public async Task<List<Product>> GetProductsAsync(bool forceRefresh = false)
        {
            try
            {
                var shouldRefreshApi = forceRefresh ||
                    (DateTime.Now - _lastApiSync).TotalMinutes > SyncIntervalMinutes;

                if (shouldRefreshApi)
                {
                    try
                    {
                        Debug.WriteLine("📡 Fetching products from API...");
                        var apiProducts = await _apiService.GetAsync<List<Product>>("products");

                        if (apiProducts != null && apiProducts.Any())
                        {
                            // Update local cache
                            await _productLocalRepo.ClearAllAsync();
                            await _productLocalRepo.AddRangeAsync(apiProducts);
                            _lastApiSync = DateTime.Now;

                            Debug.WriteLine($"✅ Synced {apiProducts.Count} products from API");
                            return apiProducts;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"⚠️ API fetch failed: {ex.Message} - using local cache");
                    }
                }

                // Fallback: use local cache
                var cachedProducts = await _productLocalRepo.GetAllAsync();
                if (cachedProducts.Any())
                {
                    Debug.WriteLine($"💾 Using {cachedProducts.Count()} products from local cache");
                    return cachedProducts.ToList();
                }

                return new List<Product>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ GetProductsAsync error: {ex.Message}");
                return new List<Product>();
            }
        }

        public async Task<List<Product>> GetProductsByCategoryAsync(int categoryId)
        {
            try
            {
                // Try API first for fresh data
                try
                {
                    var apiProducts = await _apiService.GetAsync<List<Product>>($"products/category/{categoryId}");
                    if (apiProducts != null)
                    {
                        Debug.WriteLine($"✅ Got {apiProducts.Count} products for category {categoryId} from API");
                        return apiProducts;
                    }
                }
                catch
                {
                    Debug.WriteLine($"⚠️ API fetch failed - using local cache");
                }

                // Fallback: use local cache
                var cached = await _productLocalRepo.GetByCategoryAsync(categoryId);
                return cached.ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ GetProductsByCategoryAsync error: {ex.Message}");
                return new List<Product>();
            }
        }

        public async Task<Product?> GetProductByIdAsync(int productId)
        {
            try
            {
                // Try API first
                try
                {
                    var apiProduct = await _apiService.GetAsync<Product>($"products/{productId}");
                    if (apiProduct != null)
                    {
                        Debug.WriteLine($"✅ Got product {productId} from API");
                        return apiProduct;
                    }
                }
                catch
                {
                    Debug.WriteLine($"⚠️ API fetch failed - using local cache");
                }

                // Fallback: use local cache
                return await _productLocalRepo.GetByIdAsync(productId);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ GetProductByIdAsync error: {ex.Message}");
                return null;
            }
        }

        public async Task<List<Product>> SearchProductsAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return new List<Product>();

            try
            {
                // Use local cache for search (faster)
                var results = await _productLocalRepo.SearchAsync(searchTerm);
                Debug.WriteLine($"🔍 Found {results.Count()} products matching '{searchTerm}'");
                return results.ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ SearchProductsAsync error: {ex.Message}");
                return new List<Product>();
            }
        }

        public async Task<List<Product>> GetInStockProductsAsync()
        {
            try
            {
                var inStock = await _productLocalRepo.GetInStockAsync();
                Debug.WriteLine($"📦 {inStock.Count()} products in stock");
                return inStock.ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ GetInStockProductsAsync error: {ex.Message}");
                return new List<Product>();
            }
        }

        /// <summary>
        /// Manually sync with API
        /// Useful for force-refresh
        /// </summary>
        public async Task SyncWithApiAsync()
        {
            Debug.WriteLine("🔄 Force syncing with API...");
            _lastApiSync = DateTime.MinValue; // Force refresh on next call

            await GetCategoriesAsync(forceRefresh: true);
            await GetProductsAsync(forceRefresh: true);

            Debug.WriteLine("✅ Sync completed");
        }

        public void Dispose()
        {
            _productLocalRepo?.Dispose();
            _categoryLocalRepo?.Dispose();
        }
    }
}