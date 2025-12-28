
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlgoDuck.Models;

public partial class Contest : IEntityTypeConfiguration<Contest>
{
    public Guid ContestId { get; set; }

    public string ContestName { get; set; } = null!;

    public string ContestDescription { get; set; } = null!;

    public DateTime ContestStartDate { get; set; }

    public DateTime ContestEndDate { get; set; }

    public Guid ItemId { get; set; }

    public virtual Item Item { get; set; } = null!;

    public virtual ICollection<Problem> Problems { get; set; } = new List<Problem>();
    public void Configure(EntityTypeBuilder<Contest> builder)
    {
        builder.HasKey(e => e.ContestId).HasName("contest_pk");

        builder.ToTable("contest");

        builder.Property(e => e.ContestId)
            .ValueGeneratedNever()
            .HasColumnName("contest_id");
        builder.Property(e => e.ContestDescription)
            .HasMaxLength(2048)
            .HasColumnName("contest_description");
        builder.Property(e => e.ContestEndDate)
            .HasColumnType("timestamp with time zone")
            .HasColumnName("contest_end_date");
        builder.Property(e => e.ContestName)
            .HasMaxLength(256)
            .HasColumnName("contest_name");
        builder.Property(e => e.ContestStartDate)
            .HasColumnType("timestamp with time zone")
            .HasColumnName("contest_start_date");
        builder.Property(e => e.ItemId).HasColumnName("item_id");

        builder.HasOne(d => d.Item).WithMany(p => p.Contests)
            .HasForeignKey(d => d.ItemId)
            .HasConstraintName("contest_item_ref");
    }
}
