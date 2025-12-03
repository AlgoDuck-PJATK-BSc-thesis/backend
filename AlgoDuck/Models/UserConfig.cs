using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlgoDuck.Models;

public partial class UserConfig : IEntityTypeConfiguration<UserConfig>
{
    public Guid UserId { get; set; }

    public bool IsDarkMode { get; set; }

    public bool IsHighContrast { get; set; }
    
    public string Language { get; set; } = "en";
    public string AvatarKey { get; set; } = string.Empty;

    public virtual ICollection<EditorLayout> EditorLayouts { get; set; } = new List<EditorLayout>();

    public virtual ApplicationUser User { get; set; } = null!;
    public void Configure(EntityTypeBuilder<UserConfig> builder)
    {
        builder.HasKey(e => e.UserId).HasName("user_config_pk");

        builder.ToTable("user_config");

        builder.Property(e => e.UserId)
            .ValueGeneratedNever()
            .HasColumnName("user_id");
        builder.Property(e => e.IsDarkMode).HasColumnName("is_dark_mode");
        builder.Property(e => e.IsHighContrast).HasColumnName("is_high_contrast");

        builder.HasOne(d => d.User).WithOne(p => p.UserConfig)
            .HasForeignKey<UserConfig>(d => d.UserId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("user_config_application_user");
    }
}
