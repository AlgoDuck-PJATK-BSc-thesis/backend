using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlgoDuck.Models;

public partial class Message : IEntityTypeConfiguration<Message>
{
    public Guid MessageId { get; set; }

    public string Message1 { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public Guid CohortId { get; set; }

    public Guid UserId { get; set; }

    public virtual Cohort Cohort { get; set; } = null!;

    public virtual ApplicationUser User { get; set; } = null!;
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.HasKey(e => e.MessageId).HasName("message_pk");

        builder.ToTable("message");

        builder.Property(e => e.MessageId)
            .ValueGeneratedNever()
            .HasColumnName("message_id");
        builder.Property(e => e.CohortId).HasColumnName("cohort_id");
        builder.Property(e => e.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .HasColumnType("timestamp with time zone")
            .HasColumnName("created_at");
        builder.Property(e => e.Message1)
            .HasMaxLength(256)
            .HasColumnName("message");
        builder.Property(e => e.UserId).HasColumnName("user_id");

        builder.HasOne(d => d.Cohort).WithMany(p => p.Messages)
            .HasForeignKey(d => d.CohortId)
            .HasConstraintName("message_cohort_ref");

        builder.HasOne(d => d.User).WithMany(p => p.Messages)
            .HasForeignKey(d => d.UserId)
            .HasConstraintName("message_user_ref");
    }
}
