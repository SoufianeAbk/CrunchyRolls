using CrunchyRolls.Core.Data.Repositories;
using CrunchyRolls.Models.Entities;
using CrunchyRolls.Models.Enums;
using System.Diagnostics;

namespace CrunchyRolls.Core.Services
{
    /// <summary>
    /// Hybrid OrderService - In-memory winkelwagen + persistent lokale orders
    /// 
    /// Features:
    /// - Persistent winkelwagen (lokale DB)
    /// - Offline order creation (lokaal)
    /// - Sync met API zodra beschikbaar
    /// </summary>
    public class HybridOrderService
    {
        private readonly ApiService _apiService;
        private readonly OrderLocalRepository _orderLocalRepo;

        // In-memory winkelwagen (session)
        private readonly List<OrderItem> _currentOrderItems = new();

        public HybridOrderService(ApiService apiService)
        {
            _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
            _orderLocalRepo = new OrderLocalRepository();

            Debug.WriteLine("🛒 HybridOrderService initialized");
        }

        // ===== WINKELWAGEN MANAGEMENT =====

        public void AddToCart(Product product, int quantity = 1)
        {
            if (product == null || !product.IsInStock)
                return;

            var existingItem = _currentOrderItems.FirstOrDefault(i => i.ProductId == product.Id);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
                Debug.WriteLine($"✅ Updated {product.Name} quantity to {existingItem.Quantity}");
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
                Debug.WriteLine($"✅ Added {product.Name} to cart");
            }
        }

        public void RemoveFromCart(int productId)
        {
            var item = _currentOrderItems.FirstOrDefault(i => i.ProductId == productId);
            if (item != null)
            {
                _currentOrderItems.Remove(item);
                Debug.WriteLine($"✅ Removed product {productId} from cart");
            }
        }

        public void UpdateQuantity(int productId, int quantity)
        {
            var item = _currentOrderItems.FirstOrDefault(i => i.ProductId == productId);
            if (item != null)
            {
                if (quantity <= 0)
                    RemoveFromCart(productId);
                else
                    item.Quantity = quantity;
            }
        }

        public List<OrderItem> GetCartItems() => _currentOrderItems;
        public void ClearCart() => _currentOrderItems.Clear();
        public decimal GetCartTotal() => _currentOrderItems.Sum(i => i.SubTotal);
        public int GetCartItemCount() => _currentOrderItems.Sum(i => i.Quantity);

        // ===== ORDER CREATION =====

        /// <summary>
        /// Create order: Try API, fallback to local storage
        /// </summary>
        public async Task<Order?> CreateOrderAsync(string customerName, string customerEmail, string deliveryAddress, List<OrderItem> orderItems)
        {
            try
            {
                // Validation
                if (string.IsNullOrWhiteSpace(customerName) ||
                    string.IsNullOrWhiteSpace(customerEmail) ||
                    string.IsNullOrWhiteSpace(deliveryAddress) ||
                    !orderItems.Any())
                {
                    Debug.WriteLine("❌ Missing required order fields");
                    return null;
                }

                var order = new Order
                {
                    CustomerName = customerName.Trim(),
                    CustomerEmail = customerEmail.Trim(),
                    DeliveryAddress = deliveryAddress.Trim(),
                    OrderDate = DateTime.Now,
                    OrderItems = orderItems,
                    Status = OrderStatus.Pending
                };

                // Try to sync with API
                Order? apiOrder = null;
                try
                {
                    Debug.WriteLine("📤 Attempting to create order via API...");
                    apiOrder = await _apiService.PostAsync<Order, Order>("orders", order);

                    if (apiOrder != null)
                    {
                        // Save to local cache too
                        await _orderLocalRepo.AddAsync(apiOrder);
                        ClearCart();

                        Debug.WriteLine($"✅ Order #{apiOrder.Id} created via API and cached locally");
                        return apiOrder;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"⚠️ API order creation failed: {ex.Message}");
                    Debug.WriteLine("💾 Saving order locally for later sync");
                }

                // Fallback: save locally if API unavailable
                var localOrder = await _orderLocalRepo.AddAsync(order);
                ClearCart();

                Debug.WriteLine($"✅ Order created locally (will sync to API when available)");
                return localOrder;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ CreateOrderAsync error: {ex.Message}");
                return null;
            }
        }

        // ===== ORDER RETRIEVAL =====

