using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlgoDuck.Models;

public partial class Status : IEntityTypeConfiguration<Status>
{
    public Guid StatusId { get; set; }

    public string StatusName { get; set; } = null!;

    public virtual ICollection<UserSolution> UserSolutions { get; set; } = new List<UserSolution>();
    public void Configure(EntityTypeBuilder<Status> builder)
    {
        builder.HasKey(e => e.StatusId).HasName("status_pk");

        builder.ToTable("status");

        builder.Property(e => e.StatusId)
            .ValueGeneratedNever()
            .HasColumnName("status_id");
        builder.Property(e => e.StatusName)
            .HasMaxLength(256)
            .HasColumnName("status_name");
    }
}
