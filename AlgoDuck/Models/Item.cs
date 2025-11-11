using System;
using System.Collections.Generic;

namespace AlgoDuck.Models;

public partial class Item
{
    public Guid ItemId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int Price { get; set; }

    public bool Purchasable { get; set; }

    public Guid RarityId { get; set; }

    public virtual ICollection<Contest> Contests { get; set; } = new List<Contest>();

    public virtual ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();

    public virtual Rarity Rarity { get; set; } = null!;
}
