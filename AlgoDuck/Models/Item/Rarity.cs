using System.ComponentModel.DataAnnotations;

namespace AlgoDuck.Modules.Item.Models
{
    public class Rarity
    {
        [Key]
        public Guid RarityId { get; set; } = Guid.NewGuid();

        [Required, MaxLength(256)]
        public required string RarityName { get; set; }
    }
}