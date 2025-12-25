
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlgoDuck.Models;

public partial class ApplicationUser : IdentityUser<Guid>,  IEntityTypeConfiguration<ApplicationUser>
{
    public int Coins { get; set; }

    public int Experience { get; set; }

    public int AmountSolved { get; set; }
    
    public Guid? CohortId { get; set; }

    public virtual Cohort? Cohort { get; set; }

    public virtual ICollection<ApiKey> ApiKeys { get; set; } = new List<ApiKey>();
    
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    
    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public virtual ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();

    public virtual ICollection<Session> Sessions { get; set; } = new List<Session>();
    
    public virtual ICollection<UserAchievement> UserAchievements { get; set; } = new List<UserAchievement>();
    public virtual ICollection<AssistantChat> AssistantChats { get; set; } = new List<AssistantChat>();

    public virtual UserConfig? UserConfig { get; set; }

    public virtual ICollection<UserSolution> UserSolutions { get; set; } = new List<UserSolution>();
    public ICollection<PurchasedTestCase> PurchasedTestCases = new List<PurchasedTestCase>();

    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.HasKey(e => e.Id).HasName("application_user_pk");

        builder.ToTable("application_user");

        builder.Property(e => e.Id)
            .HasColumnName("user_id");

        builder.Property(e => e.AmountSolved).HasColumnName("amount_solved");
        builder.Property(e => e.CohortId).HasColumnName("cohort_id");
        builder.Property(e => e.Coins).HasColumnName("coins");
        builder.Property(e => e.Experience).HasColumnName("experience");

        builder.Property(e => e.UserName)
            .HasMaxLength(256)
            .HasColumnName("username");

        builder.Property(e => e.Email)
            .HasMaxLength(256)
            .HasColumnName("email");

        builder.Property(e => e.PasswordHash)
            .HasMaxLength(256)
            .HasColumnName("password_hash");

        builder.Property(e => e.SecurityStamp)
            .HasMaxLength(256)
            .HasColumnName("security_stamp");

        builder.HasOne(d => d.Cohort).WithMany(p => p.ApplicationUsers)
            .HasForeignKey(d => d.CohortId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("user_cohort_ref");
    }
}
