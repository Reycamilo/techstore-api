using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace techstore_api.DataBase.Entities
{
    public class RoleEntity : IdentityRole
    {
        [StringLength(250)]
        public string Description { get; set; } = string.Empty;
    }
}