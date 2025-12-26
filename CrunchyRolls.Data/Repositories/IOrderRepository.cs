using CrunchyRolls.Models.Entities;
using CrunchyRolls.Models.Enums;

namespace CrunchyRolls.Data.Repositories
{
    public interface IOrderRepository : IRepository<Order>
    {
        Task<Order?> GetWithItemsAsync(int id);
        Task<IEnumerable<Order>> GetByCustomerEmailAsync(string email);
        Task<IEnumerable<Order>> GetByStatusAsync(OrderStatus status);
        Task<IEnumerable<Order>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task UpdateStatusAsync(int orderId, OrderStatus status);
        Task<IEnumerable<Order>> GetRecentOrdersAsync(int count = 10);
        Task<decimal> GetTotalRevenueAsync();
    }
}