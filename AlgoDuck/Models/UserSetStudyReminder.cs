using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlgoDuck.Models;

public sealed class UserSetStudyReminder : IEntityTypeConfiguration<UserSetStudyReminder>
{
    public Guid UserId { get; set; }

    public int StudyReminderId { get; set; }

    public int? Hour { get; set; }

    public ApplicationUser User { get; set; } = null!;

    public StudyReminder StudyReminder { get; set; } = null!;

    public void Configure(EntityTypeBuilder<UserSetStudyReminder> builder)
    {
        builder.ToTable("user_set_reminder", t => t.HasCheckConstraint("user_set_reminder_hour_chk", "hour is null or (hour between 0 and 23)"));

        builder.HasKey(e => new { e.UserId, e.StudyReminderId }).HasName("user_set_reminder_pk");

        builder.Property(e => e.UserId)
            .HasColumnName("user_id")
            .HasColumnType("uuid");

        builder.Property(e => e.StudyReminderId)
            .HasColumnName("study_reminder_id")
            .HasColumnType("integer");

        builder.Property(e => e.Hour)
            .HasColumnName("hour")
            .HasColumnType("integer");

        builder.HasOne(e => e.User)
            .WithMany(u => u.UserSetStudyReminders)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("user_set_reminder_user_ref");

        builder.HasOne(e => e.StudyReminder)
            .WithMany(r => r.UserSetStudyReminders)
            .HasForeignKey(e => e.StudyReminderId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("user_set_reminder_study_reminder_ref");
    }
}