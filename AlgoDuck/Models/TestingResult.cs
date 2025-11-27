using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlgoDuck.Models;

public class TestingResult : IEntityTypeConfiguration<TestingResult>
{
    public Guid UserSolutionId { get; set; }
    public UserSolution? UserSolution { get; set; }
    public Guid TestCaseId { get; set; }
    public TestCase? TestCase { get; set; }
    public bool IsPassed { get; set; }

    public void Configure(EntityTypeBuilder<TestingResult> builder)
    {
        builder.HasKey(e => new { e.TestCaseId, e.UserSolutionId });
        builder.Property(e => e.TestCaseId).HasColumnName("test_case_id");
        builder.Property(e => e.UserSolutionId).HasColumnName("solution_id");
        builder.Property(e => e.IsPassed).HasColumnName("is_passed");

        builder.HasOne(e => e.TestCase)
            .WithMany(e => e.TestingResults)
            .HasForeignKey(e => e.TestCaseId)
            .OnDelete(DeleteBehavior.ClientSetNull);
            
        builder.HasOne(e => e.UserSolution)
            .WithMany(e => e.TestingResults)
            .HasForeignKey(e => e.UserSolutionId)
            .OnDelete(DeleteBehavior.ClientSetNull);

    }
}