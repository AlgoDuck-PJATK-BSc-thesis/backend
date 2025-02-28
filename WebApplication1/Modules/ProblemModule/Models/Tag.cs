using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Modules.ProblemModule.Models
{
    public class Tag
    {
        [Key]
        public Guid TagId { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(256)]
        public required string TagName { get; set; }
        
        public ICollection<HasTag> HasTags { get; set; } = new List<HasTag>();

    }
}