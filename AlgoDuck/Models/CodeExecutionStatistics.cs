using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlgoDuck.Models;

public class CodeExecutionStatistics : IEntityTypeConfiguration<CodeExecutionStatistics>
{
    public Guid CodeExecutionId { get; set; } = Guid.NewGuid();
    public required Guid? ProblemId { get; set; }
    public Problem? Problem { get; set; }
    public required Guid UserId { get; set; }
    public ApplicationUser? ApplicationUser { get; set; }
    public DateTime ExecutionTimestamp { get; set; } = DateTime.UtcNow;
    public required ExecutionResult Result { get; set; }
    public required TestCaseResult TestCaseResult { get; set; }
    

    public void Configure(EntityTypeBuilder<CodeExecutionStatistics> builder)
    {
        builder.ToTable("code_execution_statistics");

        builder.HasKey(x => x.CodeExecutionId)
            .HasName("code_execution_statistics_id");

        builder.Property(x => x.CodeExecutionId)
            .HasColumnName("code_execution_statistics_id");
        
        builder.Property(x => x.ProblemId)
            .HasColumnName("problem_id");
        
        builder.Property(x => x.UserId)
            .HasColumnName("user_id");
        
        builder.Property(x => x.ExecutionTimestamp)
            .HasColumnName("execution_timestamp");
        
        builder.Property(x => x.Result)
            .HasColumnName("result");       
        
        builder.Property(x => x.TestCaseResult)
            .HasColumnName("test_case_result");
        
        builder.HasOne(x => x.Problem)
            .WithMany(p => p.CodeExecutionStatistics)
            .HasForeignKey(x => x.ProblemId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.ClientSetNull);
        
        
        builder.HasOne(x => x.ApplicationUser)
            .WithMany(p => p.CodeExecutionStatistics)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.ClientSetNull);
    }
}

public enum TestCaseResult : byte
{
    Accepted,
    Rejected,
    NotApplicable
}

public enum ExecutionResult : byte
{
    Completed,
    Timeout,
    RuntimeError,
    CompilationError
}