using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlgoDuck.Models;

public class Achievement : IEntityTypeConfiguration<Achievement>
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int TargetValue { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public virtual ICollection<UserAchievement> UserAchievements { get; set; } = new List<UserAchievement>();

    public void Configure(EntityTypeBuilder<Achievement> builder)
    {
        builder.ToTable("achievement");

        builder.HasKey(e => e.Code).HasName("achievement_pk");

        builder.Property(e => e.Code)
            .HasMaxLength(64)
            .HasColumnName("code")
            .IsRequired();

        builder.Property(e => e.Name)
            .HasMaxLength(128)
            .HasColumnName("name")
            .IsRequired();

        builder.Property(e => e.Description)
            .HasMaxLength(512)
            .HasColumnName("description")
            .IsRequired();

        builder.Property(e => e.TargetValue)
            .HasColumnName("target_value")
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();
    }
}