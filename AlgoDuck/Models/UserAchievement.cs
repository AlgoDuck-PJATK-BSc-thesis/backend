using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlgoDuck.Models;

public partial class UserAchievement : IEntityTypeConfiguration<UserAchievement>
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
        builder.HasKey(e => e.Id).HasName("user_achievement_pk");

        builder.ToTable("user_achievement");

        builder.Property(e => e.Id)
            .ValueGeneratedNever()
            .HasColumnName("achievement_id");

        builder.Property(e => e.UserId)
            .HasColumnName("user_id");

        builder.Property(e => e.Code)
            .HasMaxLength(64)
            .HasColumnName("code");

        builder.Property(e => e.Name)
            .HasMaxLength(128)
            .HasColumnName("name");

        builder.Property(e => e.Description)
            .HasMaxLength(512)
            .HasColumnName("description");

        builder.Property(e => e.CurrentValue)
            .HasColumnName("current_value");

        builder.Property(e => e.TargetValue)
            .HasColumnName("target_value");

        builder.Property(e => e.IsCompleted)
            .HasColumnName("is_completed");

        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(e => e.CompletedAt)
            .HasColumnName("completed_at");

        builder.HasOne(e => e.User)
            .WithMany(u => u.UserAchievements)
            .HasForeignKey(e => e.UserId)
            .HasConstraintName("user_achievement_user_ref");
    }
}