using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Models;

public partial class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public ApplicationDbContext()
    {
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
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

    public virtual DbSet<Language> Languages { get; set; }

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

//     protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
// #warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
//         => optionsBuilder.UseNpgsql("Host=localhost;Database=application;Username=application;Password=asd123");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<PurchasedTestCase>(entity =>
        {
            entity.HasKey(e => new { e.TestCaseId, e.UserId });

            entity.Property(e => e.TestCaseId).HasColumnName("test_case_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(e => e.TestCase)
                .WithMany(e => e.PurchasedTestCases)
                .HasForeignKey(e => e.TestCaseId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(e => e.User)
                .WithMany(e => e.PurchasedTestCases)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<TestingResult>(entity =>
        {
            entity.HasKey(e => new { e.ExerciseId, e.UserSolutionId });
            entity.Property(e => e.ExerciseId).HasColumnName("exercise_id");
            entity.Property(e => e.UserSolutionId).HasColumnName("solution_id");
            entity.Property(e => e.IsPassed).HasColumnName("is_passed");

            entity.HasOne(e => e.Problem)
                .WithMany(e => e.TestingResults)
                .HasForeignKey(e => e.ExerciseId)
                .OnDelete(DeleteBehavior.ClientSetNull);
            
            entity.HasOne(e => e.UserSolution)
                .WithMany(e => e.TestingResults)
                .HasForeignKey(e => e.UserSolutionId)
                .OnDelete(DeleteBehavior.ClientSetNull);
            
            
        });
        
        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("application_user_pk");

            entity.ToTable("application_user");

            entity.Property(e => e.Id)
                .HasColumnName("user_id");

            entity.Property(e => e.AmountSolved).HasColumnName("amount_solved");
            entity.Property(e => e.CohortId).HasColumnName("cohort_id");
            entity.Property(e => e.Coins).HasColumnName("coins");
            entity.Property(e => e.Experience).HasColumnName("experience");

            entity.Property(e => e.UserName)
                .HasMaxLength(256)
                .HasColumnName("username");

            entity.Property(e => e.Email)
                .HasMaxLength(256)
                .HasColumnName("email");

            entity.Property(e => e.PasswordHash)
                .HasMaxLength(256)
                .HasColumnName("password_hash");

            entity.Property(e => e.SecurityStamp)
                .HasMaxLength(256)
                .HasColumnName("security_stamp");

            entity.HasOne(d => d.Cohort).WithMany(p => p.ApplicationUsers)
                .HasForeignKey(d => d.CohortId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("user_cohort_ref");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("category_pk");

            entity.ToTable("category");

            entity.Property(e => e.CategoryId)
                .ValueGeneratedNever()
                .HasColumnName("category_id");
            entity.Property(e => e.CategoryName)
                .HasMaxLength(256)
                .HasColumnName("category_name");
        });

        modelBuilder.Entity<Cohort>(entity =>
        {
            entity.HasKey(e => e.CohortId).HasName("cohort_pk");

            entity.ToTable("cohort");

            entity.Property(e => e.CohortId)
                .ValueGeneratedNever()
                .HasColumnName("cohort_id");
            entity.Property(e => e.Name)
                .HasMaxLength(256)
                .HasColumnName("name");

            entity.Property(e => e.IsActive);
            
            entity.Property(e => e.CreatedByUserId)
                .HasColumnName("created_by_user_id");
    
            entity.HasOne(e => e.CreatedByUser)
                .WithMany() 
                .HasForeignKey(e => e.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
            
        });

        modelBuilder.Entity<Contest>(entity =>
        {
            entity.HasKey(e => e.ContestId).HasName("contest_pk");

            entity.ToTable("contest");

            entity.Property(e => e.ContestId)
                .ValueGeneratedNever()
                .HasColumnName("contest_id");
            entity.Property(e => e.ContestDescription)
                .HasMaxLength(2048)
                .HasColumnName("contest_description");
            entity.Property(e => e.ContestEndDate)
                .HasColumnType("timestamp with time zone")
                .HasColumnName("contest_end_date");
            entity.Property(e => e.ContestName)
                .HasMaxLength(256)
                .HasColumnName("contest_name");
            entity.Property(e => e.ContestStartDate)
                .HasColumnType("timestamp with time zone")
                .HasColumnName("contest_start_date");
            entity.Property(e => e.ItemId).HasColumnName("item_id");

            entity.HasOne(d => d.Item).WithMany(p => p.Contests)
                .HasForeignKey(d => d.ItemId)
                .HasConstraintName("contest_item_ref");
        });

        modelBuilder.Entity<Difficulty>(entity =>
        {
            entity.HasKey(e => e.DifficultyId).HasName("difficulty_pk");

            entity.ToTable("difficulty");

            entity.Property(e => e.DifficultyId)
                .ValueGeneratedNever()
                .HasColumnName("difficulty_id");
            entity.Property(e => e.DifficultyName)
                .HasMaxLength(256)
                .HasColumnName("difficulty_name");
        });

        modelBuilder.Entity<EditorLayout>(entity =>
        {
            entity.HasKey(e => e.EditorLayoutId).HasName("editor_layout_pk");

            entity.ToTable("editor_layout");

            entity.Property(e => e.EditorLayoutId)
                .ValueGeneratedNever()
                .HasColumnName("editor_layout_id");
            entity.Property(e => e.EditorThemeId).HasColumnName("editor_theme_id");
            entity.Property(e => e.UserConfigId).HasColumnName("user_config_id");

            entity.HasOne(d => d.EditorTheme).WithMany(p => p.EditorLayouts)
                .HasForeignKey(d => d.EditorThemeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("editor_layout_editor_theme");

            entity.HasOne(d => d.UserConfig).WithMany(p => p.EditorLayouts)
                .HasForeignKey(d => d.UserConfigId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("editor_layout_user_config");
        });

        modelBuilder.Entity<EditorTheme>(entity =>
        {
            entity.HasKey(e => e.EditorThemeId).HasName("editor_theme_pk");

            entity.ToTable("editor_theme");

            entity.Property(e => e.EditorThemeId)
                .ValueGeneratedNever()
                .HasColumnName("editor_theme_id");
            entity.Property(e => e.ThemeName)
                .HasMaxLength(256)
                .HasColumnName("theme_name");
        });

        modelBuilder.Entity<Item>(entity =>
        {
            entity.HasKey(e => e.ItemId).HasName("shop_pk");

            entity.ToTable("item");

            entity.Property(e => e.ItemId)
                .ValueGeneratedNever()
                .HasColumnName("item_id");
            entity.Property(e => e.Description)
                .HasMaxLength(1024)
                .HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(256)
                .HasColumnName("name");
            entity.Property(e => e.Price).HasColumnName("price");
            entity.Property(e => e.Purchasable).HasColumnName("purchasable");
            entity.Property(e => e.RarityId).HasColumnName("rarity_id");

            entity.HasOne(d => d.Rarity).WithMany(p => p.Items)
                .HasForeignKey(d => d.RarityId)
                .HasConstraintName("item_rarity_ref");
        });

        modelBuilder.Entity<Language>(entity =>
        {
            entity.HasKey(e => e.LanguageId).HasName("language_pk");

            entity.ToTable("language");

            entity.Property(e => e.LanguageId)
                .ValueGeneratedNever()
                .HasColumnName("language_id");
            entity.Property(e => e.Name)
                .HasMaxLength(256)
                .HasColumnName("name");
            entity.Property(e => e.Version)
                .HasMaxLength(256)
                .HasColumnName("version");
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.MessageId).HasName("message_pk");

            entity.ToTable("message");

            entity.Property(e => e.MessageId)
                .ValueGeneratedNever()
                .HasColumnName("message_id");
            entity.Property(e => e.CohortId).HasColumnName("cohort_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp with time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Message1)
                .HasMaxLength(256)
                .HasColumnName("message");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Cohort).WithMany(p => p.Messages)
                .HasForeignKey(d => d.CohortId)
                .HasConstraintName("message_cohort_ref");

            entity.HasOne(d => d.User).WithMany(p => p.Messages)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("message_user_ref");
        });

        modelBuilder.Entity<Problem>(entity =>
        {
            entity.HasKey(e => e.ProblemId).HasName("problem_pk");

            entity.ToTable("problem");

            entity.Property(e => e.ProblemId)
                .ValueGeneratedNever()
                .HasColumnName("problem_id");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp with time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Description)
                .HasMaxLength(1024)
                .HasColumnName("description");
            entity.Property(e => e.DifficultyId).HasColumnName("difficulty_id");
            entity.Property(e => e.ProblemTitle)
                .HasMaxLength(256)
                .HasColumnName("problem_title");

            entity.HasOne(d => d.Category).WithMany(p => p.Problems)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("problem_category_ref");

            entity.HasOne(d => d.Difficulty).WithMany(p => p.Problems)
                .HasForeignKey(d => d.DifficultyId)
                .HasConstraintName("problem_difficulty_ref");

            entity.HasMany(d => d.Contests).WithMany(p => p.Problems)
                .UsingEntity<Dictionary<string, object>>(
                    "ContestProblem",
                    r => r.HasOne<Contest>().WithMany()
                        .HasForeignKey("ContestId")
                        .HasConstraintName("contest_problem_contest_ref"),
                    l => l.HasOne<Problem>().WithMany()
                        .HasForeignKey("ProblemId")
                        .HasConstraintName("contest_problem_problem_ref"),
                    j =>
                    {
                        j.HasKey("ProblemId", "ContestId").HasName("contest_problem_pk");
                        j.ToTable("contest_problem");
                        j.IndexerProperty<Guid>("ProblemId").HasColumnName("problem_id");
                        j.IndexerProperty<Guid>("ContestId").HasColumnName("contest_id");
                    });
        });

        modelBuilder.Entity<Purchase>(entity =>
        {
            entity.HasKey(e => new { e.ItemId, e.UserId }).HasName("purchases_pk");

            entity.ToTable("purchase");

            entity.Property(e => e.ItemId).HasColumnName("item_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Selected).HasColumnName("selected");

            entity.HasOne(d => d.Item).WithMany(p => p.Purchases)
                .HasForeignKey(d => d.ItemId)
                .HasConstraintName("item_purchase_ref");

            entity.HasOne(d => d.User).WithMany(p => p.Purchases)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("purchase_user_ref");
        });

        modelBuilder.Entity<Rarity>(entity =>
        {
            entity.HasKey(e => e.RarityId).HasName("rarity_pk");

            entity.ToTable("rarity");

            entity.Property(e => e.RarityId)
                .ValueGeneratedNever()
                .HasColumnName("rarity_id");
            entity.Property(e => e.RarityName)
                .HasMaxLength(256)
                .HasColumnName("rarity_name");
        });

        modelBuilder.Entity<Session>(entity =>
        {
            entity.HasKey(e => e.SessionId)
                .HasName("session_pk");

            entity.ToTable("session");

            entity.Property(e => e.SessionId)
                .ValueGeneratedOnAdd()
                .HasColumnName("session_id");
            entity.Property(e => e.CreatedAtUtc)
                .HasColumnType("timestamp with time zone")
                .HasColumnName("created_at_utc");
            entity.Property(e => e.ExpiresAtUtc)
                .HasColumnType("timestamp with time zone")
                .HasColumnName("expires_at_utc");
            entity.Property(e => e.RefreshTokenHash)
                .HasMaxLength(512)
                .HasColumnName("refresh_token_hash");
            entity.Property(e => e.RefreshTokenSalt)
                .HasMaxLength(512)
                .HasColumnName("refresh_token_salt");

            entity.Property(e => e.RefreshTokenPrefix)
                .HasMaxLength(64)
                .HasColumnName("refresh_token_prefix");

            entity.Property(e => e.ReplacedBySessionId).HasColumnName("replaced_by_session_id");
            entity.Property(e => e.RevokedAtUtc)
                .HasColumnType("timestamp with time zone")
                .HasColumnName("revoked_at_utc");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasIndex(e => e.RefreshTokenPrefix)
                .HasDatabaseName("session_refresh_prefix_idx");

            entity.HasOne(d => d.ReplacedBySession).WithMany(p => p.InverseReplacedBySession)
                .HasForeignKey(d => d.ReplacedBySessionId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("session_replaced_by_session");

            entity.HasOne(d => d.User).WithMany(p => p.Sessions)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("session_application_user");
        });

        modelBuilder.Entity<Status>(entity =>
        {
            entity.HasKey(e => e.StatusId).HasName("status_pk");

            entity.ToTable("status");

            entity.Property(e => e.StatusId)
                .ValueGeneratedNever()
                .HasColumnName("status_id");
            entity.Property(e => e.StatusName)
                .HasMaxLength(256)
                .HasColumnName("status_name");
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(e => e.TagId).HasName("tag_pk");

            entity.ToTable("tag");

            entity.Property(e => e.TagId)
                .ValueGeneratedNever()
                .HasColumnName("tag_id");
            entity.Property(e => e.TagName)
                .HasMaxLength(256)
                .HasColumnName("tag_name");

            entity.HasMany(d => d.Problems).WithMany(p => p.Tags)
                .UsingEntity<Dictionary<string, object>>(
                    "HasTag",
                    r => r.HasOne<Problem>().WithMany()
                        .HasForeignKey("ProblemId")
                        .HasConstraintName("has_tag_problem_ref"),
                    l => l.HasOne<Tag>().WithMany()
                        .HasForeignKey("TagId")
                        .HasConstraintName("has_tag_tag_ref"),
                    j =>
                    {
                        j.HasKey("TagId", "ProblemId").HasName("has_tag_pk");
                        j.ToTable("has_tag");
                        j.IndexerProperty<Guid>("TagId").HasColumnName("tag_id");
                        j.IndexerProperty<Guid>("ProblemId").HasColumnName("problem_id");
                    });
        });

        modelBuilder.Entity<TestCase>(entity =>
        {
            entity.HasKey(e => e.TestCaseId).HasName("test_case_pk");

            entity.ToTable("test_case");

            entity.Property(e => e.TestCaseId)
                .ValueGeneratedNever()
                .HasColumnName("test_case_id");
            entity.Property(e => e.CallFunc)
                .HasMaxLength(256)
                .HasColumnName("call_func");
            entity.Property(e => e.Display)
                .HasMaxLength(1024)
                .HasColumnName("display");
            entity.Property(e => e.DisplayRes)
                .HasMaxLength(1024)
                .HasColumnName("display_res");
            entity.Property(e => e.IsPublic).HasColumnName("is_public");
            entity.Property(e => e.ProblemProblemId).HasColumnName("problem_problem_id");

            entity.HasOne(d => d.ProblemProblem).WithMany(p => p.TestCases)
                .HasForeignKey(d => d.ProblemProblemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("test_case_problem");
        });

        modelBuilder.Entity<UserConfig>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("user_config_pk");

            entity.ToTable("user_config");

            entity.Property(e => e.UserId)
                .ValueGeneratedNever()
                .HasColumnName("user_id");
            entity.Property(e => e.IsDarkMode).HasColumnName("is_dark_mode");
            entity.Property(e => e.IsHighContrast).HasColumnName("is_high_contrast");

            entity.HasOne(d => d.User).WithOne(p => p.UserConfig)
                .HasForeignKey<UserConfig>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("user_config_application_user");
        });

        modelBuilder.Entity<UserSolution>(entity =>
        {
            entity.HasKey(e => e.SolutionId).HasName("user_solution_pk");

            entity.ToTable("user_solution");

            entity.Property(e => e.SolutionId)
                .ValueGeneratedNever()
                .HasColumnName("solution_id");
            entity.Property(e => e.CodeRuntimeSubmitted)
                .HasColumnType("timestamp with time zone")
                .HasColumnName("code_runtime_submitted");
            entity.Property(e => e.LanguageId).HasColumnName("language_id");
            entity.Property(e => e.ProblemId).HasColumnName("problem_id");
            entity.Property(e => e.Stars).HasColumnName("stars");
            entity.Property(e => e.StatusId).HasColumnName("status_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Language).WithMany(p => p.UserSolutions)
                .HasForeignKey(d => d.LanguageId)
                .HasConstraintName("user_solution_language_ref");

            entity.HasOne(d => d.Problem).WithMany(p => p.UserSolutions)
                .HasForeignKey(d => d.ProblemId)
                .HasConstraintName("user_solution_problem_ref");

            entity.HasOne(d => d.Status).WithMany(p => p.UserSolutions)
                .HasForeignKey(d => d.StatusId)
                .HasConstraintName("user_solution_status_ref");

            entity.HasOne(d => d.User).WithMany(p => p.UserSolutions)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("solution_user_ref");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
