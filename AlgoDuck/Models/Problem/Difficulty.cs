using System.ComponentModel.DataAnnotations;

namespace AlgoDuck.Modules.Problem.Models
{
    public class Difficulty
    {
        [Key]
        public Guid DifficultyId { get; set; } = Guid.NewGuid();

        [Required, MaxLength(256)]
        public required string DifficultyName { get; set; }
    }
}