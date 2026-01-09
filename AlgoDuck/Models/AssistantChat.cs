using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlgoDuck.Models;

public class AssistantChat : IEntityTypeConfiguration<AssistantChat>
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    public required Guid UserId { get; set; }
    public ApplicationUser? User { get; set; }

    public required Guid ProblemId { get; set; }
    public Problem? Problem { get; set; }


    public ICollection<AssistanceMessage> Messages { get; set; } = new List<AssistanceMessage>();
    
    public void Configure(EntityTypeBuilder<AssistantChat> builder)
    {
        builder.ToTable("assistant_chat");
        
        builder.HasKey(e => e.Id)
            .HasName("assistant_chat_id");
        
        builder.Property(e => e.Name)
            .HasColumnName("name")
            .HasMaxLength(128);
        
        builder.Property(e => e.ProblemId)
            .HasColumnName("problem_id");
        
        builder.Property(e => e.UserId)
            .HasColumnName("user_id");
        
        builder.Property(e => e.CreatedOn)
            .HasColumnName("created_on")
            .ValueGeneratedNever();
        
        builder.HasOne(e => e.User)
            .WithMany(e => e.AssistantChats)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.ClientSetNull);
        
        builder.HasOne(e => e.Problem)
            .WithMany(e => e.AssistantChats)
            .HasForeignKey(e => e.ProblemId)
            .OnDelete(DeleteBehavior.ClientSetNull);
    }
    
}