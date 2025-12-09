using AlgoDuck.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.DAL;

public abstract partial class BaseDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public BaseDbContext()
    {
    }

    public BaseDbContext(DbContextOptions options)
        : base(options)
    {
    }
    

    public virtual DbSet<ApplicationUser> ApplicationUsers { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Cohort> Cohorts { get; set; }

    public virtual DbSet<Contest> Contests { get; set; }

    public virtual DbSet<Difficulty> Difficulties { get; set; }

    public virtual DbSet<EditorLayout> EditorLayouts { get; set; }

    public virtual DbSet<EditorTheme> EditorThemes { get; set; }

    public virtual DbSet<Item> Items { get; set; }


    public virtual DbSet<Message> Messages { get; set; }

    public virtual DbSet<Problem> Problems { get; set; }

    public virtual DbSet<Purchase> Purchases { get; set; }

    public virtual DbSet<Rarity> Rarities { get; set; }

    public virtual DbSet<Session> Sessions { get; set; }

    public virtual DbSet<Status> Statuses { get; set; }

    public virtual DbSet<Tag> Tags { get; set; }

    public virtual DbSet<TestCase> TestCases { get; set; }

    public virtual DbSet<UserConfig> UserConfigs { get; set; }

    public virtual DbSet<UserSolution> UserSolutions { get; set; }
    public virtual DbSet<PurchasedTestCase> PurchasedTestCases { get; set; }
    public virtual DbSet<TestingResult> TestingResults { get; set; }
    public virtual DbSet<UserAchievement> UserAchievements { get; set; }
    public virtual DbSet<AssistantChat> AssistantChats { get; set; }
    public virtual DbSet<AssistanceMessage> AssistanceMessages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BaseDbContext).Assembly);
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}