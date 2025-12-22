using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlgoDuck.Models;

public class UserSolutionSnapshot : IEntityTypeConfiguration<UserSolutionSnapshot>
{
    public Guid SnapShotId { get; set; } = Guid.NewGuid();
    public required Guid UserId { get; set; }
    public ApplicationUser? User { get; set; } 
    public required Guid ProblemId { get; set; }
    public Problem? Problem { get; set; }
    public required DateTime CreatedAt { get; set; }
    public virtual ICollection<TestingResult> TestingSnapshotsResults { get; set; } = [];

    
    public void Configure(EntityTypeBuilder<UserSolutionSnapshot> builder)
    {
        builder.ToTable("user_solution_snapshots");
        
        builder.HasKey(s => s.SnapShotId)
            .HasName("user_solution_snapshot_id");
        
        builder.Property(s => s.UserId)
            .HasColumnName("user_id");
        
        builder.Property(s => s.SnapShotId)
            .HasColumnName("user_solution_snapshot_id");
        
        builder.Property(s => s.ProblemId)
            .HasColumnName("problem_id");
        
        builder.Property(s => s.CreatedAt)
            .HasColumnName("created_at");
        
        builder.HasOne(e => e.Problem)
            .WithMany(e => e.UserSolutionSnapshots)
            .HasForeignKey(e => e.ProblemId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(e => e.User)
            .WithMany(e => e.UserSolutionSnapshots)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}