using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlgoDuck.Models;

public abstract class ItemOwnership : IEntityTypeConfiguration<ItemOwnership>
{
    public Guid ItemId { get; set; }
    public Guid UserId { get; set; }

    public virtual Item Item { get; set; } = null!;
    public virtual ApplicationUser User { get; set; } = null!;
    public DateTime PurchasedAt { get; set; } = DateTime.UtcNow;

    public void Configure(EntityTypeBuilder<ItemOwnership> builder)
    {
        builder.HasKey(e => new { e.ItemId, e.UserId }).HasName("purchases_pk");
        builder.ToTable("purchase");

        builder.HasDiscriminator<string>("ownership_type")
            .HasValue<DuckOwnership>("Duck")
            .HasValue<PlantOwnership>("Plant");

        builder.Property(e => e.PurchasedAt)
            .HasColumnName("purchased_at");
        
        builder.Property(e => e.ItemId).HasColumnName("item_id");
        builder.Property(e => e.UserId).HasColumnName("user_id");

        builder.HasOne(d => d.Item).WithMany(p => p.Purchases)
            .HasForeignKey(d => d.ItemId)
            .HasConstraintName("item_purchase_ref");

        builder.HasOne(d => d.User).WithMany(p => p.Purchases)
            .HasForeignKey(d => d.UserId)
            .HasConstraintName("purchase_user_ref");
    }
}

public class DuckOwnership : ItemOwnership, IEntityTypeConfiguration<DuckOwnership>
{
    public bool SelectedAsAvatar { get; set; }
    public bool SelectedForPond { get; set; }

    public void Configure(EntityTypeBuilder<DuckOwnership> builder)
    {
        builder.Property(e => e.SelectedAsAvatar)
            .HasColumnName("duck_selected_as_avatar");
        
        builder.Property(e => e.SelectedForPond)
            .HasColumnName("duck_selected_for_pond");
    }
}

public class PlantOwnership : ItemOwnership, IEntityTypeConfiguration<PlantOwnership>
{
    public byte? GridX { get; set; }
    public byte? GridY { get; set; }

    public void Configure(EntityTypeBuilder<PlantOwnership> builder)
    {
        builder.Property(e => e.GridX)
            .HasColumnName("plant_grid_x");
        
        builder.Property(e => e.GridY)
            .HasColumnName("plant_grid_y");
    }
}