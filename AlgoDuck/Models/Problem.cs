using System;
using System.Collections.Generic;

namespace AlgoDuck.Models;

public partial class Problem
{
    public Guid ProblemId { get; set; }

    public string ProblemTitle { get; set; } = null!;

    public string Description { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public Guid CategoryId { get; set; }

    public Guid DifficultyId { get; set; }

    public virtual Category Category { get; set; } = null!;

    public virtual Difficulty Difficulty { get; set; } = null!;

    public virtual ICollection<TestCase> TestCases { get; set; } = new List<TestCase>();

    public virtual ICollection<UserSolution> UserSolutions { get; set; } = new List<UserSolution>();

    public virtual ICollection<Contest> Contests { get; set; } = new List<Contest>();

    public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();
}
