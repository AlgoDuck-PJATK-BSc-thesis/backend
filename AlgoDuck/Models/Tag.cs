using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlgoDuck.Models;

public partial class Tag : IEntityTypeConfiguration<Tag>
{
    public Guid TagId { get; set; } = Guid.NewGuid();

    public string TagName { get; set; } = null!;

    public virtual ICollection<Problem> Problems { get; set; } = new List<Problem>();
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.HasKey(e => e.TagId).HasName("tag_pk");

        builder.ToTable("tag");

        builder.Property(e => e.TagId)
            .ValueGeneratedNever()
            .HasColumnName("tag_id");
        builder.Property(e => e.TagName)
            .HasMaxLength(256)
            .HasColumnName("tag_name");

        builder.HasMany(d => d.Problems).WithMany(p => p.Tags)
            .UsingEntity<Dictionary<string, object>>(
                "HasTag",
                r => r.HasOne<Problem>().WithMany()
                    .HasForeignKey("ProblemId")
                    .HasConstraintName("has_tag_problem_ref"),
                l => l.HasOne<Tag>().WithMany()
                    .HasForeignKey("TagId")
                    .HasConstraintName("has_tag_tag_ref"),
                j =>
                {
                    j.HasKey("TagId", "ProblemId").HasName("has_tag_pk");
                    j.ToTable("has_tag");
                    j.IndexerProperty<Guid>("TagId").HasColumnName("tag_id");
                    j.IndexerProperty<Guid>("ProblemId").HasColumnName("problem_id");
                });
    }
}
