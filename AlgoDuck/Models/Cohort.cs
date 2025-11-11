using System;
using System.Collections.Generic;

namespace AlgoDuck.Models;

public partial class Cohort
{
    public Guid CohortId { get; set; }

    public string Name { get; set; } = null!;

    public ApplicationUser? CreatedByUser { get; set; }
    public Guid CreatedByUserId { get; set; }

    public bool IsActive { get; set; } = true;

    public virtual ICollection<ApplicationUser> ApplicationUsers { get; set; } = new List<ApplicationUser>();

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
}
