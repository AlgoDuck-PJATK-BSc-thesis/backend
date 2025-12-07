
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlgoDuck.Models;

public partial class RefreshToken : IEntityTypeConfiguration<RefreshToken>
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public Guid SessionId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public string TokenSalt { get; set; } = string.Empty;
    public string? TokenPrefix { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }
    public Guid? ReplacedByTokenId { get; set; }
    public virtual RefreshToken? ReplacedByToken { get; set; }
    public virtual ICollection<RefreshToken> InverseReplacedByToken { get; set; } = new List<RefreshToken>();
    public virtual ApplicationUser User { get; set; } = null!;
    public virtual Session Session { get; set; } = null!;

    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasKey(e => e.Id).HasName("refresh_token_pk");

        builder.ToTable("refresh_token");

        builder.Property(e => e.Id)
            .HasColumnName("refresh_token_id");

        builder.Property(e => e.UserId)
            .HasColumnName("user_id");

        builder.Property(e => e.SessionId)
            .HasColumnName("session_id");

        builder.Property(e => e.TokenHash)
            .HasMaxLength(512)
            .HasColumnName("token_hash");

        builder.Property(e => e.TokenSalt)
            .HasMaxLength(512)
            .HasColumnName("token_salt");

        builder.Property(e => e.TokenPrefix)
            .HasMaxLength(64)
            .HasColumnName("token_prefix");

        builder.Property(e => e.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .HasColumnName("created_at");

        builder.Property(e => e.ExpiresAt)
            .HasColumnType("timestamp with time zone")
            .HasColumnName("expires_at");

        builder.Property(e => e.RevokedAt)
            .HasColumnType("timestamp with time zone")
            .HasColumnName("revoked_at");

        builder.Property(e => e.ReplacedByTokenId)
            .HasColumnName("replaced_by_token_id");

        builder.HasIndex(e => e.TokenPrefix)
            .HasDatabaseName("refresh_token_prefix_idx");

        builder.HasOne(e => e.ReplacedByToken)
            .WithMany(e => e.InverseReplacedByToken)
            .HasForeignKey(e => e.ReplacedByTokenId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("refresh_token_replaced_by_token");

        builder.HasOne(e => e.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(e => e.UserId)
            .HasConstraintName("refresh_token_application_user");

        builder.HasOne(e => e.Session)
            .WithMany()
            .HasForeignKey(e => e.SessionId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("refresh_token_session");
    }
}