using System.ComponentModel.DataAnnotations;

namespace techstore_api.Dtos.Transactions
{
    /// <summary>
    /// Datos para registrar una transacción de pago (usado por la pasarela al capturar el pago).
    /// </summary>
    public class TransactionCreateDto
    {
        [Required]
        public int OrderId { get; set; }

        public string? GatewayTransactionId { get; set; }

        [Required]
        [StringLength(30)]
        public string Status { get; set; } = string.Empty;

        [Required]
        public decimal Amount { get; set; }

        [StringLength(10)]
        public string Currency { get; set; } = "USD";

        [StringLength(50)]
        public string PaymentMethod { get; set; } = "PayPal";
    }
}