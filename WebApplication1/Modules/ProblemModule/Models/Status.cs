using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Modules.ProblemModule.Models
{
    public class Status
    {
        [Key]
        public Guid StatusId { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(256)]
        public required string StatusName { get; set; }
    }
}