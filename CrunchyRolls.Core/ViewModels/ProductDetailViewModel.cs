using CrunchyRolls.Models.Entities;
using CrunchyRolls.Core.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CrunchyRolls.Core.ViewModels
{
    /// <summary>
    /// ProductDetailViewModel - Product detail pagina
    /// ✅ Refactored naar CommunityToolkit.Mvvm
    /// </summary>
    [QueryProperty(nameof(Product), "Product")]
    public partial class ProductDetailViewModel : BaseViewModel
    {
        private readonly HybridOrderService _orderService;

        [ObservableProperty]
        private Product? product;

        [ObservableProperty]
        private int quantity = 1;

        public ProductDetailViewModel(HybridOrderService orderService)
        {
            _orderService = orderService;
        }

        // ===== PROPERTY CHANGED HANDLING =====

        partial void OnProductChanged(Product? value)
        {
            Title = value?.Name ?? "Product Details";
        }

        // ===== COMMANDS =====

        [RelayCommand]
        private void OnIncreaseQuantity()
        {
            if (Product != null && Quantity < Product.StockQuantity)
            {
                Quantity++;
            }
        }

        [RelayCommand]
        private void OnDecreaseQuantity()
        {
            if (Quantity > 1)
            {
                Quantity--;
            }
        }

        [RelayCommand]
        private async void OnAddToCart()
        {
            if (Product == null || !Product.IsInStock)
                return;

            _orderService.AddToCart(Product, Quantity);

            await ShowAlert(
                "Toegevoegd",
                $"{Quantity}x {Product.Name} toegevoegd aan winkelwagen",
                "OK");

            await Shell.Current.GoToAsync("//producten");
        }

        // ===== PRIVATE METHODS =====

        private static async Task ShowAlert(string title, string message, string cancel)
        {
            if (Shell.Current?.CurrentPage != null)
            {
                await Shell.Current.CurrentPage.DisplayAlert(title, message, cancel);
            }
        }
    }
}