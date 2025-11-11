using System;
using System.Collections.Generic;

namespace AlgoDuck.Models;

public partial class EditorLayout
{
    public Guid EditorLayoutId { get; set; }

    public Guid EditorThemeId { get; set; }

    public Guid UserConfigId { get; set; }

    public virtual EditorTheme EditorTheme { get; set; } = null!;

    public virtual UserConfig UserConfig { get; set; } = null!;
}
