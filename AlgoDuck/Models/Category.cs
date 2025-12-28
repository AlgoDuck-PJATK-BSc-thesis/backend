using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlgoDuck.Models;

public partial class Category : IEntityTypeConfiguration<Category>
{
    public Guid CategoryId { get; set; }

    public string CategoryName { get; set; } = null!;

    public virtual ICollection<Problem> Problems { get; set; } = new List<Problem>();
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasKey(e => e.CategoryId).HasName("category_pk");

        builder.ToTable("category");

        builder.Property(e => e.CategoryId)
            .ValueGeneratedNever()
            .HasColumnName("category_id");
        builder.Property(e => e.CategoryName)
            .HasMaxLength(256)
            .HasColumnName("category_name");
    }
}
