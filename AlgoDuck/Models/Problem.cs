using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlgoDuck.Models;

public partial class Problem : IEntityTypeConfiguration<Problem>
{
    public Guid ProblemId { get; set; } = Guid.NewGuid();

    public string ProblemTitle { get; set; } = null!;

    public string Description { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public Guid CategoryId { get; set; }

    public Guid DifficultyId { get; set; }
    public ProblemStatus Status { get; set; } = ProblemStatus.Unverified;

    public virtual Category Category { get; set; } = null!;

    public virtual Difficulty Difficulty { get; set; } = null!;

    public virtual ICollection<TestCase> TestCases { get; set; } = new List<TestCase>();

    public virtual ICollection<UserSolution> UserSolutions { get; set; } = new List<UserSolution>();

    public virtual ICollection<Contest> Contests { get; set; } = new List<Contest>();
    public virtual ICollection<AssistantChat> AssistantChats { get; set; } = new List<AssistantChat>();
    public virtual ICollection<UserSolutionSnapshot> UserSolutionSnapshots { get; set; } = new List<UserSolutionSnapshot>();
    public virtual ICollection<CodeExecutionStatistics> CodeExecutionStatistics { get; set; } = new List<CodeExecutionStatistics>();
    
    

    public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();
    public void Configure(EntityTypeBuilder<Problem> builder)
    {
        builder.HasKey(e => e.ProblemId).HasName("problem_pk");

        builder.ToTable("problem");

        builder.Property(e => e.ProblemId)
                .ValueGeneratedNever()
                .HasColumnName("problem_id");
        builder.Property(e => e.CategoryId).HasColumnName("category_id");
        builder.Property(e => e.CreatedAt)
                .HasColumnType("timestamp with time zone")
                .HasColumnName("created_at");
        builder.Property(e => e.Description)
                .HasMaxLength(1024)
                .HasColumnName("description");
        builder.Property(e => e.DifficultyId).HasColumnName("difficulty_id");
        builder.Property(e => e.ProblemTitle)
                .HasMaxLength(256)
                .HasColumnName("problem_title");

        builder.HasOne(d => d.Category).WithMany(p => p.Problems)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("problem_category_ref");

        builder.HasOne(d => d.Difficulty).WithMany(p => p.Problems)
                .HasForeignKey(d => d.DifficultyId)
                .HasConstraintName("problem_difficulty_ref");

        builder.HasMany(d => d.Contests).WithMany(p => p.Problems)
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
    }
}

public enum ProblemStatus
{
    Verified, Unverified
}
