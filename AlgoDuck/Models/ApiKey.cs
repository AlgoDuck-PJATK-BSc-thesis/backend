using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlgoDuck.Models;

public partial class ApiKey : IEntityTypeConfiguration<ApiKey>
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Prefix { get; set; } = string.Empty;
    public string KeyHash { get; set; } = string.Empty;
    public string KeySalt { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }
    public virtual ApplicationUser User { get; set; } = null!;

    public void Configure(EntityTypeBuilder<ApiKey> builder)
    {
        builder.HasKey(e => e.Id).HasName("api_key_pk");

        builder.ToTable("api_key");

        builder.Property(e => e.Id)
            .HasColumnName("api_key_id");

        builder.Property(e => e.UserId)
            .HasColumnName("user_id");

        builder.Property(e => e.Name)
            .HasMaxLength(256)
            .HasColumnName("name");

        builder.Property(e => e.Prefix)
            .HasMaxLength(64)
            .HasColumnName("prefix");

        builder.Property(e => e.KeyHash)
            .HasMaxLength(512)
            .HasColumnName("key_hash");

        builder.Property(e => e.KeySalt)
            .HasMaxLength(512)
            .HasColumnName("key_salt");

        builder.Property(e => e.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .HasColumnName("created_at");

        builder.Property(e => e.ExpiresAt)
            .HasColumnType("timestamp with time zone")
            .HasColumnName("expires_at");

        builder.Property(e => e.RevokedAt)
            .HasColumnType("timestamp with time zone")
            .HasColumnName("revoked_at");

        builder.HasIndex(e => e.Prefix)
            .HasDatabaseName("api_key_prefix_idx");

        builder.HasOne(e => e.User)
            .WithMany(u => u.ApiKeys)
            .HasForeignKey(e => e.UserId)
            .HasConstraintName("api_key_application_user");
    }
}