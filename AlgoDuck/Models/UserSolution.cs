using System;
using System.Collections.Generic;

namespace AlgoDuck.Models;

public partial class UserSolution
{
    public Guid SolutionId { get; set; }

    public int Stars { get; set; }

    public DateTime CodeRuntimeSubmitted { get; set; }

    public Guid UserId { get; set; }

    public Guid ProblemId { get; set; }

    public Guid StatusId { get; set; }

    public Guid LanguageId { get; set; }

    public virtual Language Language { get; set; } = null!;

    public virtual Problem Problem { get; set; } = null!;

    public virtual Status Status { get; set; } = null!;

    public virtual ApplicationUser User { get; set; } = null!;
}
