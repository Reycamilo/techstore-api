using System.ComponentModel.DataAnnotations;
using techstore_api.DataBase.Entities.Common;

namespace techstore_api.DataBase.Entities
{
    public class CategoryEntity : EntidadBase
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Type { get; set; } = string.Empty; // "Product" o "Service"

        [Required]
        public ICollection<ProductEntity> Products { get; set; } = new List<ProductEntity>();
    }
}