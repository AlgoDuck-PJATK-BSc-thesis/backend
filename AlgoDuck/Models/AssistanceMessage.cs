using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlgoDuck.Models;

public class AssistanceMessage : IEntityTypeConfiguration<AssistanceMessage>
{
    public Guid MessageId { get; set; } = Guid.NewGuid();
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    public virtual ICollection<AssistantMessageFragment> Fragments { get; set; } = new List<AssistantMessageFragment>();
    public Guid ChatId { get; set; } = Guid.NewGuid();
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

        builder.Property(e => e.IsUserMessage)
            .HasColumnName("is_user_message")
            .IsRequired();
        
        builder.HasOne(e => e.Chat)
            .WithMany(e => e.Messages)
            .HasForeignKey(e => e.ChatId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}