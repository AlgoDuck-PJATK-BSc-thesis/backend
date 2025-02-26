using Microsoft.EntityFrameworkCore;
using WebApplication1.Modules.User.Models;
using WebApplication1.Modules.Cohort.Models;
using WebApplication1.Modules.Contest.Models;
using WebApplication1.Modules.Item.Models;
using WebApplication1.Modules.Problem.Models;

namespace WebApplication1.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        
        public DbSet<User> Users { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Cohort> Cohorts { get; set; }
        public DbSet<Duel> Duels { get; set; }
        public DbSet<DuelParticipant> DuelParticipants { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Contest> Contests { get; set; }
        public DbSet<ContestProblem> ContestProblems { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<Purchase> Purchases { get; set; }
        public DbSet<Rarity> Rarities { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Difficulty> Difficulties { get; set; }
        public DbSet<HasTag> HasTags { get; set; }
        public DbSet<Language> Languages { get; set; }
        public DbSet<PersonalizedProblem> PersonalizedProblems { get; set; }
        public DbSet<Problem> Problems { get; set; }
        public DbSet<ProblemTemplate> ProblemTemplates { get; set; }
        public DbSet<ProblemType> ProblemTypes { get; set; }
        public DbSet<Status> Statuses { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<UserSolution> UserSolutions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            modelBuilder.Entity<ContestProblem>()
                .HasKey(cp => new { cp.ProblemId, cp.ContestId });

            modelBuilder.Entity<DuelParticipant>()
                .HasKey(dp => new { dp.DuelId, dp.UserId });

            modelBuilder.Entity<HasTag>()
                .HasKey(ht => new { ht.ProblemId, ht.TagId });

            modelBuilder.Entity<PersonalizedProblem>()
                .HasKey(pp => new { pp.ProblemId, pp.UserId });

            modelBuilder.Entity<Purchase>()
                .HasKey(p => new { p.ItemId, p.UserId });

            modelBuilder.Entity<UserSolution>()
                .HasKey(us => us.SolutionId);


            
            modelBuilder.Entity<Category>()
                .Property(c => c.CategoryId)
                .HasDefaultValueSql("gen_random_uuid()");

            modelBuilder.Entity<Cohort>()
                .Property(c => c.CohortId)
                .HasDefaultValueSql("gen_random_uuid()");

            modelBuilder.Entity<Contest>()
                .Property(c => c.ContestId)
                .HasDefaultValueSql("gen_random_uuid()");

            modelBuilder.Entity<Difficulty>()
                .Property(d => d.DifficultyId)
                .HasDefaultValueSql("gen_random_uuid()");

            modelBuilder.Entity<Duel>()
                .Property(d => d.DuelId)
                .HasDefaultValueSql("gen_random_uuid()");

            modelBuilder.Entity<Item>()
                .Property(i => i.ItemId)
                .HasDefaultValueSql("gen_random_uuid()");

            modelBuilder.Entity<Language>()
                .Property(l => l.LanguageId)
                .HasDefaultValueSql("gen_random_uuid()");

            modelBuilder.Entity<Message>()
                .Property(m => m.MessageId)
                .HasDefaultValueSql("gen_random_uuid()");

            modelBuilder.Entity<Notification>()
                .Property(n => n.NotificationId)
                .HasDefaultValueSql("gen_random_uuid()");

            modelBuilder.Entity<Problem>()
                .Property(p => p.ProblemId)
                .HasDefaultValueSql("gen_random_uuid()");

            modelBuilder.Entity<ProblemTemplate>()
                .Property(pt => pt.ProblemTemplateId)
                .HasDefaultValueSql("gen_random_uuid()");

            modelBuilder.Entity<ProblemType>()
                .Property(pt => pt.ProblemTypeId)
                .HasDefaultValueSql("gen_random_uuid()");

            modelBuilder.Entity<Rarity>()
                .Property(r => r.RarityId)
                .HasDefaultValueSql("gen_random_uuid()");

            modelBuilder.Entity<Session>()
                .Property(s => s.SessionId)
                .HasDefaultValueSql("gen_random_uuid()");

            modelBuilder.Entity<Status>()
                .Property(s => s.StatusId)
                .HasDefaultValueSql("gen_random_uuid()");

            modelBuilder.Entity<Tag>()
                .Property(t => t.TagId)
                .HasDefaultValueSql("gen_random_uuid()");

            modelBuilder.Entity<User>()
                .Property(u => u.UserId)
                .HasDefaultValueSql("gen_random_uuid()");

            modelBuilder.Entity<UserRole>()
                .Property(ur => ur.UserRoleId)
                .HasDefaultValueSql("gen_random_uuid()");

            modelBuilder.Entity<UserSolution>()
                .Property(us => us.SolutionId)
                .HasDefaultValueSql("gen_random_uuid()");


            
            
            modelBuilder.Entity<User>()
                .HasOne(u => u.Cohort)
                .WithMany(c => c.Users)
                .HasForeignKey(u => u.CohortId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Problem>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Problems)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Problem>()
                .HasOne(p => p.Difficulty)
                .WithMany(d => d.Problems)
                .HasForeignKey(p => p.DifficultyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Problem>()
                .HasOne(p => p.ProblemType)
                .WithMany(pt => pt.Problems)
                .HasForeignKey(p => p.ProblemTypeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserSolution>()
                .HasOne(us => us.User)
                .WithMany(u => u.UserSolutions)
                .HasForeignKey(us => us.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserSolution>()
                .HasOne(us => us.Problem)
                .WithMany(p => p.UserSolutions)
                .HasForeignKey(us => us.ProblemId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
