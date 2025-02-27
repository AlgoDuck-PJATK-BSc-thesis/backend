using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApplication1.Modules.UserModule.Models;

namespace WebApplication1.Modules.AuthModule.Models
{
    public class Session
    {
        [Key]
        public Guid SessionId { get; set; } = Guid.NewGuid();

        [Required, MaxLength(512)]
        public string RefreshToken { get; set; }

        [Required]
        public DateTime RefreshTokenExpiresAt { get; set; }

        public bool Revoked { get; set; } = false;

        [ForeignKey("User")]
        public Guid UserId { get; set; }
        public User User { get; set; }
    }
}