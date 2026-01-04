using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CrunchyRolls.Core.Services;
using CrunchyRolls.Models.Entities;
using CrunchyRolls.Models.Enums;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace CrunchyRolls.Core.ViewModels
{
    /// <summary>
    /// OrderHistoryViewModel - Toont alle bestellingen van een klant
    /// ✅ Refactored naar CommunityToolkit.Mvvm
    /// </summary>
    [QueryProperty(nameof(Email), "email")]
    public partial class OrderHistoryViewModel : BaseViewModel
    {
        private readonly HybridOrderService _orderService;

        [ObservableProperty]
        private ObservableCollection<Order> orders = new();

        [ObservableProperty]
        private Order? selectedOrder;

        [ObservableProperty]
        private int totalOrders;

        [ObservableProperty]
        private decimal totalSpent;

        [ObservableProperty]
        private string customerEmail = string.Empty;

        [ObservableProperty]
        private string email = string.Empty;

        public bool HasOrders => Orders.Any();

        public OrderHistoryViewModel(HybridOrderService orderService)
        {
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
            Title = "Bestellingen";

            Debug.WriteLine("📋 OrderHistoryViewModel initialized");
        }

        // ===== PROPERTY CHANGED HANDLING =====

        partial void OnEmailChanged(string value)
        {
            // Wanneer email ingesteld wordt via query parameter, set deze in CustomerEmail
            if (!string.IsNullOrWhiteSpace(value))
            {
                Debug.WriteLine($"📧 Email ontvangen via query parameter: {value}");
                SetCustomerEmail(value);
            }
        }

        // ===== COMMANDS =====

        [RelayCommand]
        public async Task LoadOrdersAsync()
        {
            if (IsBusy)
                return;

            if (string.IsNullOrWhiteSpace(CustomerEmail))
            {
                Debug.WriteLine("⚠️ Attempted to load orders without customer email");
                await ShowAlert(
                    "Fout",
                    "Geen klant-email ingesteld. Meld je aan om je bestellingen te zien.",
                    "OK");
                return;
            }

            try
            {
                IsBusy = true;
                Debug.WriteLine($"📋 Loading orders for {CustomerEmail}...");

                var orders = await _orderService.GetOrderHistoryAsync(CustomerEmail);

                Orders.Clear();
                foreach (var order in orders)
                {
                    Orders.Add(order);
                }

                TotalOrders = orders.Count;
                TotalSpent = orders.Sum(o => o.TotalAmount);

                OnPropertyChanged(nameof(HasOrders));

                Debug.WriteLine($"✅ Loaded {orders.Count} orders for {CustomerEmail}");

                if (!orders.Any())
                {
                    Debug.WriteLine("ℹ️ No orders found for customer");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error loading orders for {CustomerEmail}: {ex.Message}");
                await ShowAlert(
                    "Fout",
                    $"Kon bestellingen niet laden: {ex.Message}",
                    "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async void OnOrderTapped(Order? order)
        {
            if (order == null)
                return;

            try
            {
                var fullOrder = await _orderService.GetOrderByIdAsync(order.Id);

                if (fullOrder == null)
                    fullOrder = order;

                var itemsText = string.Join("\n", fullOrder.OrderItems.Select(i =>
                    $"• {i.Product?.Name ?? $"Product #{i.ProductId}"} x{i.Quantity} - €{i.SubTotal:F2}"));

                await ShowAlert(
                    $"Bestelling #{fullOrder.Id}",
                    $"Datum: {fullOrder.OrderDate:dd/MM/yyyy HH:mm}\n" +
                    $"Status: {GetStatusText(fullOrder.Status)}\n" +
                    $"Totaal: €{fullOrder.TotalAmount:F2}\n\n" +
                    $"Items:\n{itemsText}\n\n" +
                    $"Bezorgadres:\n{fullOrder.DeliveryAddress}",
                    "Sluiten");

                Debug.WriteLine($"✅ Showed details for order #{order.Id}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error showing order details for #{order.Id}: {ex.Message}");
                await ShowAlert(
                    "Fout",
                    "Kon orderdetails niet laden",
                    "OK");
            }
        }

        [RelayCommand]
        private async Task OnCancelOrderAsync(Order? order)
        {
            if (order == null || order.Status == OrderStatus.Delivered || order.Status == OrderStatus.Cancelled)
                return;

            try
            {
                bool confirm = await ShowConfirmation(
                    "Bestelling verwijderen",
                    $"Weet je zeker dat je bestelling #{order.Id} wilt verwijderen? Dit kan niet ongedaan gemaakt worden.",
                    "Ja, verwijderen",
                    "Nee");

                if (confirm)
                {
                    IsBusy = true;
                    Debug.WriteLine($"🗑️ Deleting order #{order.Id}...");

                    var success = await _orderService.DeleteOrderAsync(order.Id);

                    if (success)
                    {
                        await ShowAlert(
                            "Verwijderd",
                            $"Bestelling #{order.Id} is verwijderd.",
                            "OK");

                        await LoadOrdersAsync();

                        Debug.WriteLine($"✅ Order #{order.Id} deleted successfully");
                    }
                    else
                    {
                        await ShowAlert(
                            "Fout",
                            "Kon bestelling niet verwijderen. Probeer later opnieuw.",
                            "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error deleting order #{order?.Id}: {ex.Message}");
                await ShowAlert(
                    "Fout",
                    $"Error: {ex.Message}",
                    "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task OnRefreshAsync()
        {
            await LoadOrdersAsync();
        }

        // ===== PUBLIC METHODS =====

        /// <summary>
        /// Set customer email (moet ingesteld worden voor order loading)
        /// </summary>
        public void SetCustomerEmail(string email)
        {
            if (!string.IsNullOrWhiteSpace(email))
            {
                CustomerEmail = email;
                Debug.WriteLine($"📧 Customer email set to: {email}");
            }
        }

        /// <summary>
        /// Called from OrderHistoryPage.xaml.cs OnAppearing
        /// </summary>
        public async Task OnAppearingAsync()
        {
            try
            {
                Debug.WriteLine("📱 OrderHistoryPage OnAppearing - loading orders");

                if (!string.IsNullOrWhiteSpace(CustomerEmail))
                {
                    await LoadOrdersAsync();
                }
                else
                {
                    Debug.WriteLine("⚠️ CustomerEmail not set yet");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error in OnAppearingAsync: {ex.Message}");
            }
        }

        // ===== PRIVATE METHODS =====

        private string GetStatusText(OrderStatus status)
        {
            return status switch
            {
                OrderStatus.Pending => "In behandeling",
                OrderStatus.Processing => "Wordt verwerkt",
                OrderStatus.Shipped => "Onderweg",
                OrderStatus.Delivered => "Afgeleverd",
                OrderStatus.Cancelled => "Geannuleerd",
                _ => status.ToString()
            };
        }

        public string GetStatusColor(OrderStatus status)
        {
            return status switch
            {
                OrderStatus.Pending => "#FFA500",
                OrderStatus.Processing => "#0000FF",
                OrderStatus.Shipped => "#800080",
                OrderStatus.Delivered => "#008000",
                OrderStatus.Cancelled => "#FF0000",
                _ => "#808080"
            };
        }

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