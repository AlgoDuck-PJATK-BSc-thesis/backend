using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlgoDuck.Models;

public partial class TestCase : IEntityTypeConfiguration<TestCase>
{
    public Guid TestCaseId { get; set; }

    public string CallFunc { get; set; } = null!;

    public bool IsPublic { get; set; }

    public Guid ProblemProblemId { get; set; }

    public string Display { get; set; } = null!;

    public string DisplayRes { get; set; } = null!;

    public virtual Problem ProblemProblem { get; set; } = null!;

    public ICollection<PurchasedTestCase> PurchasedTestCases = new List<PurchasedTestCase>();
    public virtual ICollection<TestingResult> TestingResults { get; set; } = new List<TestingResult>();


    public void Configure(EntityTypeBuilder<TestCase> builder)
    {
        builder.HasKey(e => e.TestCaseId).HasName("test_case_pk");

        builder.ToTable("test_case");

        builder.Property(e => e.TestCaseId)
            .ValueGeneratedNever()
            .HasColumnName("test_case_id");
        builder.Property(e => e.CallFunc)
            .HasMaxLength(256)
            .HasColumnName("call_func");
        builder.Property(e => e.Display)
            .HasMaxLength(1024)
            .HasColumnName("display");
        builder.Property(e => e.DisplayRes)
            .HasMaxLength(1024)
            .HasColumnName("display_res");
        builder.Property(e => e.IsPublic).HasColumnName("is_public");
        builder.Property(e => e.ProblemProblemId).HasColumnName("problem_problem_id");

        builder.HasOne(d => d.ProblemProblem).WithMany(p => p.TestCases)
            .HasForeignKey(d => d.ProblemProblemId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("test_case_problem");
    }
}
