using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlgoDuck.Modules.Cohort.Models
{
    public class Notification
    {
        [Key]
        public Guid NotificationId { get; set; } = Guid.NewGuid();

        [Required, MaxLength(512)]
        public required string Message { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("Cohort")]
        public Guid CohortId { get; set; }
        public required Cohort Cohort { get; set; }
    }
}