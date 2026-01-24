using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlgoDuck.Models;

public class UserConfig : IEntityTypeConfiguration<UserConfig>
{
    public Guid UserId { get; set; }

    public bool IsDarkMode { get; set; }

    public bool IsHighContrast { get; set; }

    public bool EmailNotificationsEnabled { get; set; }

    public DateTimeOffset? StudyReminderNextAtUtc { get; set; }

    public Guid EditorThemeId { get; set; } = Guid.Parse("276cc32e-a0bd-408e-b6f0-0f4e3ff80796");

    public virtual EditorTheme EditorTheme { get; set; } = null!;

    public int EditorFontSize { get; set; } = 11;

    public virtual ApplicationUser User { get; set; } = null!;

    public void Configure(EntityTypeBuilder<UserConfig> builder)
    {
        builder.HasKey(e => e.UserId).HasName("user_config_pk");

        builder.ToTable("user_config");

        builder.Property(e => e.EditorFontSize)
            .HasColumnName("editor_font_size");

        builder.Property(e => e.UserId)
            .ValueGeneratedNever()
            .HasColumnName("user_id");

        builder.Property(e => e.IsDarkMode).HasColumnName("is_dark_mode");
        builder.Property(e => e.IsHighContrast).HasColumnName("is_high_contrast");

        builder.Property(e => e.EmailNotificationsEnabled).HasColumnName("email_notifications_enabled");

        builder.Property(e => e.StudyReminderNextAtUtc)
            .HasColumnType("timestamp with time zone")
            .HasColumnName("study_reminder_next_at_utc");

        builder.HasOne(d => d.EditorTheme)
            .WithMany(e => e.UserConfigs)
            .HasForeignKey(d => d.EditorThemeId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(d => d.User).WithOne(p => p.UserConfig)
            .HasForeignKey<UserConfig>(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("user_config_application_user");
    }
}