using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ApplicationUser = AlgoDuck.Modules.User.Models.ApplicationUser;
using UserNamespace = AlgoDuck.Modules.User.Models;

namespace AlgoDuck.Modules.Cohort.Models
{
    public class Message
    {
        [Key]
        public Guid MessageId { get; set; } = Guid.NewGuid();

        [Required, MaxLength(256)]
        public required string Content { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("Cohort")]
        public Guid CohortId { get; set; }
        public Cohort? Cohort { get; set; }

        [ForeignKey("User")]
        public Guid UserId { get; set; }
        public ApplicationUser? User { get; set; }
    }
}

