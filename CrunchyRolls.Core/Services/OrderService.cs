using CrunchyRolls.Core.Models;

namespace CrunchyRolls.Core.Services
{
    public class OrderService
    {
        private readonly ApiService _apiService;
        private readonly List<OrderItem> _currentOrderItems = new();
        private readonly List<Order> _orderHistory = new(); // Lokale opslag voor bestelgeschiedenis

        public OrderService()
        {
            _apiService = new ApiService();
        }

        // Winkelwagen functionaliteit
        public void AddToCart(Product product, int quantity = 1)
        {
            var existingItem = _currentOrderItems.FirstOrDefault(i => i.ProductId == product.Id);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                _currentOrderItems.Add(new OrderItem
                {
                    ProductId = product.Id,
                    Product = product,
                    Quantity = quantity,
                    UnitPrice = product.Price
                });
            }
        }

        public void RemoveFromCart(int productId)
        {
            var item = _currentOrderItems.FirstOrDefault(i => i.ProductId == productId);
            if (item != null)
            {
                _currentOrderItems.Remove(item);
            }
        }

        public void UpdateQuantity(int productId, int quantity)
        {
            var item = _currentOrderItems.FirstOrDefault(i => i.ProductId == productId);
            if (item != null)
            {
                if (quantity <= 0)
                {
                    RemoveFromCart(productId);
                }
                else
                {
                    item.Quantity = quantity;
                }
            }
        }

        public List<OrderItem> GetCartItems()
        {
            return _currentOrderItems;
        }

        public void ClearCart()
        {
            _currentOrderItems.Clear();
        }

        public decimal GetCartTotal()
        {
            return _currentOrderItems.Sum(item => item.SubTotal);
        }

        public int GetCartItemCount()
        {
            return _currentOrderItems.Sum(item => item.Quantity);
        }

        // Order functionaliteit
        public async Task<Order?> CreateOrderAsync(string customerName, string customerEmail, string deliveryAddress)
        {
            if (!_currentOrderItems.Any())
            {
                return null;
            }

            var order = new Order
            {
                Id = _orderHistory.Any() ? _orderHistory.Max(o => o.Id) + 1 : 1001,
                CustomerName = customerName,
                CustomerEmail = customerEmail,
                DeliveryAddress = deliveryAddress,
                OrderDate = DateTime.Now,
                OrderItems = new List<OrderItem>(_currentOrderItems.Select(item => new OrderItem
                {
                    Id = item.Id,
                    ProductId = item.ProductId,
                    Product = item.Product,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    OrderId = 0 // Wordt gezet door het order ID
                })),
                Status = OrderStatus.Pending
            };

            // API call (mock voor nu)
            // var createdOrder = await _apiService.PostAsync<Order, Order>("orders", order);

            // Sla op in lokale geschiedenis
            _orderHistory.Add(order);

            // Leeg de winkelwagen
            ClearCart();

            // Simuleer API delay
            await Task.Delay(500);

            return order;
        }

        // Bestelgeschiedenis functionaliteit
        public async Task<List<Order>> GetOrderHistoryAsync()
        {
            // API call voor echte implementatie
            // return await _apiService.GetAsync<List<Order>>("orders") ?? new List<Order>();

            // Return lokale geschiedenis, gesorteerd op datum (nieuwste eerst)
            await Task.Delay(100); // Simuleer API call
            return _orderHistory.OrderByDescending(o => o.OrderDate).ToList();
        }

        public async Task<Order?> GetOrderByIdAsync(int orderId)
        {
            // API call
            // return await _apiService.GetAsync<Order>($"orders/{orderId}");

            await Task.Delay(100);
            return _orderHistory.FirstOrDefault(o => o.Id == orderId);
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus status)
        {
            // API call
            // return await _apiService.PutAsync($"orders/{orderId}/status", new { Status = status });

            var order = _orderHistory.FirstOrDefault(o => o.Id == orderId);
            if (order != null)
            {
                order.Status = status;
                await Task.Delay(100);
                return true;
            }

            return false;
        }

        public async Task<bool> CancelOrderAsync(int orderId)
        {
            return await UpdateOrderStatusAsync(orderId, OrderStatus.Cancelled);
        }

        /// <summary>
        /// Verwijder een bestelling volledig uit de geschiedenis
        /// </summary>
        public async Task<bool> DeleteOrderAsync(int orderId)
        {
            var order = _orderHistory.FirstOrDefault(o => o.Id == orderId);
            if (order != null)
            {
                _orderHistory.Remove(order);
                await Task.Delay(100);
                return true;
            }
            return false;
        }

        // Statistieken
        public int GetTotalOrdersCount()
        {
            return _orderHistory.Count;
        }

        public decimal GetTotalSpent()
        {
            return _orderHistory.Sum(o => o.TotalAmount);
        }

        public List<Order> GetRecentOrders(int count = 5)
        {
            return _orderHistory
                .OrderByDescending(o => o.OrderDate)
                .Take(count)
                .ToList();
        }
    }
}