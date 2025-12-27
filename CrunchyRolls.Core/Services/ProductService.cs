using CrunchyRolls.Models.Entities;
using System.Diagnostics;

namespace CrunchyRolls.Core.Services
{
    /// <summary>
    /// Service voor product en categorie operaties.
    /// Probeert eerst API te gebruiken, fallback op mock data als offline.
    /// </summary>
    public class ProductService
    {
        private readonly ApiService _apiService;

        // Cache voor offline mode
        private List<Category>? _cachedCategories;
        private List<Product>? _cachedProducts;
        private DateTime _lastApiCheck = DateTime.MinValue;

        public ProductService(ApiService apiService)
        {
            _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
            Debug.WriteLine("📦 ProductService initialized");
        }

        // ===== CATEGORIES =====

        /// <summary>
        /// Haal alle categorieën op van API, fallback op mock data
        /// </summary>
        public async Task<List<Category>> GetCategoriesAsync()
        {
            try
            {
                // Probeer van API
                var categories = await _apiService.GetAsync<List<Category>>("categories");

                if (categories != null && categories.Any())
                {
                    _cachedCategories = categories;
                    Debug.WriteLine($"✅ Loaded {categories.Count} categories from API");
                    return categories;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"⚠️ Failed to load categories from API: {ex.Message}");
            }

            // Fallback: gebruik cache of genereer mock data
            if (_cachedCategories != null)
            {
                Debug.WriteLine("💾 Using cached categories");
                return _cachedCategories;
            }

            Debug.WriteLine("🎭 Loading mock categories (offline mode)");
            return GetMockCategories();
        }

        // ===== PRODUCTS =====

        /// <summary>
        /// Haal alle producten op
        /// </summary>
        public async Task<List<Product>> GetProductsAsync()
        {
            try
            {
                var products = await _apiService.GetAsync<List<Product>>("products");

                if (products != null && products.Any())
                {
                    _cachedProducts = products;
                    Debug.WriteLine($"✅ Loaded {products.Count} products from API");
                    return products;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"⚠️ Failed to load products from API: {ex.Message}");
            }

            // Fallback
            if (_cachedProducts != null)
            {
                Debug.WriteLine("💾 Using cached products");
                return _cachedProducts;
            }

            Debug.WriteLine("🎭 Loading mock products (offline mode)");
            return GetMockProducts();
        }

        /// <summary>
        /// Haal producten voor specifieke categorie
        /// </summary>
        public async Task<List<Product>> GetProductsByCategoryAsync(int categoryId)
        {
            try
            {
                var products = await _apiService.GetAsync<List<Product>>($"products/category/{categoryId}");

                if (products != null && products.Any())
                {
                    Debug.WriteLine($"✅ Loaded {products.Count} products for category {categoryId} from API");
                    return products;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"⚠️ Failed to load products for category {categoryId}: {ex.Message}");
            }

            // Fallback
            var allProducts = _cachedProducts ?? GetMockProducts();
            return allProducts.Where(p => p.CategoryId == categoryId).ToList();
        }

        /// <summary>
        /// Haal specifiek product met categorie info
        /// </summary>
        public async Task<Product?> GetProductByIdAsync(int productId)
        {
            try
            {
                var product = await _apiService.GetAsync<Product>($"products/{productId}");

                if (product != null)
                {
                    Debug.WriteLine($"✅ Loaded product {productId} from API");
                    return product;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"⚠️ Failed to load product {productId}: {ex.Message}");
            }

            // Fallback
            var allProducts = _cachedProducts ?? GetMockProducts();
            return allProducts.FirstOrDefault(p => p.Id == productId);
        }

        /// <summary>
        /// Zoek producten op naam/beschrijving
        /// </summary>
        public async Task<List<Product>> SearchProductsAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return new List<Product>();

