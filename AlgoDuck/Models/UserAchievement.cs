using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlgoDuck.Models;

public class UserAchievement : IEntityTypeConfiguration<UserAchievement>
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int CurrentValue { get; set; }
    public int TargetValue { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public virtual ApplicationUser User { get; set; } = null!;

    public void Configure(EntityTypeBuilder<UserAchievement> builder)
    {
        builder.ToTable("user_achievement");

        builder.HasKey(e => e.Id).HasName("user_achievement_pk");

        builder.Property(e => e.Id)
            .ValueGeneratedNever()
            .HasColumnName("achievement_id")
            .IsRequired();

        builder.Property(e => e.UserId)
            .HasColumnName("user_id")
            .IsRequired();

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

        builder.Property(e => e.CurrentValue)
            .HasColumnName("current_value")
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(e => e.TargetValue)
            .HasColumnName("target_value")
            .IsRequired();

        builder.Property(e => e.IsCompleted)
            .HasColumnName("is_completed")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(e => e.CompletedAt)
            .HasColumnName("completed_at");

        builder.HasOne(e => e.User)
            .WithMany(u => u.UserAchievements)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("user_achievement_user_ref");

        builder.HasIndex(e => new { e.UserId, e.Code })
            .IsUnique()
            .HasDatabaseName("user_achievement_user_code_uq");

        builder.HasIndex(e => e.UserId)
            .HasDatabaseName("user_achievement_user_id_ix");

        builder.HasIndex(e => e.IsCompleted)
            .HasDatabaseName("user_achievement_is_completed_ix");
    }
}
