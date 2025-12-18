using CrunchyRolls.Core.Helpers;
using CrunchyRolls.Core.Models;
using CrunchyRolls.Core.Services;
using System.Windows.Input;
using static System.Net.Mime.MediaTypeNames;

namespace CrunchyRolls.Core.ViewModels
{
    [QueryProperty(nameof(Product), "Product")]
    public class ProductDetailViewModel : BaseViewModel
    {
        private readonly OrderService _orderService;

        private Product? _product;
        private int _quantity = 1;

        public Product? Product
        {
            get => _product;
            set
            {
                if (SetProperty(ref _product, value))
                {
                    Title = _product?.Name ?? "Product Details";
                }
            }
        }

        public int Quantity
        {
            get => _quantity;
            set => SetProperty(ref _quantity, Math.Max(1, value));
        }

        public ICommand IncreaseQuantityCommand { get; }
        public ICommand DecreaseQuantityCommand { get; }
        public ICommand AddToCartCommand { get; }

        public ProductDetailViewModel(OrderService orderService)
        {
            _orderService = orderService;

            IncreaseQuantityCommand = new Command(OnIncreaseQuantity);
            DecreaseQuantityCommand = new Command(OnDecreaseQuantity);
            AddToCartCommand = new Command(OnAddToCart);
        }

        private void OnIncreaseQuantity()
        {
            if (Product != null && Quantity < Product.StockQuantity)
            {
                Quantity++;
            }
        }

        private void OnDecreaseQuantity()
        {
            if (Quantity > 1)
            {
                Quantity--;
            }
        }

        private async void OnAddToCart()
        {
            if (Product == null || !Product.IsInStock)
                return;

            _orderService.AddToCart(Product, Quantity);

            await ShowAlert(
                "Toegevoegd",
                $"{Quantity}x {Product.Name} toegevoegd aan winkelwagen",
                "OK");

            // Ga terug naar producten pagina
            await Shell.Current.GoToAsync("..");
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