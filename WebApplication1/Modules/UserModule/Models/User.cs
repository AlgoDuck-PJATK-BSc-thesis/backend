using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApplication1.Modules.CohortModule.Models;
using WebApplication1.Modules.AuthModule.Models;

namespace WebApplication1.Modules.UserModule.Models
{
    public class User
    {
        [Key]
        public Guid UserId { get; set; } = Guid.NewGuid();

        [Required, MaxLength(256)]
        public string Username { get; set; }

        [Required, MaxLength(256)]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string Salt { get; set; }

        public int Coins { get; set; }
        public int Experience { get; set; }
        public int AmountSolved { get; set; }

        [MaxLength(256)]
        public string ProfilePicture { get; set; }

        [ForeignKey("UserRole")]
        public Guid UserRoleId { get; set; }
        public UserRole UserRole { get; set; }

        [ForeignKey("Cohort")]
        public Guid? CohortId { get; set; }
        public Cohort Cohort { get; set; }

        public ICollection<Session> Sessions { get; set; } = new List<Session>();
    }
}