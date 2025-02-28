using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Modules.ItemModule.Models
{
    public class Item
    {
        [Key]
        public Guid ItemId { get; set; } = Guid.NewGuid();

        [Required, MaxLength(256)]
        public string Name { get; set; }

        [Required]
        public string Picture { get; set; }

        public string? Description { get; set; }

        [Required]
        public int Price { get; set; }

        public bool Purchasable { get; set; }

        [ForeignKey("Rarity")]
        public Guid RarityId { get; set; }
        public Rarity Rarity { get; set; }
    }
}