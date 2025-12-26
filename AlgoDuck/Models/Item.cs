using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlgoDuck.Models;

public partial class Item : IEntityTypeConfiguration<Item>
{
    public Guid ItemId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int Price { get; set; }

    public bool Purchasable { get; set; }

    public Guid RarityId { get; set; }

    public virtual ICollection<Contest> Contests { get; set; } = new List<Contest>();

    public virtual ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();

    public virtual Rarity Rarity { get; set; } = null!;
    public void Configure(EntityTypeBuilder<Item> builder)
    {
        builder.HasKey(e => e.ItemId).HasName("shop_pk");

        builder.ToTable("item");

        builder.Property(e => e.ItemId)
            .ValueGeneratedNever()
            .HasColumnName("item_id");
        builder.Property(e => e.Description)
            .HasMaxLength(1024)
            .HasColumnName("description");
        builder.Property(e => e.Name)
            .HasMaxLength(256)
            .HasColumnName("name");
        builder.Property(e => e.Price).HasColumnName("price");
        builder.Property(e => e.Purchasable).HasColumnName("purchasable");
        builder.Property(e => e.RarityId).HasColumnName("rarity_id");

        builder.HasOne(d => d.Rarity).WithMany(p => p.Items)
            .HasForeignKey(d => d.RarityId)
            .HasConstraintName("item_rarity_ref");
    }
}
