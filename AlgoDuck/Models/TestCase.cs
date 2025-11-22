using System;
using System.Collections.Generic;

namespace AlgoDuck.Models;

public partial class TestCase
{
    public Guid TestCaseId { get; set; }

    public string CallFunc { get; set; } = null!;

    public bool IsPublic { get; set; }

    public Guid ProblemProblemId { get; set; }

    public string Display { get; set; } = null!;

    public string DisplayRes { get; set; } = null!;

    public virtual Problem ProblemProblem { get; set; } = null!;

    public ICollection<PurchasedTestCase> PurchasedTestCases = new List<PurchasedTestCase>();

}
