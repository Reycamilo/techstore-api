using System.ComponentModel.DataAnnotations;
using techstore_api.DataBase.Entities.Common;

namespace techstore_api.DataBase.Entities
{
    public class ProductEntity : EntidadBase
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public decimal Price { get; set; }

        [Required]
        public int Stock { get; set; }

        // Clave foránea para Categoría
        public int CategoryId { get; set; }
        public CategoryEntity Category { get; set; } = null!;

        // Clave foránea para Vendedor (UserEntity)
        public string SellerId { get; set; } = string.Empty;
        public UserEntity Seller { get; set; } = null!;

        // URL de la imagen del producto (opcional)
        [StringLength(500)]
        public string? ImageUrl { get; set; }
    }
}