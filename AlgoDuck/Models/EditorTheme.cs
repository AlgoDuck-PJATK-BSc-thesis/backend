using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlgoDuck.Models;

public partial class EditorTheme : IEntityTypeConfiguration<EditorTheme>
{
    public Guid EditorThemeId { get; set; } = Guid.NewGuid();

    public string ThemeName { get; set; } = null!;

    public virtual ICollection<UserConfig> UserConfigs { get; set; } = new List<UserConfig>();
    public void Configure(EntityTypeBuilder<EditorTheme> builder)
    {
        builder.HasKey(e => e.EditorThemeId)
            .HasName("editor_theme_pk");

        builder.ToTable("editor_theme");

        builder.Property(e => e.EditorThemeId)
            .ValueGeneratedNever()
            .HasColumnName("editor_theme_id");
        builder.Property(e => e.ThemeName)
            .HasMaxLength(256)
            .HasColumnName("theme_name");
    }
}
