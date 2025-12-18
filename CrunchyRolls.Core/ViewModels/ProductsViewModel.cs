using CrunchyRolls.Core.Models;
using CrunchyRolls.Core.Helpers;
using CrunchyRolls.Core.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;
using static System.Net.Mime.MediaTypeNames;

namespace CrunchyRolls.Core.ViewModels
{
    public class ProductsViewModel : BaseViewModel
    {
        private readonly ProductService _productService;
        private readonly OrderService _orderService;

        private ObservableCollection<Category> _categories = new();
        private ObservableCollection<Product> _products = new();
        private ObservableCollection<Product> _filteredProducts = new();
        private Category? _selectedCategory;
        private string _searchText = string.Empty;

        public ObservableCollection<Category> Categories
        {
            get => _categories;
            set => SetProperty(ref _categories, value);
        }

        public ObservableCollection<Product> Products
        {
            get => _products;
            set => SetProperty(ref _products, value);
        }

        public ObservableCollection<Product> FilteredProducts
        {
            get => _filteredProducts;
            set => SetProperty(ref _filteredProducts, value);
        }

        public Category? SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (SetProperty(ref _selectedCategory, value))
                {
                    FilterProducts();
                }
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    FilterProducts();
                }
            }
        }

        public ICommand LoadDataCommand { get; }
        public ICommand ProductTappedCommand { get; }
        public ICommand AddToCartCommand { get; }

        public ProductsViewModel(ProductService productService, OrderService orderService)
        {
            _productService = productService;
            _orderService = orderService;

            Title = "Producten";

            LoadDataCommand = new Command(async () => await LoadDataAsync());
            ProductTappedCommand = new Command<Product>(OnProductTapped);
            AddToCartCommand = new Command<Product>(OnAddToCart);
        }

        public async Task LoadDataAsync()
        {
            if (IsBusy)
                return;

            try
            {
                IsBusy = true;

                var categories = await _productService.GetCategoriesAsync();
                var products = await _productService.GetProductsAsync();

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
            }
            catch (Exception ex)
            {
                // Handle error
                Console.WriteLine($"Error loading data: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

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

        private async void OnProductTapped(Product product)
        {
            if (product == null)
                return;

            // Navigate to product detail page
            var navigationParameter = new Dictionary<string, object>
            {
                { "Product", product }
            };

            await Shell.Current.GoToAsync($"ProductDetailPage", navigationParameter);
        }

        private async void OnAddToCart(Product product)
        {
            if (product == null || !product.IsInStock)
                return;

            _orderService.AddToCart(product, 1);

            // Toon feedback
            await ShowAlert(
                "Toegevoegd",
                $"{product.Name} is toegevoegd aan je winkelwagen",
                "OK");
        }

        // Helper method for dialog
        private async Task ShowAlert(string title, string message, string cancel)
        {
            var page = GetCurrentPage();
            if (page != null)
            {
                await page.DisplayAlert(title, message, cancel);
            }
        }

        private static Page? GetCurrentPage()
        {
            return Application.Current?.Windows.FirstOrDefault()?.Page ?? Application.Current?.MainPage;
        }
    }
}