        /// <summary>
        /// Get order history: API first, then local
        /// </summary>
        public async Task<List<Order>> GetOrderHistoryAsync(string customerEmail)
        {
            if (string.IsNullOrWhiteSpace(customerEmail))
                return new List<Order>();

            try
            {
                // Try API first
                try
                {
                    Debug.WriteLine($"📡 Fetching orders from API for {customerEmail}...");
                    var apiOrders = await _apiService.GetAsync<List<Order>>($"orders/customer/{Uri.EscapeDataString(customerEmail)}");

                    if (apiOrders != null && apiOrders.Any())
                    {
                        // Cache locally
                        await _orderLocalRepo.ClearAllAsync();
                        await _orderLocalRepo.AddRangeAsync(apiOrders);

                        Debug.WriteLine($"✅ Synced {apiOrders.Count} orders from API");
                        return apiOrders.OrderByDescending(o => o.OrderDate).ToList();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"⚠️ API fetch failed: {ex.Message} - using local cache");
                }

                // Fallback: use local cache
                var localOrders = await _orderLocalRepo.GetByCustomerEmailAsync(customerEmail);
                Debug.WriteLine($"💾 Using {localOrders.Count()} orders from local cache");
                return localOrders.ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ GetOrderHistoryAsync error: {ex.Message}");
                return new List<Order>();
            }
        }

        public async Task<Order?> GetOrderByIdAsync(int orderId)
        {
            try
            {
                // Try API first
                try
                {
                    var apiOrder = await _apiService.GetAsync<Order>($"orders/{orderId}");
                    if (apiOrder != null)
                    {
                        Debug.WriteLine($"✅ Got order #{orderId} from API");
                        return apiOrder;
                    }
                }
                catch
                {
                    Debug.WriteLine($"⚠️ API fetch failed - using local cache");
                }

                // Fallback: use local cache
                return await _orderLocalRepo.GetByIdAsync(orderId);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ GetOrderByIdAsync error: {ex.Message}");
                return null;
            }
        }

        public async Task<List<Order>> GetOrdersByStatusAsync(OrderStatus status)
        {
            try
            {
                var orders = await _orderLocalRepo.GetByStatusAsync(status);
                Debug.WriteLine($"📋 Found {orders.Count()} orders with status {status}");
                return orders.ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ GetOrdersByStatusAsync error: {ex.Message}");
                return new List<Order>();
            }
        }

        // ===== ORDER MANAGEMENT =====

        public async Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus status)
        {
            try
            {
                // Try API first
                try
                {
                    var request = new { Status = status };
                    var success = await _apiService.PutAsync($"orders/{orderId}/status", request);
                    if (success)
                    {
                        // Update local cache
                        await _orderLocalRepo.UpdateStatusAsync(orderId, status);
                        Debug.WriteLine($"✅ Order #{orderId} status updated via API");
                        return true;
                    }
                }
                catch
                {
                    Debug.WriteLine($"⚠️ API update failed - updating local only");
                }

                // Fallback: update local
                return await _orderLocalRepo.UpdateStatusAsync(orderId, status);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ UpdateOrderStatusAsync error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteOrderAsync(int orderId)
        {
            try
            {
                // Try API first
                try
                {
                    var success = await _apiService.DeleteAsync($"orders/{orderId}");
                    if (success)
                    {
                        // Delete from local too
                        await _orderLocalRepo.DeleteAsync(orderId);
                        Debug.WriteLine($"✅ Order #{orderId} deleted via API");
                        return true;
                    }
                }
                catch
                {
                    Debug.WriteLine($"⚠️ API delete failed - deleting local only");
                }

                // Fallback: delete local
                return await _orderLocalRepo.DeleteAsync(orderId);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ DeleteOrderAsync error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Sync pending local orders to API
        /// Call when API becomes available again
        /// </summary>
        public async Task SyncPendingOrdersAsync()
        {
            try
            {
                Debug.WriteLine("🔄 Syncing pending orders with API...");

                var pendingOrders = await _orderLocalRepo.GetPendingOrdersAsync();
                int synced = 0;

                foreach (var order in pendingOrders)
                {
                    try
                    {
                        var apiOrder = await _apiService.PostAsync<Order, Order>("orders", order);
                        if (apiOrder != null)
                        {
                            // Update local record with API ID
                            order.Id = apiOrder.Id;
                            await _orderLocalRepo.UpdateAsync(order);
                            synced++;

                            Debug.WriteLine($"✅ Synced local order to API as #{apiOrder.Id}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"⚠️ Failed to sync order: {ex.Message}");
                    }
                }

                Debug.WriteLine($"✅ Synced {synced} orders with API");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ SyncPendingOrdersAsync error: {ex.Message}");
            }
        }

        public void Dispose()
        {
            _orderLocalRepo?.Dispose();
        }
    }
}