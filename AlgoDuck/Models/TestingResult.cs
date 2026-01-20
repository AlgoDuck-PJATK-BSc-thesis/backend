using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlgoDuck.Models;

public class TestingResult : IEntityTypeConfiguration<TestingResult>
{
    public required Guid CodeExecutionId { get; set; }
    public CodeExecutionStatistics? CodeExecution { get; set; }
    public required Guid TestCaseId { get; set; }
    public TestCase? TestCase { get; set; }
    public bool IsPassed { get; set; }

    public void Configure(EntityTypeBuilder<TestingResult> builder)
    {
        builder.ToTable("testing_results");
        builder.HasKey(e => new { e.TestCaseId, e.CodeExecutionId })
            .HasName("PK_TestingResult");
        
        builder.Property(e => e.TestCaseId)
            .HasColumnName("test_case_id");
        
        builder.Property(e => e.IsPassed)
            .HasColumnName("is_passed");

        builder.HasOne(e => e.TestCase)
            .WithMany(e => e.TestingResults)
            .HasForeignKey(e => e.TestCaseId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(e => e.CodeExecution)
            .WithMany(e => e.TestingResults)
            .HasForeignKey(e => e.CodeExecutionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}