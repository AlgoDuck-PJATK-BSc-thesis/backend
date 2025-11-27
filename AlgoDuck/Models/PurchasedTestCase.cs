using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlgoDuck.Models;

public class PurchasedTestCase : IEntityTypeConfiguration<PurchasedTestCase>
{
    public TestCase? TestCase { get; set; }
    public Guid TestCaseId { get; set; }
    public ApplicationUser? User { get; set; }
    public Guid UserId { get; set; }

    public void Configure(EntityTypeBuilder<PurchasedTestCase> builder)
    {
        builder.HasKey(e => new { e.TestCaseId, e.UserId });

        builder.Property(e => e.TestCaseId).HasColumnName("test_case_id");
        builder.Property(e => e.UserId).HasColumnName("user_id");

        builder.HasOne(e => e.TestCase)
            .WithMany(e => e.PurchasedTestCases)
            .HasForeignKey(e => e.TestCaseId)
            .OnDelete(DeleteBehavior.ClientSetNull);

        builder.HasOne(e => e.User)
            .WithMany(e => e.PurchasedTestCases)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.ClientSetNull);
    }
}