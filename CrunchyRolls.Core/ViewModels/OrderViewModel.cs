using CrunchyRolls.Models.Entities;
using CrunchyRolls.Core.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace CrunchyRolls.Core.ViewModels
{
    /// <summary>
    /// OrderViewModel - Shopping cart & order placement
    /// ✅ Refactored naar CommunityToolkit.Mvvm
    /// </summary>
    public partial class OrderViewModel : BaseViewModel
    {
        private readonly HybridOrderService _orderService;

        [ObservableProperty]
        private ObservableCollection<OrderItem> cartItems = new();

        [ObservableProperty]
        private string customerName = string.Empty;

        [ObservableProperty]
        private string customerEmail = string.Empty;

        [ObservableProperty]
        private string deliveryAddress = string.Empty;

        public decimal CartTotal => CartItems.Sum(item => item.SubTotal);
        public int CartItemCount => CartItems.Sum(item => item.Quantity);

        public OrderViewModel(HybridOrderService orderService)
        {
            _orderService = orderService;
            Title = "Winkelwagen";
        }

        // ===== COMMANDS =====

        [RelayCommand]
        public void LoadCart()
        {
            var items = _orderService.GetCartItems();

            CartItems.Clear();
            foreach (var item in items)
            {
                CartItems.Add(item);
            }

            OnPropertyChanged(nameof(CartTotal));
            OnPropertyChanged(nameof(CartItemCount));
        }

        [RelayCommand]
        private void OnRemoveItem(OrderItem? item)
        {
            if (item == null)
                return;

            _orderService.RemoveFromCart(item.ProductId);
            LoadCart();
        }

        [RelayCommand]
        private void OnUpdateQuantity((int productId, int quantity) data)
        {
            _orderService.UpdateQuantity(data.productId, data.quantity);
            LoadCart();
        }

        [RelayCommand]
        private async Task OnPlaceOrderAsync()
        {
            if (IsBusy)
                return;

            // Validatie
            if (!CartItems.Any())
            {
                await ShowAlert(
                    "Lege winkelwagen",
                    "Je winkelwagen is leeg",
                    "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(CustomerName) ||
                string.IsNullOrWhiteSpace(CustomerEmail) ||
                string.IsNullOrWhiteSpace(DeliveryAddress))
            {
                await ShowAlert(
                    "Vereiste velden",
                    "Vul alle velden in om je bestelling te plaatsen",
                    "OK");
                return;
            }

            try
            {
                IsBusy = true;

                // ✅ CORRECT - Only ID reference
                var orderItems = CartItems.Select(x => new OrderItem
                {
                    ProductId = x.ProductId,
                    Quantity = x.Quantity,
                    UnitPrice = x.UnitPrice
                }).ToList();

                var order = await _orderService.CreateOrderAsync(
                    CustomerName,
                    CustomerEmail,
                    DeliveryAddress,
                    orderItems);

                if (order != null)
                {
                    await ShowAlert(
                        "Bestelling geplaatst!",
                        $"Je bestelling #{order.Id} is succesvol geplaatst. Totaal: €{order.TotalAmount:F2}",
                        "OK");

                    // Store de email voor navigatie
                    var emailForNavigation = CustomerEmail;

                    // Reset form
                    CustomerName = string.Empty;
                    CustomerEmail = string.Empty;
                    DeliveryAddress = string.Empty;
                    LoadCart();

                    // ✅ FIXED - Navigate naar orders pagina met email parameter
                    Debug.WriteLine($"🎯 Navigating to //orders with email: {emailForNavigation}");
                    await Shell.Current.GoToAsync($"//orders?email={Uri.EscapeDataString(emailForNavigation)}");
                }
                else
                {
                    await ShowAlert(
                        "Fout",
                        "Er is iets misgegaan bij het plaatsen van je bestelling",
                        "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error placing order: {ex.Message}");
                await ShowAlert(
                    "Fout",
                    $"Er is een fout opgetreden: {ex.Message}",
                    "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async void OnClearCart()
        {
            bool confirm = await ShowConfirmation(
                "Winkelmand leegmaken",
                "Weet je zeker dat je de winkelwagen wilt leegmaken?",
                "Ja",
                "Nee");

            if (confirm)
            {
                _orderService.ClearCart();
                LoadCart();
            }
        }

        // ===== PRIVATE METHODS =====

        private static async Task ShowAlert(string title, string message, string cancel)
        {
            if (Shell.Current?.CurrentPage != null)
            {
                await Shell.Current.CurrentPage.DisplayAlert(title, message, cancel);
            }
        }

        private static async Task<bool> ShowConfirmation(string title, string message, string accept, string cancel)
        {
            if (Shell.Current?.CurrentPage != null)
            {
                return await Shell.Current.CurrentPage.DisplayAlert(title, message, accept, cancel);
            }
            return false;
        }
    }
}