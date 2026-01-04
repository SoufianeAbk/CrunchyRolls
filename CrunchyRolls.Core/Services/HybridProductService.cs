using CrunchyRolls.Core.Data.Repositories;
using CrunchyRolls.Models.Entities;
using System.Diagnostics;

namespace CrunchyRolls.Core.Services
{
    
    public class HybridProductService
    {
        private readonly ApiService _apiService;
        private readonly ProductLocalRepository _productLocalRepo;
        private readonly CategoryLocalRepository _categoryLocalRepo;

        private DateTime _lastCategorySync = DateTime.MinValue;
        private DateTime _lastProductSync = DateTime.MinValue;
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
        /// Get categories: API -> Local Cache -> Empty
        /// </summary>
        public async Task<List<Category>> GetCategoriesAsync(bool forceRefresh = false)
        {
            try
            {
                Debug.WriteLine("📦 GetCategoriesAsync called");

                var shouldRefreshApi = forceRefresh ||
                    (DateTime.Now - _lastCategorySync).TotalMinutes > SyncIntervalMinutes;

                if (shouldRefreshApi)
                {
                    try
                    {
                        Debug.WriteLine("📡 Attempting to fetch categories from API...");
                        var apiCategories = await _apiService.GetAsync<List<Category>>("categories");

                        if (apiCategories != null && apiCategories.Any())
                        {
                            Debug.WriteLine($"✅ API SUCCESS: Got {apiCategories.Count} categories from API");

                            await _categoryLocalRepo.ClearAllAsync();
                            await _categoryLocalRepo.AddRangeAsync(apiCategories);
                            _lastCategorySync = DateTime.Now;

                            return apiCategories;
                        }
                        else
                        {
                            Debug.WriteLine("⚠️ API returned empty list for categories");
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"❌ API FAILED: {ex.GetType().Name} - {ex.Message}");
                    }
                }

                Debug.WriteLine("💾 Falling back to local cache for categories...");
                var cachedCategories = await _categoryLocalRepo.GetAllAsync();

                if (cachedCategories.Any())
                {
                    Debug.WriteLine($"✅ LOCAL CACHE: Found {cachedCategories.Count()} cached categories");
                    return cachedCategories.ToList();
                }

                Debug.WriteLine("❌ NO DATA: No categories found anywhere (API down, cache empty)");
                return new List<Category>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ CRITICAL ERROR in GetCategoriesAsync: {ex.Message}");
                return new List<Category>();
            }
        }

        // ===== PRODUCTS =====

        public async Task<List<Product>> GetProductsAsync(bool forceRefresh = false)
        {
            try
            {
                Debug.WriteLine("📦 GetProductsAsync called");

                var shouldRefreshApi = forceRefresh ||
                    (DateTime.Now - _lastProductSync).TotalMinutes > SyncIntervalMinutes;

                if (shouldRefreshApi)
                {
                    try
                    {
                        Debug.WriteLine("📡 Attempting to fetch products from API...");
                        var apiProducts = await _apiService.GetAsync<List<Product>>("Products");

                        if (apiProducts != null && apiProducts.Any())
                        {
                            Debug.WriteLine($"✅ API SUCCESS: Got {apiProducts.Count} products from API");

                            await _productLocalRepo.ClearAllAsync();
                            await _productLocalRepo.AddRangeAsync(apiProducts);
                            _lastProductSync = DateTime.Now;

                            return apiProducts;
                        }
                        else
                        {
                            Debug.WriteLine("⚠️ API returned empty list for products");
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"❌ API FAILED: {ex.GetType().Name} - {ex.Message}");
                    }
                }

                Debug.WriteLine("💾 Falling back to local cache for products...");
                var cachedProducts = await _productLocalRepo.GetAllAsync();

                if (cachedProducts.Any())
                {
                    Debug.WriteLine($"✅ LOCAL CACHE: Found {cachedProducts.Count()} cached products");
                    return cachedProducts.ToList();
                }

                Debug.WriteLine("❌ NO DATA: No products found anywhere (API down, cache empty)");
                return new List<Product>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ CRITICAL ERROR in GetProductsAsync: {ex.Message}");
                return new List<Product>();
            }
        }

