using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlgoDuck.Models;

public partial class Rarity : IEntityTypeConfiguration<Rarity>
{
    public Guid RarityId { get; set; }

    public string RarityName { get; set; } = null!;

    public virtual ICollection<Item> Items { get; set; } = new List<Item>();
    public void Configure(EntityTypeBuilder<Rarity> builder)
    {
        builder.HasKey(e => e.RarityId).HasName("rarity_pk");

        builder.ToTable("rarity");

        builder.Property(e => e.RarityId)
            .ValueGeneratedNever()
            .HasColumnName("rarity_id");
        builder.Property(e => e.RarityName)
            .HasMaxLength(256)
            .HasColumnName("rarity_name");
    }
}
