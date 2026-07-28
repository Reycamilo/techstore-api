using System.ComponentModel.DataAnnotations;

namespace techstore_api.Dtos.Security.Roles
{
    public class RoleEditDto
    {
        [Required(ErrorMessage = "El nombre del rol es requerido.")]
        [StringLength(50, ErrorMessage = "El nombre del rol no puede exceder los 50 caracteres.")]
        public string Name { get; set; } = string.Empty;

        [StringLength(250, ErrorMessage = "La descripción del rol no puede exceder los 250 caracteres.")]
        public string? Description { get; set; }
    }
}