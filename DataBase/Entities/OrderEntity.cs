using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using techstore_api.DataBase.Entities.Common;

namespace techstore_api.DataBase.Entities
{
    public class OrderEntity : EntidadBase
    {
        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = string.Empty;

        public ICollection<OrderDetailEntity> OrderDetails { get; set; } = new List<OrderDetailEntity>();
    }
    
}