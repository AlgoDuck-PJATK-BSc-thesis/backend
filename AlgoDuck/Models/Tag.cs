using System;
using System.Collections.Generic;

namespace AlgoDuck.Models;

public partial class Tag
{
    public Guid TagId { get; set; }

    public string TagName { get; set; } = null!;

    public virtual ICollection<Problem> Problems { get; set; } = new List<Problem>();
}
