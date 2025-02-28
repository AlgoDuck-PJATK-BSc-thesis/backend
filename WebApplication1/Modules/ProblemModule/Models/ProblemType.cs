using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Modules.ProblemModule.Models
{
    public class ProblemType
    {
        [Key]
        public Guid ProblemTypeId { get; set; } = Guid.NewGuid();

        [Required, MaxLength(256)]
        public string Name { get; set; }
    }
}