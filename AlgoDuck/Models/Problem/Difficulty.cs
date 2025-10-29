using System.ComponentModel.DataAnnotations;

namespace AlgoDuck.Models.Problem
{
    public class Difficulty
    {
        [Key]
        public Guid DifficultyId { get; set; } = Guid.NewGuid();

        [Required, MaxLength(256)]
        public required string DifficultyName { get; set; }
    }
}