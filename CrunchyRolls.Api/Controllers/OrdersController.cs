using CrunchyRolls.Data.Repositories;
using CrunchyRolls.Models.Entities;
using CrunchyRolls.Models.Enums;
using Microsoft.AspNetCore.Mvc;

namespace CrunchyRolls.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(
            IOrderRepository orderRepository,
            ILogger<OrdersController> logger)
        {
            _orderRepository = orderRepository;
            _logger = logger;
        }

        // GET: api/orders
        /// <summary>
        /// Get all orders
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetAllOrders()
        {
            try
            {
                var orders = await _orderRepository.GetAllAsync();
                return Ok(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all orders");
                return StatusCode(500, new { message = "Error retrieving orders", error = ex.Message });
            }
        }

        // GET: api/orders/5
        /// <summary>
        /// Get order by ID with items
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetOrderById(int id)
        {
            try
            {
                var order = await _orderRepository.GetWithItemsAsync(id);

                if (order == null)
                    return NotFound(new { message = $"Order with ID {id} not found" });

                return Ok(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order");
                return StatusCode(500, new { message = "Error retrieving order", error = ex.Message });
            }
        }

        // GET: api/orders/customer/john@example.com
        /// <summary>
        /// Get orders by customer email
        /// </summary>
        [HttpGet("customer/{email}")]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrdersByCustomer(string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                    return BadRequest(new { message = "Email cannot be empty" });

                var orders = await _orderRepository.GetByCustomerEmailAsync(email);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer orders");
                return StatusCode(500, new { message = "Error retrieving orders", error = ex.Message });
            }
        }

        // GET: api/orders/status/pending
        /// <summary>
        /// Get orders by status
        /// </summary>
        [HttpGet("status/{status}")]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrdersByStatus(OrderStatus status)
        {
            try
            {
                var orders = await _orderRepository.GetByStatusAsync(status);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting orders by status");
                return StatusCode(500, new { message = "Error retrieving orders", error = ex.Message });
            }
        }

        // GET: api/orders/recent
        /// <summary>
        /// Get recent orders
        /// </summary>
        [HttpGet("recent")]
        public async Task<ActionResult<IEnumerable<Order>>> GetRecentOrders([FromQuery] int count = 10)
        {
            try
            {
                var orders = await _orderRepository.GetRecentOrdersAsync(count);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent orders");
                return StatusCode(500, new { message = "Error retrieving orders", error = ex.Message });
            }
        }

        // GET: api/orders/revenue
        /// <summary>
        /// Get total revenue from delivered orders
        /// </summary>
        [HttpGet("revenue")]
        public async Task<ActionResult<decimal>> GetTotalRevenue()
        {
            try
            {
                var revenue = await _orderRepository.GetTotalRevenueAsync();
                return Ok(new { totalRevenue = revenue });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating revenue");
                return StatusCode(500, new { message = "Error calculating revenue", error = ex.Message });
            }
        }

        // POST: api/orders
        /// <summary>
        /// Create new order
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Order>> CreateOrder([FromBody] Order order)
        {
            try
            {
                if (order == null)
                    return BadRequest(new { message = "Order cannot be null" });

                if (string.IsNullOrWhiteSpace(order.CustomerName))
                    return BadRequest(new { message = "Customer name is required" });

                if (string.IsNullOrWhiteSpace(order.CustomerEmail))
                    return BadRequest(new { message = "Customer email is required" });

                if (string.IsNullOrWhiteSpace(order.DeliveryAddress))
                    return BadRequest(new { message = "Delivery address is required" });

                if (!order.OrderItems.Any())
                    return BadRequest(new { message = "Order must contain at least one item" });

                var createdOrder = await _orderRepository.AddAsync(order);
                return CreatedAtAction(nameof(GetOrderById), new { id = createdOrder.Id }, createdOrder);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order");
                return StatusCode(500, new { message = "Error creating order", error = ex.Message });
            }
        }

        // PUT: api/orders/5/status
        /// <summary>
        /// Update order status
        /// </summary>
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] OrderStatusUpdateRequest request)
        {
            try
            {
                if (request == null)
                    return BadRequest(new { message = "Status update request cannot be null" });

                var order = await _orderRepository.GetByIdAsync(id);
                if (order == null)
                    return NotFound(new { message = $"Order with ID {id} not found" });

                await _orderRepository.UpdateStatusAsync(id, request.Status);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order status");
                return StatusCode(500, new { message = "Error updating order status", error = ex.Message });
            }
        }

        // DELETE: api/orders/5
        /// <summary>
        /// Delete order (cascade delete order items)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            try
            {
                var order = await _orderRepository.GetByIdAsync(id);
                if (order == null)
                    return NotFound(new { message = $"Order with ID {id} not found" });

                await _orderRepository.DeleteAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting order");
                return StatusCode(500, new { message = "Error deleting order", error = ex.Message });
            }
        }
    }

    // Helper class for order status update
    public class OrderStatusUpdateRequest
    {
        public OrderStatus Status { get; set; }
    }
}