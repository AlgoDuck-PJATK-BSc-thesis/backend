using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlgoDuck.Models;

public partial class Purchase : IEntityTypeConfiguration<Purchase>
{
    public Guid ItemId { get; set; }

    public Guid UserId { get; set; }

    public bool Selected { get; set; }

    public virtual Item Item { get; set; } = null!;

    public virtual ApplicationUser User { get; set; } = null!;
    public void Configure(EntityTypeBuilder<Purchase> builder)
    {
        builder.HasKey(e => new { e.ItemId, e.UserId }).HasName("purchases_pk");

        builder.ToTable("purchase");

        builder.Property(e => e.ItemId).HasColumnName("item_id");
        builder.Property(e => e.UserId).HasColumnName("user_id");
        builder.Property(e => e.Selected).HasColumnName("selected");

        builder.HasOne(d => d.Item).WithMany(p => p.Purchases)
            .HasForeignKey(d => d.ItemId)
            .HasConstraintName("item_purchase_ref");

        builder.HasOne(d => d.User).WithMany(p => p.Purchases)
            .HasForeignKey(d => d.UserId)
            .HasConstraintName("purchase_user_ref");
    }
}
