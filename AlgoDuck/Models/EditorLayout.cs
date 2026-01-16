using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlgoDuck.Models;

public partial class EditorLayout : IEntityTypeConfiguration<EditorLayout>
{
    public Guid EditorLayoutId { get; set; } = Guid.NewGuid();
    public required string LayoutName { get; set; }

    public List<OwnsLayout> OwnedBy { get; set; } = [];

    public void Configure(EntityTypeBuilder<EditorLayout> builder)
    {
        builder.HasKey(e => e.EditorLayoutId).HasName("editor_layout_pk");

        builder.ToTable("editor_layout");

        builder.Property(e => e.EditorLayoutId)
            .ValueGeneratedNever()
            .HasColumnName("editor_layout_id");
        
        builder.Property(e => e.LayoutName)
            .HasMaxLength(256)
            .HasColumnName("layout_name");
    }
}
