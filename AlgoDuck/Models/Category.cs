using System;
using System.Collections.Generic;

namespace AlgoDuck.Models;

public partial class Category
{
    public Guid CategoryId { get; set; }

    public string CategoryName { get; set; } = null!;

    public virtual ICollection<Problem> Problems { get; set; } = new List<Problem>();
}
