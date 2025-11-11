using System;
using System.Collections.Generic;

namespace AlgoDuck.Models;

public partial class Language
{
    public Guid LanguageId { get; set; }

    public string Name { get; set; } = null!;

    public string Version { get; set; } = null!;

    public virtual ICollection<UserSolution> UserSolutions { get; set; } = new List<UserSolution>();
}
