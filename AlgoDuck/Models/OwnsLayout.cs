using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlgoDuck.Models;

public class OwnsLayout : IEntityTypeConfiguration<OwnsLayout>
{
    public ApplicationUser User { get; set; } = null!;
    public required Guid UserId { get; set; }
    public EditorLayout Layout { get; set; } = null!;
    public required Guid LayoutId { get; set; }
    public bool IsSelected { get; set; } = false;
    
    public void Configure(EntityTypeBuilder<OwnsLayout> builder)
    {
        builder.ToTable("owns_layout");
        
        builder.HasKey(e => new { e.LayoutId, e.UserId });
        
        builder.Property(e => e.IsSelected)
            .HasColumnName("is_selected");
        
        builder.HasOne(e => e.User)
            .WithMany(e => e.EditorLayouts)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(e => e.Layout)
            .WithMany(e => e.OwnedBy)
            .HasForeignKey(e => e.LayoutId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}