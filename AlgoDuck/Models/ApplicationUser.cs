using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlgoDuck.Models;

public sealed class ApplicationUser : IdentityUser<Guid>, IEntityTypeConfiguration<ApplicationUser>
{
    public ApplicationUser()
    {
    }

    public ApplicationUser EnrichWithDefaults()
    {
        if (Id == Guid.Empty)
        {
            Id = Guid.NewGuid();
        }

        if (EditorLayouts is null || EditorLayouts.Count == 0)
        {
            EditorLayouts = new List<OwnsLayout>
            {
                new()
                {
                    UserId = Id,
                    LayoutId = Guid.Parse("7d2e1c42-f7da-4261-a8c1-42826d976116"),
                    IsSelected = true
                },
                new()
                {
                    UserId = Id,
                    LayoutId = Guid.Parse("3922523c-7c2f-4a9a-9f43-9fc5b8698972")
                },
                new()
                {
                    UserId = Id,
                    LayoutId = Guid.Parse("b9647438-6bec-45e6-a942-207dc40be273")
                }
            };
        }

        if (UserConfig is null)
        {
            UserConfig = new UserConfig
            {
                UserId = Id,
                IsDarkMode = true,
                IsHighContrast = false,
                EmailNotificationsEnabled = false,
                EditorFontSize = 11
            };
        }

        return this;
    }

    public int Coins { get; set; }

    public int Experience { get; set; }

    public int AmountSolved { get; set; }
    
    public Guid? CohortId { get; set; }

    public DateTime? CohortJoinedAt { get; set; }

    public Cohort? Cohort { get; set; }

    public ICollection<ApiKey> ApiKeys { get; set; } = new List<ApiKey>();
    
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    
    public ICollection<Message> Messages { get; set; } = new List<Message>();

    public ICollection<ItemOwnership> Purchases { get; set; } = new List<ItemOwnership>();

    public ICollection<Session> Sessions { get; set; } = new List<Session>();

    public ICollection<UserAchievement> UserAchievements { get; set; } = new List<UserAchievement>();

    public ICollection<AssistantChat> AssistantChats { get; set; } = new List<AssistantChat>();

    public UserConfig? UserConfig { get; set; }

    public ICollection<UserSolution> UserSolutions { get; set; } = new List<UserSolution>();

    public ICollection<PurchasedTestCase> PurchasedTestCases { get; set; } = new List<PurchasedTestCase>();

    public ICollection<UserSolutionSnapshot> UserSolutionSnapshots { get; set; } = new List<UserSolutionSnapshot>();
    public ICollection<CodeExecutionStatistics> CodeExecutionStatistics { get; set; } = new List<CodeExecutionStatistics>();
    public ICollection<Item> CreatedItems { get; set; } = new List<Item>();
    public ICollection<OwnsLayout> EditorLayouts { get; set; } = new List<OwnsLayout>();
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.HasKey(e => e.Id).HasName("application_user_pk");

        builder.ToTable("application_user");

        builder.Property(e => e.Id)
            .HasColumnName("user_id");

        builder.Property(e => e.AmountSolved).HasColumnName("amount_solved");
        builder.Property(e => e.CohortId).HasColumnName("cohort_id");

        builder.Property(e => e.CohortJoinedAt)
            .HasColumnType("timestamp with time zone")
            .HasColumnName("cohort_joined_at");

        builder.Property(e => e.Coins).HasColumnName("coins");
        builder.Property(e => e.Experience).HasColumnName("experience");

        builder.Property(e => e.UserName)
            .HasMaxLength(256)
            .HasColumnName("username");

        builder.Property(e => e.Email)
            .HasMaxLength(256)
            .HasColumnName("email");

        builder.Property(e => e.PasswordHash)
            .HasColumnName("password_hash");

        builder.Property(e => e.SecurityStamp)
            .HasMaxLength(256)
            .HasColumnName("security_stamp");

        builder.HasOne(d => d.Cohort).WithMany(p => p.ApplicationUsers)
            .HasForeignKey(d => d.CohortId)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("user_cohort_ref");
    }
}
