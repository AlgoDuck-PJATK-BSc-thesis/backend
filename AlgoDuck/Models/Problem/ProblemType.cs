using System.ComponentModel.DataAnnotations;

namespace AlgoDuck.Models.Problem
{
    public class ProblemType
    {
        [Key]
        public Guid ProblemTypeId { get; set; } = Guid.NewGuid();

        [Required, MaxLength(256)]
        public required string Name { get; set; }
    }
}