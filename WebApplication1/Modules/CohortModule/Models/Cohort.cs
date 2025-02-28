using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WebApplication1.Modules.UserModule.Models;

namespace WebApplication1.Modules.CohortModule.Models
{
    public class Cohort
    {
        [Key]
        public Guid CohortId { get; set; } = Guid.NewGuid();

        [Required, MaxLength(256)]
        public string Name { get; set; }

        [Required]
        public string ImageUrl { get; set; }

        public ICollection<User> Users { get; set; } = new List<User>();
    }
}