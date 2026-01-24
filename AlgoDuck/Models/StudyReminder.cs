using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlgoDuck.Models;

public sealed class StudyReminder : IEntityTypeConfiguration<StudyReminder>
{
    public int StudyReminderId { get; set; }

    public string Code { get; set; } = string.Empty;

    public int DayOfWeek { get; set; }

    public string Name { get; set; } = string.Empty;

    public ICollection<UserSetStudyReminder> UserSetStudyReminders { get; set; } = new List<UserSetStudyReminder>();

    public void Configure(EntityTypeBuilder<StudyReminder> builder)
    {
        builder.ToTable("study_reminder", t => t.HasCheckConstraint("study_reminder_day_of_week_chk", "day_of_week between 1 and 7"));

        builder.HasKey(e => e.StudyReminderId).HasName("study_reminder_pk");

        builder.Property(e => e.StudyReminderId)
            .HasColumnName("study_reminder_id")
            .HasColumnType("integer");

        builder.Property(e => e.Code)
            .HasMaxLength(3)
            .HasColumnName("code")
            .HasColumnType("character varying(3)")
            .IsRequired();

        builder.Property(e => e.DayOfWeek)
            .HasColumnName("day_of_week")
            .HasColumnType("integer")
            .IsRequired();

        builder.Property(e => e.Name)
            .HasMaxLength(16)
            .HasColumnName("name")
            .HasColumnType("character varying(16)")
            .IsRequired();

        builder.HasIndex(e => e.Code)
            .IsUnique()
            .HasDatabaseName("study_reminder_code_uq");

        builder.HasIndex(e => e.DayOfWeek)
            .IsUnique()
            .HasDatabaseName("study_reminder_day_of_week_uq");

        builder.HasData(
            new StudyReminder { StudyReminderId = 1, Code = "MON", DayOfWeek = 1, Name = "Monday" },
            new StudyReminder { StudyReminderId = 2, Code = "TUE", DayOfWeek = 2, Name = "Tuesday" },
            new StudyReminder { StudyReminderId = 3, Code = "WED", DayOfWeek = 3, Name = "Wednesday" },
            new StudyReminder { StudyReminderId = 4, Code = "THU", DayOfWeek = 4, Name = "Thursday" },
            new StudyReminder { StudyReminderId = 5, Code = "FRI", DayOfWeek = 5, Name = "Friday" },
            new StudyReminder { StudyReminderId = 6, Code = "SAT", DayOfWeek = 6, Name = "Saturday" },
            new StudyReminder { StudyReminderId = 7, Code = "SUN", DayOfWeek = 7, Name = "Sunday" }
        );
    }
}
