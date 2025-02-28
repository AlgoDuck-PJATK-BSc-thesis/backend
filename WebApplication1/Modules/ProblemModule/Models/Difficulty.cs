using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Modules.ProblemModule.Models
{
    public class Difficulty
    {
        [Key]
        public Guid DifficultyId { get; set; } = Guid.NewGuid();

        [Required, MaxLength(256)]
        public required string DifficultyName { get; set; }
    }
}