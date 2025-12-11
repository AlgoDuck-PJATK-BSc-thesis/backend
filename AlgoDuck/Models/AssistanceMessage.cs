using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlgoDuck.Models;

public class AssistanceMessage : IEntityTypeConfiguration<AssistanceMessage>
{
    public Guid MessageId { get; set; } = Guid.NewGuid();
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    public virtual ICollection<AssistantMessageFragment> Fragments { get; set; } = new List<AssistantMessageFragment>();
    public Guid UserId { get; set; }
    public Guid ProblemId { get; set; }
    public string ChatName { get; set; } = string.Empty;
    
    public bool IsUserMessage { get; set; } // this could probably be done cleaner, but I can't be bothered
    public AssistantChat? Chat { get; set; }
    
    public void Configure(EntityTypeBuilder<AssistanceMessage> builder)
    {
        builder.ToTable("assistance_message");

        builder.HasKey(e => e.MessageId);
        
        builder.Property(e => e.MessageId)
            .HasColumnName("message_id")
            .ValueGeneratedNever();
        
        builder.Property(e => e.CreatedOn)
            .HasColumnName("created_on")
            .ValueGeneratedNever();

        builder.Property(e => e.UserId)
            .HasColumnName("user_id")
            .IsRequired();
        
        builder.Property(e => e.IsUserMessage)
            .HasColumnName("is_user_message")
            .IsRequired();
        
        builder.Property(e => e.ProblemId)
            .HasColumnName("problem_id")
            .IsRequired();
        
        builder.Property(e => e.ChatName)
            .HasColumnName("chat_name")
            .HasMaxLength(128)
            .IsRequired();
        
        builder.HasOne(e => e.Chat)
            .WithMany(e => e.Messages)
            .HasForeignKey(e => new { e.ChatName, e.ProblemId,  e.UserId })
            .OnDelete(DeleteBehavior.ClientSetNull);
    }
}