using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PinkRoosterAi.Framework.PersistentChatClient.EntityFramework.Entities;

public class ChatResponseEntity : BaseEntity
{
    public Guid ConversationId { get; set; }

    [MaxLength(500)]
    public string? ResponseId { get; set; }

    [MaxLength(200)]
    public string? ModelId { get; set; }

    public string? FinishReason { get; set; }


    // Embedded usage details to avoid unnecessary complexity
    public long? InputTokenCount { get; set; }
    public long? OutputTokenCount { get; set; }
    public long? TotalTokenCount { get; set; }


    [ForeignKey(nameof(ConversationId))]
    public virtual ChatConversationEntity Conversation { get; set; } = null!;

    public virtual ICollection<ChatMessageEntity> Messages { get; set; } = [];
}