        public async Task<List<Product>> GetProductsByCategoryAsync(int categoryId)
        {
            try
            {
                Debug.WriteLine($"📦 GetProductsByCategoryAsync({categoryId}) called");

                // Try API first for fresh data
                try
                {
                    Debug.WriteLine($"📡 Fetching products for category {categoryId} from API...");
                    var apiProducts = await _apiService.GetAsync<List<Product>>($"Products/category/{categoryId}");

                    if (apiProducts != null && apiProducts.Any())
                    {
                        Debug.WriteLine($"✅ API SUCCESS: Got {apiProducts.Count} products for category {categoryId}");
                        return apiProducts;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"⚠️ API fetch failed: {ex.Message} - using local cache");
                }

                // Fallback: use local cache
                var cached = await _productLocalRepo.GetByCategoryAsync(categoryId);
                Debug.WriteLine($"💾 LOCAL CACHE: Found {cached.Count()} products for category {categoryId}");
                return cached.ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error in GetProductsByCategoryAsync: {ex.Message}");
                return new List<Product>();
            }
        }

        public async Task<Product?> GetProductByIdAsync(int productId)
        {
            try
            {
                Debug.WriteLine($"📦 GetProductByIdAsync({productId}) called");

                // Try API first
                try
                {
                    Debug.WriteLine($"📡 Fetching product {productId} from API...");
                    var apiProduct = await _apiService.GetAsync<Product>($"Products/{productId}");

                    if (apiProduct != null)
                    {
                        Debug.WriteLine($"✅ API SUCCESS: Got product {productId}");
                        return apiProduct;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"⚠️ API fetch failed: {ex.Message} - using local cache");
                }

                // Fallback: use local cache
                var cached = await _productLocalRepo.GetByIdAsync(productId);
                Debug.WriteLine($"💾 LOCAL CACHE: Found product {productId} = {(cached != null ? "✓" : "✗")}");
                return cached;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error in GetProductByIdAsync: {ex.Message}");
                return null;
            }
        }

        public async Task<List<Product>> SearchProductsAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return new List<Product>();

            try
            {
                Debug.WriteLine($"🔍 Searching for '{searchTerm}'");
                var results = await _productLocalRepo.SearchAsync(searchTerm);
                Debug.WriteLine($"🔍 Found {results.Count()} products matching '{searchTerm}'");
                return results.ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error in SearchProductsAsync: {ex.Message}");
                return new List<Product>();
            }
        }

        public async Task<List<Product>> GetInStockProductsAsync()
        {
            try
            {
                Debug.WriteLine("📦 GetInStockProductsAsync called");
                var inStock = await _productLocalRepo.GetInStockAsync();
                Debug.WriteLine($"📦 In stock: {inStock.Count()} products");
                return inStock.ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error in GetInStockProductsAsync: {ex.Message}");
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
            _lastCategorySync = DateTime.MinValue;
            _lastProductSync = DateTime.MinValue;

            await GetCategoriesAsync(forceRefresh: true);
            await GetProductsAsync(forceRefresh: true);

            Debug.WriteLine("✅ Sync completed");
        }

        /// <summary>
        /// Diagnostiek: Check welke data beschikbaar is
        /// </summary>
        public async Task<string> GetDiagnosticsAsync()
        {
            try
            {
                var cachedProducts = await _productLocalRepo.GetAllAsync();
                var cachedCategories = await _categoryLocalRepo.GetAllAsync();

                return $"📊 DIAGNOSTICS:\n" +
                       $"  Cached Products: {cachedProducts.Count()}\n" +
                       $"  Cached Categories: {cachedCategories.Count()}\n" +
                       $"  Last Category Sync: {_lastCategorySync:g}\n" +
                       $"  Last Product Sync: {_lastProductSync:g}";
            }
            catch (Exception ex)
            {
                return $"❌ Diagnostics error: {ex.Message}";
            }
        }

        public void Dispose()
        {
            _productLocalRepo?.Dispose();
            _categoryLocalRepo?.Dispose();
        }
    }
}