using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlgoDuck.Models;

public class TestingResult : IEntityTypeConfiguration<TestingResult>
{
    public Guid UserSolutionSnapshotId = Guid.NewGuid();
    public UserSolutionSnapshot? UserSolutionSnapshot { get; set; }
    public Guid TestingResultId { get; set; } = Guid.NewGuid();
    public Guid TestCaseId { get; set; }
    public TestCase? TestCase { get; set; }
    public bool IsPassed { get; set; }

    public void Configure(EntityTypeBuilder<TestingResult> builder)
    {
        builder.HasKey(e => e.TestingResultId)
            .HasName("PK_TestingResult");
        
        builder.Property(e => e.TestCaseId)
            .HasColumnName("test_case_id");
        
        builder.Property(e => e.IsPassed)
            .HasColumnName("is_passed");

        builder.Property(e => e.TestingResultId)
            .HasColumnName("PK_TestingResult")
            .ValueGeneratedNever();
        
        builder.HasOne(e => e.TestCase)
            .WithMany(e => e.TestingResults)
            .HasForeignKey(e => e.TestCaseId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(e => e.UserSolutionSnapshot)
            .WithMany(e => e.TestingSnapshotsResults)
            .HasForeignKey(e => e.UserSolutionSnapshotId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}