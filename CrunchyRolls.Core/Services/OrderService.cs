using CrunchyRolls.Models.Entities;
using CrunchyRolls.Models.Enums;
using System.Diagnostics;

namespace CrunchyRolls.Core.Services
{
    /// <summary>
    /// Service voor order en winkelwagen management.
    /// Integratie met Order API voor persistentie.
    /// </summary>
    public class OrderService
    {
        private readonly ApiService _apiService;

        // In-memory winkelwagen (session)
        private readonly List<OrderItem> _currentOrderItems = new();

        // Cache van bestellingen (fallback for offline)
        private List<Order>? _cachedOrderHistory;

        public OrderService(ApiService apiService)
        {
            _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
            Debug.WriteLine("🛒 OrderService initialized");
        }

        // ===== WINKELWAGEN FUNCTIONALITEIT =====

        /// <summary>
        /// Voeg product toe aan winkelwagen
        /// </summary>
        public void AddToCart(Product product, int quantity = 1)
        {
            if (product == null)
            {
                Debug.WriteLine("⚠️ Attempted to add null product to cart");
                return;
            }

            if (!product.IsInStock)
            {
                Debug.WriteLine($"⚠️ Product {product.Name} is not in stock");
                return;
            }

            var existingItem = _currentOrderItems.FirstOrDefault(i => i.ProductId == product.Id);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
                Debug.WriteLine($"✅ Updated quantity for {product.Name}: {existingItem.Quantity}");
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
                Debug.WriteLine($"✅ Added {product.Name} to cart (qty: {quantity})");
            }
        }

        /// <summary>
        /// Verwijder product uit winkelwagen
        /// </summary>
        public void RemoveFromCart(int productId)
        {
            var item = _currentOrderItems.FirstOrDefault(i => i.ProductId == productId);
            if (item != null)
            {
                _currentOrderItems.Remove(item);
                Debug.WriteLine($"✅ Removed product {productId} from cart");
            }
        }

