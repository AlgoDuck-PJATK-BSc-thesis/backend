using System;
using System.Collections.Generic;

namespace AlgoDuck.Models;

public partial class EditorTheme
{
    public Guid EditorThemeId { get; set; }

    public string ThemeName { get; set; } = null!;

    public virtual ICollection<EditorLayout> EditorLayouts { get; set; } = new List<EditorLayout>();
}
