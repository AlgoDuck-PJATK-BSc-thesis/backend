using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlgoDuck.Models;

public partial class UserSolution : IEntityTypeConfiguration<UserSolution>
{
    public Guid SolutionId { get; set; } = Guid.NewGuid();

    public byte Stars { get; set; }

    public long CodeRuntimeSubmitted { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public Guid UserId { get; set; }

    public Guid ProblemId { get; set; }

    public Guid StatusId { get; set; }

    public virtual Problem Problem { get; set; } = null!;
    
    public virtual Status Status { get; set; } = null!;

    public virtual ApplicationUser User { get; set; } = null!;
    public virtual ICollection<TestingResult> TestingResults { get; set; } = new List<TestingResult>();
    public void Configure(EntityTypeBuilder<UserSolution> builder)
    {
        builder.HasKey(e => e.SolutionId).HasName("user_solution_pk");

        builder.ToTable("user_solution");

        builder.Property(e => e.SolutionId)
            .ValueGeneratedNever()
            .HasColumnName("solution_id");
        builder.Property(e => e.CodeRuntimeSubmitted)
            .HasColumnName("code_runtime_submitted");
            
        builder.Property(e => e.ProblemId).HasColumnName("problem_id");
        builder.Property(e => e.Stars).HasColumnName("stars");
        builder.Property(e => e.StatusId).HasColumnName("status_id");
        builder.Property(e => e.UserId).HasColumnName("user_id");

        builder.HasOne(d => d.Problem).WithMany(p => p.UserSolutions)
            .HasForeignKey(d => d.ProblemId)
            .HasConstraintName("user_solution_problem_ref");

        builder.HasOne(d => d.Status).WithMany(p => p.UserSolutions)
            .HasForeignKey(d => d.StatusId)
            .HasConstraintName("user_solution_status_ref");

        builder.HasOne(d => d.User).WithMany(p => p.UserSolutions)
            .HasForeignKey(d => d.UserId)
            .HasConstraintName("solution_user_ref");
        
        builder.HasKey(e => e.SolutionId).HasName("user_solution_pk");

        builder.ToTable("user_solution");

        builder.Property(e => e.SolutionId)
            .ValueGeneratedNever()
            .HasColumnName("solution_id");

        builder.Property(e => e.CodeRuntimeSubmitted)
            .HasColumnName("code_runtime_submitted");

        builder.Property(e => e.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .HasColumnName("created_at");

        builder.Property(e => e.ProblemId).HasColumnName("problem_id");
        builder.Property(e => e.Stars).HasColumnName("stars");
        builder.Property(e => e.StatusId).HasColumnName("status_id");
        builder.Property(e => e.UserId).HasColumnName("user_id");
    }
}
