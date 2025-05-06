using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Modules.AuthModule.Models;
using WebApplication1.Modules.CohortModule.Models;
using WebApplication1.Modules.ContestModule.Models;
using WebApplication1.Modules.DuelModule.Models;
using WebApplication1.Modules.ItemModule.Models;
using WebApplication1.Modules.ProblemModule.Models;
using WebApplication1.Modules.UserModule.Models;

namespace WebApplication1.DAL
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Session> Sessions { get; set; }
        public new DbSet<UserRole> UserRoles { get; set; }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Cohort> Cohorts { get; set; }
        public DbSet<Contest> Contests { get; set; }
        public DbSet<ContestProblem> ContestProblems { get; set; }
        public DbSet<Difficulty> Difficulties { get; set; }
        public DbSet<Duel> Duels { get; set; }
        public DbSet<DuelParticipant> DuelParticipants { get; set; }
        public DbSet<HasTag> HasTags { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<Language> Languages { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<PersonalizedProblem> PersonalizedProblems { get; set; }
        public DbSet<Problem> Problems { get; set; }
        public DbSet<ProblemTemplate> ProblemTemplates { get; set; }
        public DbSet<ProblemType> ProblemTypes { get; set; }
        public DbSet<Purchase> Purchases { get; set; }
        public DbSet<Rarity> Rarities { get; set; }
        public DbSet<Status> Statuses { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<ApplicationUser> Users { get; set; }
        public DbSet<UserSolution> UserSolutions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                entity.ToTable("user");
                entity.HasKey(u => u.Id);
                entity.Property(u => u.Id).HasColumnName("user_id");
                entity.Property(u => u.UserName).HasColumnName("username");
                entity.Property(u => u.Email).HasColumnName("email");
                entity.Property(u => u.PasswordHash).HasColumnName("password");
                entity.Property(u => u.SecurityStamp).HasColumnName("salt");
                entity.Property(u => u.ProfilePicture).HasColumnName("profile_picture");
                entity.Property(u => u.CohortId).HasColumnName("cohort_id");
                entity.Property(u => u.Coins).HasColumnName("coins");
                entity.Property(u => u.Experience).HasColumnName("experience");
                entity.Property(u => u.AmountSolved).HasColumnName("amount_solved");

                entity.HasOne(u => u.UserRole)
                      .WithMany()
                      .HasForeignKey(u => u.UserRoleId)
                      .HasConstraintName("fk_user_user_role");
            });

            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.ToTable("user_role");
                entity.HasKey(r => r.UserRoleId);
                entity.Property(r => r.UserRoleId).HasColumnName("user_role_id");
                entity.Property(r => r.Name).HasColumnName("name");
            });

            modelBuilder.Entity<Session>(entity =>
            {
                entity.ToTable("session");
                entity.HasKey(s => s.SessionId);
                entity.Property(s => s.SessionId).HasColumnName("session_id");
                entity.Property(s => s.RefreshToken).HasColumnName("refresh_token");
                entity.Property(s => s.RefreshTokenExpiresAt).HasColumnName("refresh_token_expires_at");
                entity.Property(s => s.Revoked).HasColumnName("revoked");
                entity.Property(s => s.UserId).HasColumnName("user_id");

                entity.HasOne<ApplicationUser>(s => s.User)
                      .WithMany(u => u.Sessions)
                      .HasForeignKey(s => s.UserId)
                      .HasConstraintName("fk_session_user");
            });
            
            modelBuilder.Entity<ContestProblem>()
                .HasKey(cp => new { cp.ContestId, cp.ProblemId });

            modelBuilder.Entity<ContestProblem>()
                .HasOne(cp => cp.Contest)
                .WithMany(c => c.ContestProblems)
                .HasForeignKey(cp => cp.ContestId);

            modelBuilder.Entity<ContestProblem>()
                .HasOne(cp => cp.Problem)
                .WithMany(p => p.ContestProblems)
                .HasForeignKey(cp => cp.ProblemId);

            modelBuilder.Entity<DuelParticipant>()
                .HasKey(dp => new { dp.DuelId, dp.UserId });

            modelBuilder.Entity<DuelParticipant>()
                .HasOne(dp => dp.Duel)
                .WithMany(d => d.DuelParticipants)
                .HasForeignKey(dp => dp.DuelId);

            modelBuilder.Entity<DuelParticipant>()
                .HasOne(dp => dp.User)
                .WithMany(u => u.DuelParticipants)
                .HasForeignKey(dp => dp.UserId);

            modelBuilder.Entity<HasTag>()
                .HasKey(ht => new { ht.ProblemId, ht.TagId });

            modelBuilder.Entity<HasTag>()
                .HasOne(ht => ht.Problem)
                .WithMany(p => p.HasTags)
                .HasForeignKey(ht => ht.ProblemId);

            modelBuilder.Entity<HasTag>()
                .HasOne(ht => ht.Tag)
                .WithMany(t => t.HasTags)
                .HasForeignKey(ht => ht.TagId);

            modelBuilder.Entity<Purchase>()
                .HasKey(p => new { p.ItemId, p.UserId });

            modelBuilder.Entity<Purchase>()
                .HasOne(p => p.Item)
                .WithMany(i => i.Purchases)
                .HasForeignKey(p => p.ItemId);

            modelBuilder.Entity<Purchase>()
                .HasOne(p => p.User)
                .WithMany(u => u.Purchases)
                .HasForeignKey(p => p.UserId);

            modelBuilder.Entity<PersonalizedProblem>()
                .HasKey(pp => new { pp.ProblemId, pp.UserId });

            modelBuilder.Entity<PersonalizedProblem>()
                .HasOne(pp => pp.Problem)
                .WithMany(p => p.PersonalizedProblems)
                .HasForeignKey(pp => pp.ProblemId);

            modelBuilder.Entity<PersonalizedProblem>()
                .HasOne(pp => pp.User)
                .WithMany(u => u.PersonalizedProblems)
                .HasForeignKey(pp => pp.UserId);

            modelBuilder.Entity<UserSolution>()
                .HasOne(us => us.Problem)
                .WithMany(p => p.UserSolutions)
                .HasForeignKey(us => us.ProblemId);

            modelBuilder.Entity<UserSolution>()
                .HasOne(us => us.User)
                .WithMany(u => u.UserSolutions)
                .HasForeignKey(us => us.UserId);

            modelBuilder.Entity<UserSolution>()
                .HasOne(us => us.Language)
                .WithMany(l => l.UserSolutions)
                .HasForeignKey(us => us.LanguageId);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Cohort)
                .WithMany(c => c.Messages)
                .HasForeignKey(m => m.CohortId);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Cohort)
                .WithMany(c => c.Notifications)
                .HasForeignKey(n => n.CohortId);

            modelBuilder.Entity<Session>()
                .HasOne(s => s.User)
                .WithMany(u => u.Sessions)
                .HasForeignKey(s => s.UserId);

            modelBuilder.Entity<ApplicationUser>()
                .HasOne(u => u.Cohort)
                .WithMany(c => c.Users)
                .HasForeignKey(u => u.CohortId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}