            try
            {
                var products = await _apiService.GetAsync<List<Product>>($"products/search?term={Uri.EscapeDataString(searchTerm)}");

                if (products != null && products.Any())
                {
                    Debug.WriteLine($"✅ Found {products.Count} products matching '{searchTerm}' from API");
                    return products;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"⚠️ Failed to search products for '{searchTerm}': {ex.Message}");
            }

            // Fallback: lokaal zoeken
            var allProducts = _cachedProducts ?? GetMockProducts();
            var lowerSearch = searchTerm.ToLower();
            return allProducts.Where(p =>
                p.Name.ToLower().Contains(lowerSearch) ||
                p.Description.ToLower().Contains(lowerSearch)).ToList();
        }

        /// <summary>
        /// Haal alleen beschikbare producten
        /// </summary>
        public async Task<List<Product>> GetInStockProductsAsync()
        {
            try
            {
                var products = await _apiService.GetAsync<List<Product>>("products/instock");

                if (products != null)
                {
                    Debug.WriteLine($"✅ Loaded {products.Count} in-stock products from API");
                    return products;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"⚠️ Failed to load in-stock products: {ex.Message}");
            }

            // Fallback
            var allProducts = _cachedProducts ?? GetMockProducts();
            return allProducts.Where(p => p.StockQuantity > 0).ToList();
        }

        // ===== MOCK DATA (Fallback) =====

        private List<Category> GetMockCategories()
        {
            return new List<Category>
            {
                new Category { Id = 1, Name = "Sushi", Description = "Verse sushi rollen en nigiri" },
                new Category { Id = 2, Name = "Ramen", Description = "Warme Japanse noedelsoepen" },
                new Category { Id = 3, Name = "Dranken", Description = "Frisdranken, thee en meer" },
                new Category { Id = 4, Name = "Desserts", Description = "Zoete Japanse lekkernijen" },
                new Category { Id = 5, Name = "Voorgerechten", Description = "Kleine hapjes en starters" }
            };
        }

