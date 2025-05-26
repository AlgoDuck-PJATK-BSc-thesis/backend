using System.ComponentModel.DataAnnotations;
using UserNamespace = WebApplication1.Modules.UserModule.Models;

namespace WebApplication1.Modules.CohortModule.Models
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
        
        public UserNamespace.ApplicationUser CreatedByUser { get; set; } = null!;

        public ICollection<UserNamespace.ApplicationUser> Users { get; set; } = new List<UserNamespace.ApplicationUser>();
        public ICollection<Message> Messages { get; set; } = new List<Message>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    }
}