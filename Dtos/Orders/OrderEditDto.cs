
using System.ComponentModel.DataAnnotations;

namespace techstore_api.Dtos.Orders
{
    public class OrderEditDto
    {
        [Required(ErrorMessage = "El ID de usuario es requerido.")]
        public string UserId { get; set; } = string.Empty;

        [Required(ErrorMessage = "El monto total es requerido.")]
        public decimal TotalAmount { get; set; }

        [Required(ErrorMessage = "El estado de la orden es requerido.")]
        [StringLength(50, ErrorMessage = "El estado no puede exceder los 50 caracteres.")]
        public string Status { get; set; } = string.Empty;

        public List<OrderDetailEditDto> OrderDetails { get; set; } = new List<OrderDetailEditDto>();
    }

    public class OrderDetailEditDto
    {
        public int Id { get; set; } // Id para detalle de orden existente
        public int? ProductId { get; set; }

        [Required(ErrorMessage = "La cantidad es requerida.")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser al menos 1.")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "El precio unitario es requerido.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio unitario debe ser mayor que 0.")]
        public decimal UnitPrice { get; set; }
    }
}