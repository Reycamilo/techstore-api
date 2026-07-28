

using System.ComponentModel.DataAnnotations;

namespace techstore_api.Dtos.Orders
{
    public class OrderCreateDto
    {
        [Required(ErrorMessage = "El ID de usuario es requerido.")]
        public string UserId { get; set; } = string.Empty;

        // El backend calculará automáticamente el total
        public decimal TotalAmount { get; set; }

        // El backend establecerá automáticamente el estado como "PENDIENTE"
        public string Status { get; set; } = string.Empty;

        public List<OrderDetailCreateDto> OrderDetails { get; set; } = new List<OrderDetailCreateDto>();
    }

    public class OrderDetailCreateDto
    {
        public int? ProductId { get; set; }

        [Required(ErrorMessage = "La cantidad es requerida.")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser al menos 1.")]
        public int Quantity { get; set; }

        // El backend calculará automáticamente el precio unitario desde el producto/servicio
        public decimal UnitPrice { get; set; }
    }
}