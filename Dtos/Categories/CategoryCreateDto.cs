using System.ComponentModel.DataAnnotations;

namespace techstore_api.Dtos.Categories
{
    public class CategoryCreateDto
    {
        [Required(ErrorMessage = "El nombre de la categoría es requerido.")]
        [StringLength(100, ErrorMessage = "El nombre de la categoría no puede exceder los 100 caracteres.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "El tipo de categoría es requerido.")]
        [StringLength(20, ErrorMessage = "El tipo de categoría no puede exceder los 20 caracteres.")]
        public string Type { get; set; } = string.Empty; // "Product" o "Service"
    }
}