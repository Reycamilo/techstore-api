
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace techstore_api.DataBase.Entities

/// <summary>
/// Transacción de pago asociada a una orden (registro del resultado de la pasarela).
/// </summary>
{
    public class PaymentTransactionEntity
    {
        [Required]
        public int OrderId { get; set; }
        public OrderEntity Order { get; set; } = null!;

        // ID de la transacción devuelto por la pasarela (ej. capture id de PayPal)
        [StringLength(200)]
        public string? GatewayTransactionId { get; set; }

        // Estado del pago: "PAGADA", "PAGO_RECHAZADO", "PENDIENTE"
        [Required]
        [StringLength(30)]
        public string Status { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(10)]
        public string Currency { get; set; } = "USD";

        [Required]
        [StringLength(50)]
        public string PaymentMethod { get; set; } = "PayPal";
    }
}