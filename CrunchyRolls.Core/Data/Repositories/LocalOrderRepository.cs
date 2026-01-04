using CrunchyRolls.Models.Entities;
using CrunchyRolls.Models.Enums;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace CrunchyRolls.Core.Data.Repositories
{
    /// <summary>
    /// Lokale repository voor bestellingen
    /// Lokaal geplaatste orders + sync met API
    /// </summary>
    public class OrderLocalRepository : LocalRepository<Order>
    {
        public async Task<Order?> GetWithItemsAsync(int id)
        {
            try
            {
                return await _dbSet
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                    .FirstOrDefaultAsync(o => o.Id == id);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error getting order with items: {ex.Message}");
                return null;
            }
        }

        public async Task<IEnumerable<Order>> GetByCustomerEmailAsync(string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                    return new List<Order>();

                return await _dbSet
                    .Where(o => o.CustomerEmail.ToLower().Equals(email.ToLower()))
                    .Include(o => o.OrderItems)
                    .OrderByDescending(o => o.OrderDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error getting orders by email: {ex.Message}");
                return new List<Order>();
            }
        }

        public async Task<IEnumerable<Order>> GetByStatusAsync(OrderStatus status)
        {
            try
            {
                return await _dbSet
                    .Where(o => o.Status == status)
                    .Include(o => o.OrderItems)
                    .OrderByDescending(o => o.OrderDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error getting orders by status: {ex.Message}");
                return new List<Order>();
            }
        }

        public async Task<IEnumerable<Order>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                return await _dbSet
                    .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate)
                    .Include(o => o.OrderItems)
                    .OrderByDescending(o => o.OrderDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error getting orders by date range: {ex.Message}");
                return new List<Order>();
            }
        }

        public async Task<bool> UpdateStatusAsync(int orderId, OrderStatus status)
        {
            try
            {
                var order = await GetByIdAsync(orderId);
                if (order != null)
                {
                    order.Status = status;
                    await UpdateAsync(order);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error updating order status: {ex.Message}");
                return false;
            }
        }

        public async Task<IEnumerable<Order>> GetRecentOrdersAsync(int count = 10)
        {
            try
            {
                return await _dbSet
                    .Include(o => o.OrderItems)
                    .OrderByDescending(o => o.OrderDate)
                    .Take(count)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error getting recent orders: {ex.Message}");
                return new List<Order>();
            }
        }

        public async Task<decimal> GetTotalRevenueAsync()
        {
            try
            {
                return await _dbSet
                    .Where(o => o.Status == OrderStatus.Delivered || o.Status == OrderStatus.Shipped)
                    .SumAsync(o => o.TotalAmount);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error calculating revenue: {ex.Message}");
                return 0m;
            }
        }

        /// <summary>
        /// Get orders that were synced to API
        /// (ones with ID > 0, meaning they came from API)
        /// </summary>
        public async Task<IEnumerable<Order>> GetSyncedOrdersAsync()
        {
            try
            {
                return await _dbSet
                    .Where(o => o.Id > 0)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error getting synced orders: {ex.Message}");
                return new List<Order>();
            }
        }

        /// <summary>
        /// Get pending orders (not yet sent to API)
        /// In a real app, you'd have a separate flag for this
        /// </summary>
        public async Task<IEnumerable<Order>> GetPendingOrdersAsync()
        {
            try
            {
                // Orders created locally without API sync would have specific markers
                // For now, return all undelivered orders
                return await _dbSet
                    .Where(o => o.Status == OrderStatus.Pending || o.Status == OrderStatus.Processing)
                    .OrderBy(o => o.OrderDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error getting pending orders: {ex.Message}");
                return new List<Order>();
            }
        }
    }
}