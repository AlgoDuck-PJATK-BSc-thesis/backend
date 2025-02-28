using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Modules.UserModule.Models
{
    public class UserRole
    {
        [Key]
        public Guid UserRoleId { get; set; } = Guid.NewGuid();

        [Required, MaxLength(256)]
        public string Name { get; set; }
    }
}