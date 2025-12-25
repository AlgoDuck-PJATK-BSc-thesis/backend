using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlgoDuck.Models;

public partial class Cohort  : IEntityTypeConfiguration<Cohort>
{
    public Guid CohortId { get; set; }

    public string Name { get; set; } = null!;

    public ApplicationUser? CreatedByUser { get; set; }
    public Guid CreatedByUserId { get; set; }

    public bool IsActive { get; set; } = true;

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

        builder.Property(e => e.IsActive);
            
        builder.Property(e => e.CreatedByUserId)
            .HasColumnName("created_by_user_id");
    
        builder.HasOne(e => e.CreatedByUser)
            .WithMany() 
            .HasForeignKey(e => e.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
