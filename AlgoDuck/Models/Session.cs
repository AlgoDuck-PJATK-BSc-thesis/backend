using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlgoDuck.Models;

public partial class Session : IEntityTypeConfiguration<Session>
{
    public Guid SessionId { get; set; } = Guid.NewGuid();

    public string RefreshTokenHash { get; set; } = null!;

    public string RefreshTokenSalt { get; set; } = null!;
    
    public string? RefreshTokenPrefix { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime ExpiresAtUtc { get; set; }

    public DateTime? RevokedAtUtc { get; set; }

    public Guid? ReplacedBySessionId { get; set; }

    public Guid UserId { get; set; }

    public virtual ICollection<Session> InverseReplacedBySession { get; set; } = new List<Session>();

    public virtual Session? ReplacedBySession { get; set; }

    public virtual ApplicationUser User { get; set; } = null!;
    public void Configure(EntityTypeBuilder<Session> builder)
    {
        builder.HasKey(e => e.SessionId)
            .HasName("session_pk");

        builder.ToTable("session");

        builder.Property(e => e.SessionId)
            .ValueGeneratedNever()
            .HasColumnName("session_id");
        builder.Property(e => e.CreatedAtUtc)
            .HasColumnType("timestamp with time zone")
            .HasColumnName("created_at_utc");
        builder.Property(e => e.ExpiresAtUtc)
            .HasColumnType("timestamp with time zone")
            .HasColumnName("expires_at_utc");
        builder.Property(e => e.RefreshTokenHash)
            .HasMaxLength(512)
            .HasColumnName("refresh_token_hash");
        builder.Property(e => e.RefreshTokenSalt)
            .HasMaxLength(512)
            .HasColumnName("refresh_token_salt");

        builder.Property(e => e.RefreshTokenPrefix)
            .HasMaxLength(64)
            .HasColumnName("refresh_token_prefix");

        builder.Property(e => e.ReplacedBySessionId).HasColumnName("replaced_by_session_id");
        builder.Property(e => e.RevokedAtUtc)
            .HasColumnType("timestamp with time zone")
            .HasColumnName("revoked_at_utc");
        builder.Property(e => e.UserId).HasColumnName("user_id");

        builder.HasIndex(e => e.RefreshTokenPrefix)
            .HasDatabaseName("session_refresh_prefix_idx");

        builder.HasOne(d => d.ReplacedBySession).WithMany(p => p.InverseReplacedBySession)
            .HasForeignKey(d => d.ReplacedBySessionId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("session_replaced_by_session");

        builder.HasOne(d => d.User).WithMany(p => p.Sessions)
            .HasForeignKey(d => d.UserId)
            .HasConstraintName("session_application_user");
    }
}
