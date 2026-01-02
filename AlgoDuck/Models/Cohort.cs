using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlgoDuck.Models;

public class Cohort : IEntityTypeConfiguration<Cohort>
{
    public Guid CohortId { get; set; }

    public string Name { get; set; } = null!;

    public string JoinCode { get; set; } = null!;

    public virtual ApplicationUser? CreatedByUser { get; set; }
    
    public Guid? CreatedByUserId { get; set; }

    public string? CreatedByUserLabel { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime? EmptiedAt { get; set; }

    public virtual ICollection<ApplicationUser> ApplicationUsers { get; set; } = new List<ApplicationUser>();

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public void Configure(EntityTypeBuilder<Cohort> builder)
    {
        builder.HasKey(e => e.CohortId).HasName("cohort_pk");

        builder.ToTable("cohort");

        builder.Property(e => e.CohortId)
            .ValueGeneratedNever()
            .HasColumnName("cohort_id");

        builder.Property(e => e.Name)
            .HasMaxLength(256)
            .HasColumnName("name");

        builder.Property(e => e.JoinCode)
            .HasMaxLength(16)
            .HasColumnName("join_code");

        builder.HasIndex(e => e.JoinCode)
            .IsUnique()
            .HasDatabaseName("cohort_join_code_uq");

        builder.Property(e => e.IsActive)
            .HasColumnName("is_active");

        builder.Property(e => e.CreatedByUserId)
            .HasColumnName("created_by_user_id");

        builder.Property(e => e.CreatedByUserLabel)
            .HasMaxLength(256)
            .HasColumnName("created_by_user_label");

        builder.Property(e => e.EmptiedAt)
            .HasColumnType("timestamp with time zone")
            .HasColumnName("emptied_at");

        builder.HasOne(e => e.CreatedByUser)
            .WithMany()
            .HasForeignKey(e => e.CreatedByUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
