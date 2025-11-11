using System;
using System.Collections.Generic;

namespace AlgoDuck.Models;

public partial class Difficulty
{
    public Guid DifficultyId { get; set; }

    public string DifficultyName { get; set; } = null!;

    public virtual ICollection<Problem> Problems { get; set; } = new List<Problem>();
}
