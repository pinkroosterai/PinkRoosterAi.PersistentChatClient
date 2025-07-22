using System.ComponentModel.DataAnnotations;

namespace PinkRoosterAi.Framework.PersistentChatClient.EntityFramework.Entities;

public class ChatConversationEntity : BaseEntity
{
    [MaxLength(500)]
    public string? ConversationId { get; set; }

    public DateTimeOffset LastModifiedAt { get; set; } = DateTimeOffset.UtcNow;


    public virtual ICollection<ChatMessageEntity> Messages { get; set; } = [];
    public virtual ICollection<ChatResponseEntity> Responses { get; set; } = [];
}