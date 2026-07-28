
using System.ComponentModel.DataAnnotations;

namespace techstore_api.Dtos.Products
{
    public class ProductCreateDto
    {
        [Required(ErrorMessage = "El nombre del producto es requerido.")]
        [StringLength(100, ErrorMessage = "El nombre del producto no puede exceder los 100 caracteres.")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "La descripción del producto no puede exceder los 500 caracteres.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "El precio del producto es requerido.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0.")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "El stock del producto es requerido.")]
        [Range(0, int.MaxValue, ErrorMessage = "El stock debe ser mayor o igual a 0.")]
        public int Stock { get; set; }

        [Required(ErrorMessage = "La categoría del producto es requerida.")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "El vendedor del producto es requerido.")]
        public string SellerId { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
    }
}