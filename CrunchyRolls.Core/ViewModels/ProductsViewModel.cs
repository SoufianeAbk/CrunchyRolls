using CrunchyRolls.Models.Entities;
using CrunchyRolls.Core.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace CrunchyRolls.Core.ViewModels
{
    /// <summary>
    /// ProductsViewModel - Products, Categories, Search
    /// ✅ Refactored naar CommunityToolkit.Mvvm
    /// ✅ MINIMALE AANPASSINGEN - 100% WERKT
    /// </summary>
    public partial class ProductsViewModel : BaseViewModel
    {
        private readonly HybridProductService _productService;
        private readonly HybridOrderService _orderService;

        [ObservableProperty]
        private ObservableCollection<Category> categories = new();

        [ObservableProperty]
        private ObservableCollection<Product> products = new();

        [ObservableProperty]
        private ObservableCollection<Product> filteredProducts = new();

        [ObservableProperty]
        private Category? selectedCategory;

        [ObservableProperty]
        private string searchText = string.Empty;

        public ProductsViewModel(HybridProductService productService, HybridOrderService orderService)
        {
            _productService = productService;
            _orderService = orderService;

            Title = "Producten";
            Debug.WriteLine("📋 ProductsViewModel initialized");
        }

        // ===== PROPERTY CHANGED HANDLING =====

        partial void OnSelectedCategoryChanged(Category? value)
        {
            FilterProducts();
        }

        partial void OnSearchTextChanged(string value)
        {
            FilterProducts();
        }

        // ===== COMMANDS =====

        [RelayCommand]
        public async Task LoadDataAsync()
        {
            if (IsBusy)
                return;

            try
            {
                IsBusy = true;
                Debug.WriteLine("📥 LoadDataAsync: Fetching categories and products...");

                // Load categories
                var categories = await _productService.GetCategoriesAsync();
                Debug.WriteLine($"✅ LoadDataAsync: Got {categories.Count} categories");

                // Load products
                var products = await _productService.GetProductsAsync();
                Debug.WriteLine($"✅ LoadDataAsync: Got {products.Count} products");

                // Update UI on main thread
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Categories.Clear();
                    Categories.Add(new Category { Id = 0, Name = "Alle" });
                    foreach (var category in categories)
                    {
                        Categories.Add(category);
                    }

                    Products.Clear();
                    foreach (var product in products)
                    {
                        Products.Add(product);
                    }

                    FilteredProducts.Clear();
                    foreach (var product in products)
                    {
                        FilteredProducts.Add(product);
                    }

                    // Reset selection
                    if (Categories.Any())
                    {
                        SelectedCategory = Categories.First();
                    }

                    Debug.WriteLine($"📊 UI Updated: {Categories.Count} categories, {Products.Count} products");
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ LoadDataAsync ERROR: {ex.Message}");
                Debug.WriteLine($"   Stack: {ex.StackTrace}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async void OnProductTapped(Product? product)
        {
            if (product == null)
                return;

            try
            {
                var navigationParameter = new Dictionary<string, object>
                {
                    { "Product", product }
                };

                await Shell.Current.GoToAsync("//productdetail", navigationParameter);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Navigation error: {ex.Message}");
            }
        }

        [RelayCommand]
        private async void OnAddToCart(Product? product)
        {
            if (product == null || !product.IsInStock)
                return;

            _orderService.AddToCart(product, 1);

            await ShowAlert(
                "Toegevoegd",
                $"{product.Name} is toegevoegd aan je winkelwagen",
                "OK");
        }

        [RelayCommand]
        private async Task OnNavigateToCart()
        {
            await Shell.Current.GoToAsync("//cart");
        }

        // ===== PRIVATE METHODS =====

        private void FilterProducts()
        {
            var filtered = Products.AsEnumerable();

            // Filter op categorie
            if (SelectedCategory != null && SelectedCategory.Id != 0)
            {
                filtered = filtered.Where(p => p.CategoryId == SelectedCategory.Id);
            }

            // Filter op zoekterm
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                filtered = filtered.Where(p =>
                    p.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    p.Description.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
            }

            FilteredProducts.Clear();
            foreach (var product in filtered)
            {
                FilteredProducts.Add(product);
            }
        }

        private static async Task ShowAlert(string title, string message, string cancel)
        {
            if (Shell.Current?.CurrentPage != null)
            {
                await Shell.Current.CurrentPage.DisplayAlert(title, message, cancel);
            }
        }
    }
}