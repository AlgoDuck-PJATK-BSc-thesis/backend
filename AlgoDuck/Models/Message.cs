using System;
using System.Collections.Generic;

namespace AlgoDuck.Models;

public partial class Message
{
    public Guid MessageId { get; set; }

    public string Message1 { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public Guid CohortId { get; set; }

    public Guid UserId { get; set; }

    public virtual Cohort Cohort { get; set; } = null!;

    public virtual ApplicationUser User { get; set; } = null!;
}
