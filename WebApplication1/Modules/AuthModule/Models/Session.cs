using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UserNamespace = WebApplication1.Modules.UserModule.Models;

namespace WebApplication1.Modules.AuthModule.Models
{
    public class Session
    {
        [Key]
        public Guid SessionId { get; set; } = Guid.NewGuid();

        [Required, MaxLength(512)]
        public required string RefreshToken { get; set; }

        [Required]
        public DateTime RefreshTokenExpiresAt { get; set; }

        public bool Revoked { get; set; } = false;

        [ForeignKey(nameof(User))]
        public required Guid UserId { get; set; }
        
        public required UserNamespace.User User { get; set; }
    }
}