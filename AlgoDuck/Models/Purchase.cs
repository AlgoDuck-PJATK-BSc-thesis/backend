using System;
using System.Collections.Generic;

namespace AlgoDuck.Models;

public partial class Purchase
{
    public Guid ItemId { get; set; }

    public Guid UserId { get; set; }

    public bool Selected { get; set; }

    public virtual Item Item { get; set; } = null!;

    public virtual ApplicationUser User { get; set; } = null!;
}
