using CrunchyRolls.Models.Enums;

namespace CrunchyRolls.Models.Entities
{
    public class Order
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string DeliveryAddress { get; set; } = string.Empty;
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public List<OrderItem> OrderItems { get; set; } = new();
        public decimal TotalAmount => OrderItems.Sum(item => item.SubTotal);
    }
}