using System.ComponentModel.DataAnnotations;

namespace AlgoDuck.Models.Item
{
    public class Rarity
    {
        [Key]
        public Guid RarityId { get; set; } = Guid.NewGuid();

        [Required, MaxLength(256)]
        public required string RarityName { get; set; }
    }
}