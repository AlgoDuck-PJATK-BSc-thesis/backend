using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlgoDuck.Models;

public partial class EditorLayout : IEntityTypeConfiguration<EditorLayout>
{
    public Guid EditorLayoutId { get; set; }

    public Guid EditorThemeId { get; set; }

    public Guid UserConfigId { get; set; }

    public virtual EditorTheme EditorTheme { get; set; } = null!;

    public virtual UserConfig UserConfig { get; set; } = null!;
    public void Configure(EntityTypeBuilder<EditorLayout> builder)
    {
        builder.HasKey(e => e.EditorLayoutId).HasName("editor_layout_pk");

        builder.ToTable("editor_layout");

        builder.Property(e => e.EditorLayoutId)
            .ValueGeneratedNever()
            .HasColumnName("editor_layout_id");
        builder.Property(e => e.EditorThemeId).HasColumnName("editor_theme_id");
        builder.Property(e => e.UserConfigId).HasColumnName("user_config_id");

        builder.HasOne(d => d.EditorTheme).WithMany(p => p.EditorLayouts)
            .HasForeignKey(d => d.EditorThemeId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("editor_layout_editor_theme");

        builder.HasOne(d => d.UserConfig).WithMany(p => p.EditorLayouts)
            .HasForeignKey(d => d.UserConfigId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("editor_layout_user_config");
    }
}
