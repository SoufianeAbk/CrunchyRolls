namespace CrunchyRolls.Models.DTOs
{
    public class OrderDto
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string DeliveryAddress { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public List<OrderItemDto> OrderItems { get; set; } = new();
        public decimal TotalAmount { get; set; }
    }
}