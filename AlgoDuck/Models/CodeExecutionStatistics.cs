using System.Text.Json.Serialization;
using AlgoDuck.Modules.Problem.Commands.CodeExecuteSubmission;
using AlgoDuck.Modules.Problem.Queries.GetProblemStatsAdmin;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlgoDuck.Models;

public class CodeExecutionStatistics : IEntityTypeConfiguration<CodeExecutionStatistics>
{
    public Guid CodeExecutionId { get; set; } = Guid.NewGuid();
    public required Guid? ProblemId { get; set; }
    public Problem? Problem { get; set; }
    public required Guid UserId { get; set; }
    public ApplicationUser ApplicationUser { get; set; } = null!;
    public required ExecutionResult Result { get; set; }
    public required TestCaseResult TestCaseResult { get; set; }
    public required JobType ExecutionType { get; set; }
    public required long ExecutionStartNs { get; set; } = DateTime.UtcNow.DateTimeToNanos();
    public required long ExecutionEndNs { get; set; } = DateTime.UtcNow.DateTimeToNanos();
    public required long JvmPeakMemKb { get; set; }
    public required int ExitCode { get; set; }
    public virtual List<TestingResult> TestingResults { get; set; } = [];

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
        
        builder.Property(x => x.Result)
            .HasColumnName("result");            
        
        builder.Property(x => x.ExecutionType)
            .HasColumnName("type");       
        
        builder.Property(e => e.ExecutionStartNs)
            .HasColumnName("execution_start_ns");
        
        builder.Property(e => e.ExecutionEndNs)
            .HasColumnName("execution_end_ns");
        
        builder.Property(e => e.JvmPeakMemKb)
            .HasColumnName("jvm_peak_mem_kb");
        
        builder.Property(e => e.ExitCode)
            .HasColumnName("exit_code");
        
        builder.Property(x => x.TestCaseResult)
            .HasColumnName("test_case_result");
        
        builder.HasOne(x => x.Problem)
            .WithMany(p => p.CodeExecutionStatistics)
            .HasForeignKey(x => x.ProblemId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
        
        builder.HasOne(x => x.ApplicationUser)
            .WithMany(p => p.CodeExecutionStatistics)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.ClientSetNull);
    }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TestCaseResult : byte
{
    Accepted,
    Rejected,
    NotApplicable
}
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ExecutionResult : byte
{
    Completed,
    Timeout,
    RuntimeError,
    CompilationError
}