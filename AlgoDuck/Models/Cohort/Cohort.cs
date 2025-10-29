using System.ComponentModel.DataAnnotations;
using ApplicationUser = AlgoDuck.Models.User.ApplicationUser;

namespace AlgoDuck.Models.Cohort
{
    public class Cohort
    {
        [Key]
        public Guid CohortId { get; set; } = Guid.NewGuid();

        [Required, MaxLength(256)]
        public required string Name { get; set; }

        [Required, MaxLength(256)]
        public required string ImageUrl { get; set; }
        
        public Guid CreatedByUserId { get; set; }
        
        public ApplicationUser CreatedByUser { get; set; } = null!;

        public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
        public ICollection<Message> Messages { get; set; } = new List<Message>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    }
}