using System;
using System.Collections.Generic;

namespace AlgoDuck.Models;

public partial class UserConfig
{
    public Guid UserId { get; set; }

    public bool IsDarkMode { get; set; }

    public bool IsHighContrast { get; set; }

    public virtual ICollection<EditorLayout> EditorLayouts { get; set; } = new List<EditorLayout>();

    public virtual ApplicationUser User { get; set; } = null!;
}
