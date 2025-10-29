using System.ComponentModel.DataAnnotations;

namespace AlgoDuck.Models.Problem
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