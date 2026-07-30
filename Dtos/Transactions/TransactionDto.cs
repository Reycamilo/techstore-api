
namespace techstore_api.Dtos.Transactions
{
    public class TransactionDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string? GatewayTransactionId { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        // Datos de la orden asociada (para el historial)
        public string OrderStatus { get; set; } = string.Empty;
        public decimal OrderTotal { get; set; }
    }
}