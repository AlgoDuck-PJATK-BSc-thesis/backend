using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Modules.ItemModule.Models
{
    public class Item
    {
        [Key]
        public Guid ItemId { get; set; } = Guid.NewGuid();

        [Required, MaxLength(256)]
        public required string Name { get; set; }

        [Required]
        public required string Picture { get; set; }

        public required string? Description { get; set; }

        [Required]
        public required int Price { get; set; }

        public required bool Purchasable { get; set; }

        [ForeignKey("Rarity")]
        public Guid RarityId { get; set; }
        public Rarity? Rarity { get; set; }
        
        public ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();
    }
}