using System;
using System.Collections.Generic;

namespace AlgoDuck.Models;

public partial class Status
{
    public Guid StatusId { get; set; }

    public string StatusName { get; set; } = null!;

    public virtual ICollection<UserSolution> UserSolutions { get; set; } = new List<UserSolution>();
}
