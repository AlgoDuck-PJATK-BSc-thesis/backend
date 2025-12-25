using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlgoDuck.Models;

public partial class Difficulty : IEntityTypeConfiguration<Difficulty>
{
    public Guid DifficultyId { get; set; }

    public string DifficultyName { get; set; } = null!;

    public virtual ICollection<Problem> Problems { get; set; } = new List<Problem>();
    public void Configure(EntityTypeBuilder<Difficulty> builder)
    {
        builder.HasKey(e => e.DifficultyId).HasName("difficulty_pk");

        builder.ToTable("difficulty");

        builder.Property(e => e.DifficultyId)
            .ValueGeneratedNever()
            .HasColumnName("difficulty_id");
        builder.Property(e => e.DifficultyName)
            .HasMaxLength(256)
            .HasColumnName("difficulty_name");
    }
}
