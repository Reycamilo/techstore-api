using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace techstore_api.DataBase.Entities
{
    public class UserEntity : IdentityUser
    {
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;
    }
}