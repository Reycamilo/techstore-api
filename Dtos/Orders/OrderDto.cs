
namespace techstore_api.Dtos.Orders
{
    public class OrderDto
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public List<OrderDetailDto> OrderDetails { get; set; } = new List<OrderDetailDto>();
    }

    public class OrderDetailDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int? ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? ProductSellerId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}