        /// <summary>
        /// Update quantity van product in winkelwagen
        /// </summary>
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
                    Debug.WriteLine($"✅ Updated product {productId} quantity to {quantity}");
                }
            }
        }

        /// <summary>
        /// Get all cart items
        /// </summary>
        public List<OrderItem> GetCartItems()
        {
            return _currentOrderItems;
        }

        /// <summary>
        /// Clear entire cart
        /// </summary>
        public void ClearCart()
        {
            _currentOrderItems.Clear();
            Debug.WriteLine("🛒 Cart cleared");
        }

        /// <summary>
        /// Get cart total price
        /// </summary>
        public decimal GetCartTotal()
        {
            return _currentOrderItems.Sum(item => item.SubTotal);
        }

        /// <summary>
        /// Get total number of items in cart
        /// </summary>
        public int GetCartItemCount()
        {
            return _currentOrderItems.Sum(item => item.Quantity);
        }

        // ===== ORDER FUNCTIONALITEIT =====

        /// <summary>
        /// Create order via API - WITH EXTENSIVE DEBUGGING
        /// </summary>
        public async Task<Order?> CreateOrderAsync(string customerName, string customerEmail, string deliveryAddress, List<OrderItem> orderItems)
        {
            try
            {
                // ===== DEBUG: INPUT VALIDATION =====
                Debug.WriteLine("\n╔════════════════════════════════════════════════════╗");
                Debug.WriteLine("║          🔍 ORDER CREATION DEBUG START 🔍          ║");
                Debug.WriteLine("╚════════════════════════════════════════════════════╝\n");

                Debug.WriteLine("📧 CUSTOMER DATA:");
                Debug.WriteLine($"   Name: '{customerName}' (empty: {string.IsNullOrWhiteSpace(customerName)})");
                Debug.WriteLine($"   Email: '{customerEmail}' (empty: {string.IsNullOrWhiteSpace(customerEmail)})");
                Debug.WriteLine($"   Address: '{deliveryAddress}' (empty: {string.IsNullOrWhiteSpace(deliveryAddress)})");

                Debug.WriteLine("\n📦 ORDER ITEMS:");
                Debug.WriteLine($"   Parameter count: {orderItems?.Count ?? 0}");
                Debug.WriteLine($"   Cart count: {_currentOrderItems.Count}");

                // ===== VALIDATION =====
                if (string.IsNullOrWhiteSpace(customerName))
                {
                    Debug.WriteLine("\n❌ VALIDATION FAILED: CustomerName is empty or whitespace");
                    return null;
                }

                if (string.IsNullOrWhiteSpace(customerEmail))
                {
                    Debug.WriteLine("\n❌ VALIDATION FAILED: CustomerEmail is empty or whitespace");
                    return null;
                }

                if (string.IsNullOrWhiteSpace(deliveryAddress))
                {
                    Debug.WriteLine("\n❌ VALIDATION FAILED: DeliveryAddress is empty or whitespace");
                    return null;
                }

                if (orderItems == null)
                {
                    Debug.WriteLine("\n❌ VALIDATION FAILED: orderItems parameter is NULL");
                    Debug.WriteLine("   This means NO items were passed from the ViewModel!");
                    return null;
                }

                if (!orderItems.Any())
                {
                    Debug.WriteLine("\n❌ VALIDATION FAILED: orderItems is empty");
                    Debug.WriteLine("   CartItems in ViewModel may not have been loaded correctly");
                    return null;
                }

                // ===== VALIDATE EACH ITEM =====
                Debug.WriteLine("\n📋 VALIDATING INDIVIDUAL ITEMS:");
                for (int i = 0; i < orderItems.Count; i++)
                {
                    var item = orderItems[i];
                    Debug.WriteLine($"\n   Item #{i + 1}:");
                    Debug.WriteLine($"      ProductId: {item.ProductId} (valid: {item.ProductId > 0})");
                    Debug.WriteLine($"      Quantity: {item.Quantity} (valid: {item.Quantity > 0})");
                    Debug.WriteLine($"      UnitPrice: €{item.UnitPrice:F2} (valid: {item.UnitPrice >= 0})");

                    if (item.ProductId <= 0)
                    {
                        Debug.WriteLine($"      ❌ ERROR: ProductId must be > 0, got {item.ProductId}");
                        return null;
                    }

                    if (item.Quantity <= 0)
                    {
                        Debug.WriteLine($"      ❌ ERROR: Quantity must be > 0, got {item.Quantity}");
                        return null;
                    }

                    if (item.UnitPrice < 0)
                    {
                        Debug.WriteLine($"      ❌ ERROR: UnitPrice cannot be negative, got €{item.UnitPrice:F2}");
                        return null;
                    }
                }

                // ===== BUILD ORDER =====
                Debug.WriteLine("\n🔨 BUILDING ORDER OBJECT:");
                var order = new Order
                {
                    CustomerName = customerName.Trim(),
                    CustomerEmail = customerEmail.Trim(),
                    DeliveryAddress = deliveryAddress.Trim(),
                    OrderDate = DateTime.Now,
                    OrderItems = orderItems,
                    Status = OrderStatus.Pending
                };

                Debug.WriteLine($"   Total items: {order.OrderItems.Count}");
                Debug.WriteLine($"   Subtotal: €{order.TotalAmount:F2}");
                Debug.WriteLine($"   Status: {order.Status}");
                Debug.WriteLine($"   Date: {order.OrderDate:yyyy-MM-dd HH:mm:ss}");

                // ===== SEND TO API =====
                Debug.WriteLine("\n📤 SENDING TO API:");
                Debug.WriteLine($"   Endpoint: POST /api/orders");
                Debug.WriteLine($"   Payload size: {System.Text.Json.JsonSerializer.Serialize(order).Length} bytes");

                var createdOrder = await _apiService.PostAsync<Order, Order>("orders", order);

                // ===== CHECK RESPONSE =====
                if (createdOrder != null)
                {
                    Debug.WriteLine("\n✅✅✅ SUCCESS! ORDER CREATED ✅✅✅");
                    Debug.WriteLine($"   Order ID: {createdOrder.Id}");
                    Debug.WriteLine($"   Total: €{createdOrder.TotalAmount:F2}");
                    Debug.WriteLine($"   Items: {createdOrder.OrderItems.Count}");
                    Debug.WriteLine($"   Status: {createdOrder.Status}");

                    ClearCart();
                    _cachedOrderHistory?.Add(createdOrder);

                    Debug.WriteLine("\n╔════════════════════════════════════════════════════╗");
                    Debug.WriteLine("║        ✅ ORDER CREATION DEBUG END - SUCCESS ✅   ║");
                    Debug.WriteLine("╚════════════════════════════════════════════════════╝\n");

                    return createdOrder;
                }
                else
                {
                    Debug.WriteLine("\n❌ API RETURNED NULL");
                    Debug.WriteLine("   Check ApiService debug output above for error details");
                    Debug.WriteLine("   Common causes:");
                    Debug.WriteLine("   - API returned 400 Bad Request (check validation)");
                    Debug.WriteLine("   - API returned 500 Internal Server Error (check database)");
                    Debug.WriteLine("   - Network timeout (check connection)");
                    Debug.WriteLine("   - JSON deserialization error (check model)");

                    Debug.WriteLine("\n╔════════════════════════════════════════════════════╗");
                    Debug.WriteLine("║        ❌ ORDER CREATION DEBUG END - FAILED ❌    ║");
                    Debug.WriteLine("╚════════════════════════════════════════════════════╝\n");

                    return null;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("\n💥💥💥 EXCEPTION THROWN 💥💥💥");
                Debug.WriteLine($"   Exception Type: {ex.GetType().Name}");
                Debug.WriteLine($"   Message: {ex.Message}");
                Debug.WriteLine($"   StackTrace:\n{ex.StackTrace}");

                if (ex.InnerException != null)
                {
                    Debug.WriteLine($"\n   Inner Exception: {ex.InnerException.Message}");
                    Debug.WriteLine($"   Inner StackTrace:\n{ex.InnerException.StackTrace}");
                }

                Debug.WriteLine("\n╔════════════════════════════════════════════════════╗");
                Debug.WriteLine("║      ❌ ORDER CREATION DEBUG END - EXCEPTION ❌   ║");
                Debug.WriteLine("╚════════════════════════════════════════════════════╝\n");

                throw;
            }
        }

        // ===== BESTELGESCHIEDENIS FUNCTIONALITEIT =====

        /// <summary>
        /// Get order history van customer (via email)
        /// </summary>
        public async Task<List<Order>> GetOrderHistoryAsync(string customerEmail)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(customerEmail))
                {
                    Debug.WriteLine("⚠️ Attempted to get order history without email");
                    return new List<Order>();
                }

                Debug.WriteLine($"📋 Fetching order history for {customerEmail}...");

                var orders = await _apiService.GetAsync<List<Order>>($"orders/customer/{Uri.EscapeDataString(customerEmail)}");

                if (orders != null && orders.Any())
                {
                    _cachedOrderHistory = orders.OrderByDescending(o => o.OrderDate).ToList();
                    Debug.WriteLine($"✅ Loaded {orders.Count} orders for {customerEmail} from API");
                    return _cachedOrderHistory;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"⚠️ Failed to fetch order history from API: {ex.Message}");
            }

            if (_cachedOrderHistory != null)
            {
                Debug.WriteLine("💾 Using cached order history");
                return _cachedOrderHistory;
            }

            Debug.WriteLine("❌ No cached order history available");
            return new List<Order>();
        }

        /// <summary>
        /// Get all orders (admin function)
        /// </summary>
        public async Task<List<Order>> GetAllOrdersAsync()
        {
            try
            {
                Debug.WriteLine("📋 Fetching all orders...");
                var orders = await _apiService.GetAsync<List<Order>>("orders");

                if (orders != null && orders.Any())
                {
                    Debug.WriteLine($"✅ Loaded {orders.Count} orders from API");
                    return orders.OrderByDescending(o => o.OrderDate).ToList();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"⚠️ Failed to fetch all orders from API: {ex.Message}");
            }

            return new List<Order>();
        }

        /// <summary>
        /// Get specific order by ID
        /// </summary>
        public async Task<Order?> GetOrderByIdAsync(int orderId)
        {
            try
            {
                Debug.WriteLine($"📋 Fetching order #{orderId}...");
                var order = await _apiService.GetAsync<Order>($"orders/{orderId}");

                if (order != null)
                {
                    Debug.WriteLine($"✅ Loaded order #{orderId} from API");
                    return order;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"⚠️ Failed to fetch order #{orderId}: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Get orders by status
        /// </summary>
        public async Task<List<Order>> GetOrdersByStatusAsync(OrderStatus status)
        {
            try
            {
                Debug.WriteLine($"📋 Fetching orders with status {status}...");
                var orders = await _apiService.GetAsync<List<Order>>($"orders/status/{status}");

                if (orders != null)
                {
                    Debug.WriteLine($"✅ Loaded {orders.Count} orders with status {status}");
                    return orders;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"⚠️ Failed to fetch orders with status {status}: {ex.Message}");
            }

            return new List<Order>();
        }

        /// <summary>
        /// Get recent orders
        /// </summary>
        public async Task<List<Order>> GetRecentOrdersAsync(int count = 10)
        {
            try
            {
                Debug.WriteLine($"📋 Fetching {count} recent orders...");
                var orders = await _apiService.GetAsync<List<Order>>($"orders/recent?count={count}");

                if (orders != null)
                {
                    Debug.WriteLine($"✅ Loaded {orders.Count} recent orders");
                    return orders;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"⚠️ Failed to fetch recent orders: {ex.Message}");
            }

            return new List<Order>();
        }

        /// <summary>
        /// Update order status
        /// </summary>
        public async Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus status)
        {
            try
            {
                Debug.WriteLine($"📝 Updating order #{orderId} status to {status}...");

                var request = new { Status = status };
                var success = await _apiService.PutAsync($"orders/{orderId}/status", request);

                if (success)
                {
                    Debug.WriteLine($"✅ Order #{orderId} status updated successfully");
                }

                return success;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Failed to update order #{orderId} status: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Cancel order
        /// </summary>
        public async Task<bool> CancelOrderAsync(int orderId)
        {
            return await UpdateOrderStatusAsync(orderId, OrderStatus.Cancelled);
        }

        /// <summary>
        /// Delete order
        /// </summary>
        public async Task<bool> DeleteOrderAsync(int orderId)
        {
            try
            {
                Debug.WriteLine($"🗑️ Deleting order #{orderId}...");

                var success = await _apiService.DeleteAsync($"orders/{orderId}");

                if (success)
                {
                    Debug.WriteLine($"✅ Order #{orderId} deleted successfully");

                    if (_cachedOrderHistory != null)
                    {
                        _cachedOrderHistory.RemoveAll(o => o.Id == orderId);
                    }
                }

                return success;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Failed to delete order #{orderId}: {ex.Message}");
                return false;
            }
        }

        // ===== STATISTIEKEN =====

        /// <summary>
        /// Get total number of orders
        /// </summary>
        public int GetTotalOrdersCount()
        {
            return _cachedOrderHistory?.Count ?? 0;
        }

        /// <summary>
        /// Get total amount spent
        /// </summary>
        public decimal GetTotalSpent()
        {
            return _cachedOrderHistory?.Sum(o => o.TotalAmount) ?? 0;
        }

        /// <summary>
        /// Get recent orders count
        /// </summary>
        public List<Order> GetRecentOrders(int count = 5)
        {
            if (_cachedOrderHistory == null)
                return new List<Order>();

            return _cachedOrderHistory
                .OrderByDescending(o => o.OrderDate)
                .Take(count)
                .ToList();
        }
    }

    /// <summary>
    /// Helper class for status update requests
    /// </summary>
    public class OrderStatusUpdateRequest
    {
        public OrderStatus Status { get; set; }
    }
}