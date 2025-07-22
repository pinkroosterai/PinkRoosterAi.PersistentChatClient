using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.Extensions.AI;

namespace PinkRoosterAi.Framework.PersistentChatClient.EntityFramework.Entities;

public class ChatMessageEntity : BaseEntity
{
    public Guid ConversationId { get; set; }

    [MaxLength(500)]
    public string? MessageId { get; set; }

    [MaxLength(200)]
    public string? AuthorName { get; set; }

    public string Role { get; set; }
    public int OrderIndex { get; set; }


    [ForeignKey(nameof(ConversationId))]
    public virtual ChatConversationEntity Conversation { get; set; } = null!;

    public virtual ICollection<BaseContentEntity> Contents { get; set; } = [];
}