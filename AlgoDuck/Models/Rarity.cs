using System;
using System.Collections.Generic;

namespace AlgoDuck.Models;

public partial class Rarity
{
    public Guid RarityId { get; set; }

    public string RarityName { get; set; } = null!;

    public virtual ICollection<Item> Items { get; set; } = new List<Item>();
}
