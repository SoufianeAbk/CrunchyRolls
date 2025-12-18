using CrunchyRolls.Helpers;
using CrunchyRolls.Models;
using CrunchyRolls.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;
using static System.Net.Mime.MediaTypeNames;

namespace CrunchyRolls.ViewModels
{
    public class OrderHistoryViewModel : BaseViewModel
    {
        private readonly OrderService _orderService;

        private ObservableCollection<Order> _orders = new();
        private Order? _selectedOrder;
        private int _totalOrders;
        private decimal _totalSpent;

        public ObservableCollection<Order> Orders
        {
            get => _orders;
            set => SetProperty(ref _orders, value);
        }

        public Order? SelectedOrder
        {
            get => _selectedOrder;
            set => SetProperty(ref _selectedOrder, value);
        }

        public int TotalOrders
        {
            get => _totalOrders;
            set => SetProperty(ref _totalOrders, value);
        }

        public decimal TotalSpent
        {
            get => _totalSpent;
            set => SetProperty(ref _totalSpent, value);
        }

        public bool HasOrders => Orders.Any();

        public ICommand LoadOrdersCommand { get; }
        public ICommand OrderTappedCommand { get; }
        public ICommand CancelOrderCommand { get; }
        public ICommand RefreshCommand { get; }

        public OrderHistoryViewModel(OrderService orderService)
        {
            _orderService = orderService;

            Title = "Bestelgeschiedenis";

            LoadOrdersCommand = new Command(async () => await LoadOrdersAsync());
            OrderTappedCommand = new Command<Order>(OnOrderTapped);
            CancelOrderCommand = new Command<Order>(async (order) => await OnCancelOrderAsync(order));
            RefreshCommand = new Command(async () => await LoadOrdersAsync());
        }

        public async Task LoadOrdersAsync()
        {
            if (IsBusy)
                return;

            try
            {
                IsBusy = true;

                var orders = await _orderService.GetOrderHistoryAsync();

                Orders.Clear();
                foreach (var order in orders)
                {
                    Orders.Add(order);
                }

                TotalOrders = _orderService.GetTotalOrdersCount();
                TotalSpent = _orderService.GetTotalSpent();

                OnPropertyChanged(nameof(HasOrders));
            }
            catch (Exception ex)
            {
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

        private async void OnOrderTapped(Order order)
        {
            if (order == null)
                return;

            // Toon order details
            var itemsText = string.Join("\n", order.OrderItems.Select(i =>
                $"• {i.Product?.Name ?? "Product"} x{i.Quantity} - €{i.SubTotal:F2}"));

            await ShowAlert(
                $"Bestelling #{order.Id}",
                $"Datum: {order.OrderDate:dd/MM/yyyy HH:mm}\n" +
                $"Status: {GetStatusText(order.Status)}\n" +
                $"Totaal: €{order.TotalAmount:F2}\n\n" +
                $"Items:\n{itemsText}\n\n" +
                $"Bezorgadres:\n{order.DeliveryAddress}",
                "Sluiten");
        }

        private async Task OnCancelOrderAsync(Order order)
        {
            if (order == null || order.Status == OrderStatus.Delivered || order.Status == OrderStatus.Cancelled)
                return;

            bool confirm = await ShowConfirmation(
                "Bestelling annuleren",
                $"Weet je zeker dat je bestelling #{order.Id} wilt annuleren?",
                "Ja, annuleren",
                "Nee");

            if (confirm)
            {
                var success = await _orderService.CancelOrderAsync(order.Id);

                if (success)
                {
                    await ShowAlert(
                        "Geannuleerd",
                        $"Bestelling #{order.Id} is geannuleerd.",
                        "OK");

                    await LoadOrdersAsync();
                }
                else
                {
                    await ShowAlert(
                        "Fout",
                        "Kon bestelling niet annuleren.",
                        "OK");
                }
            }
        }

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
                OrderStatus.Pending => "#FFA500",      // Orange
                OrderStatus.Processing => "#0000FF",   // Blue
                OrderStatus.Shipped => "#800080",      // Purple
                OrderStatus.Delivered => "#008000",    // Green
                OrderStatus.Cancelled => "#FF0000",    // Red
                _ => "#808080"                         // Gray
            };
        }

        // Helper methods for dialogs
        private async Task ShowAlert(string title, string message, string cancel)
        {
            var page = GetCurrentPage();
            if (page != null)
            {
                await page.DisplayAlert(title, message, cancel);
            }
        }

        private async Task<bool> ShowConfirmation(string title, string message, string accept, string cancel)
        {
            var page = GetCurrentPage();
            if (page != null)
            {
                return await page.DisplayAlert(title, message, accept, cancel);
            }
            return false;
        }

        private static Page? GetCurrentPage()
        {
            return Application.Current?.MainPage;
        }
    }
}