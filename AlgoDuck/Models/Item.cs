using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlgoDuck.Models;

public abstract class Item : IEntityTypeConfiguration<Item>
{
    public Guid ItemId { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public int Price { get; set; }
    public bool Purchasable { get; set; }
    public Guid RarityId { get; set; }
    // public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    // public ApplicationUser? CreatedBy { get; set; }
    // public Guid CreatedById { get; set; }
    public virtual ICollection<Contest> Contests { get; set; } = new List<Contest>();
    public virtual ICollection<ItemOwnership> Purchases { get; set; } = new List<ItemOwnership>();
    public virtual Rarity Rarity { get; set; } = null!;

    public void Configure(EntityTypeBuilder<Item> builder)
    {
        builder.HasKey(e => e.ItemId).HasName("shop_pk");
        builder.ToTable("item");

        builder.HasDiscriminator<string>("type")
            .HasValue<DuckItem>("Duck")
            .HasValue<PlantItem>("Plant");

        builder.Property(e => e.ItemId)
            .ValueGeneratedNever()
            .HasColumnName("item_id");

        builder.Property(e => e.Description)
            .HasMaxLength(1024)
            .HasColumnName("description");

        builder.Property(e => e.Name)
            .HasMaxLength(256)
            .HasColumnName("name");

        // builder.Property(e => e.CreatedAt)
        //     .HasColumnName("created_at")
        //     .HasColumnType("timestamp with time zone");
        //
        // builder.Property(e => e.Price)
        //     .HasColumnName("price");
        //
        // builder.Property(e => e.Purchasable)
        //     .HasColumnName("purchasable");
        //
        // builder.Property(e => e.RarityId)
        //     .HasColumnName("rarity_id");

        // builder.HasOne(d => d.CreatedBy)
        //     .WithMany(c => c.CreatedItems)
        //     .HasForeignKey(d => d.CreatedById)
        //     .OnDelete(DeleteBehavior.ClientSetNull);

        builder.HasOne(d => d.Rarity).WithMany(p => p.Items)
            .HasForeignKey(d => d.RarityId)
            .HasConstraintName("item_rarity_ref");
    }
}

public class DuckItem : Item, IEntityTypeConfiguration<DuckItem>
{
    public void Configure(EntityTypeBuilder<DuckItem> builder)
    {
    }
}

public class PlantItem : Item, IEntityTypeConfiguration<PlantItem>
{
    public required byte Width { get; set; }
    public required byte Height { get; set; }

    public void Configure(EntityTypeBuilder<PlantItem> builder)
    {
        builder.Property(e => e.Width)
            .HasColumnName("plant_width");

        builder.Property(e => e.Height)
            .HasColumnName("plant_height");
    }
}