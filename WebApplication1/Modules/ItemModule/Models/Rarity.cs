using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Modules.ItemModule.Models
{
    public class Rarity
    {
        [Key]
        public Guid RarityId { get; set; } = Guid.NewGuid();

        [Required, MaxLength(256)]
        public required string RarityName { get; set; }
    }
}