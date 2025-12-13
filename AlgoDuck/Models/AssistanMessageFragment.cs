using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace AlgoDuck.Models;

public class AssistantMessageFragment : IEntityTypeConfiguration<AssistantMessageFragment>
{
    public Guid MessageFragmentId { get; set; } = Guid.NewGuid();
    public Guid MessageId { get; set; }
    public AssistanceMessage? Message { get; set; }
    public string Content { get; set; } = string.Empty;
    public FragmentType FragmentType { get; set; } = FragmentType.Text;
    
    public void Configure(EntityTypeBuilder<AssistantMessageFragment> builder)
    {
        builder.ToTable("assistant_message_code_fragment");
        
        builder.HasKey(i => i.MessageFragmentId);
        
        builder.Property(i => i.MessageFragmentId)
            .HasColumnName("fragment_id")
            .ValueGeneratedNever();
        
        builder.Property(i => i.Content)
            .HasMaxLength(2048)
            .HasColumnName("fragment_content");
        
        builder.Property(i => i.FragmentType)
            .HasColumnName("fragment_type")
            .HasConversion<string>();
        
        builder.HasOne(e => e.Message)
            .WithMany(e => e.Fragments)
            .HasForeignKey(e => e.MessageId)
            .OnDelete(DeleteBehavior.ClientSetNull);
    }
}

[JsonConverter(typeof(StringEnumConverter))]
public enum FragmentType
{
    Text, Code
}