        private List<Product> GetMockProducts()
        {
            return new List<Product>
            {
                // === Sushi (Categorie 1) - 5 producten ===
                new Product { Id = 1, Name = "California Roll", Description = "Krab, avocado en komkommer", Price = 8.50m, CategoryId = 1, StockQuantity = 15, ImageUrl = "california_roll.jpg" },
                new Product { Id = 2, Name = "Salmon Nigiri", Description = "Verse zalm op rijst", Price = 6.75m, CategoryId = 1, StockQuantity = 20, ImageUrl = "salmon_nigiri.jpg" },
                new Product { Id = 3, Name = "Tuna Roll", Description = "Verse tonijn roll", Price = 9.00m, CategoryId = 1, StockQuantity = 0, ImageUrl = "tuna_roll.jpg" },
                new Product { Id = 4, Name = "Dragon Roll", Description = "Garnaal tempura met avocado", Price = 12.50m, CategoryId = 1, StockQuantity = 8, ImageUrl = "dragon_roll.jpg" },
                new Product { Id = 5, Name = "Rainbow Roll", Description = "Gemixte vis met avocado", Price = 14.00m, CategoryId = 1, StockQuantity = 10, ImageUrl = "rainbow_roll.jpg" },
                
                // === Ramen (Categorie 2) - 5 producten ===
                new Product { Id = 6, Name = "Shoyu Ramen", Description = "Klassieke soja saus ramen", Price = 12.50m, CategoryId = 2, StockQuantity = 10, ImageUrl = "shoyu_ramen.jpg" },
                new Product { Id = 7, Name = "Miso Ramen", Description = "Rijke miso bouillon", Price = 13.00m, CategoryId = 2, StockQuantity = 8, ImageUrl = "miso_ramen.jpg" },
                new Product { Id = 8, Name = "Tonkotsu Ramen", Description = "Romige varkensbouillon", Price = 14.50m, CategoryId = 2, StockQuantity = 12, ImageUrl = "tonkotsu_ramen.jpg" },
                new Product { Id = 9, Name = "Spicy Ramen", Description = "Pittige ramen met kimchi", Price = 13.50m, CategoryId = 2, StockQuantity = 0, ImageUrl = "spicy_ramen.jpg" },
                new Product { Id = 10, Name = "Vegetarische Ramen", Description = "Groentebouillon met tofu", Price = 11.50m, CategoryId = 2, StockQuantity = 15, ImageUrl = "veggie_ramen.jpg" },
                
                // === Dranken (Categorie 3) - 5 producten ===
                new Product { Id = 11, Name = "Groene Thee", Description = "Traditionele Japanse groene thee", Price = 2.50m, CategoryId = 3, StockQuantity = 30, ImageUrl = "green_tea.jpg" },
                new Product { Id = 12, Name = "Ramune", Description = "Japanse frisdrank", Price = 3.00m, CategoryId = 3, StockQuantity = 25, ImageUrl = "ramune.jpg" },
                new Product { Id = 13, Name = "Sake", Description = "Traditionele rijstwijn", Price = 8.50m, CategoryId = 3, StockQuantity = 18, ImageUrl = "sake.jpg" },
                new Product { Id = 14, Name = "Matcha Latte", Description = "Groene thee latte", Price = 4.50m, CategoryId = 3, StockQuantity = 20, ImageUrl = "matcha_latte.jpg" },
                new Product { Id = 15, Name = "Yuzu Limonade", Description = "Verfrissende citruslimonade", Price = 3.50m, CategoryId = 3, StockQuantity = 22, ImageUrl = "yuzu_lemonade.jpg" },
                
                // === Desserts (Categorie 4) - 5 producten ===
                new Product { Id = 16, Name = "Mochi", Description = "Zoete rijstcake met vulling", Price = 4.50m, CategoryId = 4, StockQuantity = 12, ImageUrl = "mochi.jpg" },
                new Product { Id = 17, Name = "Dorayaki", Description = "Pannenkoek met rode bonen pasta", Price = 3.75m, CategoryId = 4, StockQuantity = 18, ImageUrl = "dorayaki.jpg" },
                new Product { Id = 18, Name = "Taiyaki", Description = "Vis-vormige wafel met vulling", Price = 4.00m, CategoryId = 4, StockQuantity = 14, ImageUrl = "taiyaki.jpg" },
                new Product { Id = 19, Name = "Matcha Ice Cream", Description = "Groene thee ijs", Price = 5.50m, CategoryId = 4, StockQuantity = 0, ImageUrl = "matcha_ice_cream.jpg" },
                new Product { Id = 20, Name = "Anmitsu", Description = "Gelei dessert met fruit", Price = 6.00m, CategoryId = 4, StockQuantity = 10, ImageUrl = "anmitsu.jpg" },
                
                // === Voorgerechten (Categorie 5) - 5 producten ===
                new Product { Id = 21, Name = "Edamame", Description = "Gestoomde sojabonen met zeezout", Price = 4.00m, CategoryId = 5, StockQuantity = 25, ImageUrl = "edamame.jpg" },
                new Product { Id = 22, Name = "Gyoza", Description = "Gebakken dumplings (6 stuks)", Price = 6.50m, CategoryId = 5, StockQuantity = 20, ImageUrl = "gyoza.jpg" },
                new Product { Id = 23, Name = "Takoyaki", Description = "Octopus balletjes (6 stuks)", Price = 7.00m, CategoryId = 5, StockQuantity = 15, ImageUrl = "takoyaki.jpg" },
                new Product { Id = 24, Name = "Tempura Mix", Description = "Gefrituurde groenten en garnalen", Price = 8.50m, CategoryId = 5, StockQuantity = 0, ImageUrl = "tempura.jpg" },
                new Product { Id = 25, Name = "Yakitori", Description = "Gegrilde kip spiesjes (3 stuks)", Price = 7.50m, CategoryId = 5, StockQuantity = 18, ImageUrl = "yakitori.jpg" }
            };
        }
    }
}