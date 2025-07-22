using System.ComponentModel.DataAnnotations.Schema;

namespace PinkRoosterAi.Framework.PersistentChatClient.EntityFramework.Entities;

public abstract class BaseContentEntity : BaseEntity
{
    public Guid MessageId { get; set; }

    public int OrderIndex { get; set; }
    //  - is explicitly ignored for now

    [ForeignKey(nameof(MessageId))]
    public virtual ChatMessageEntity Message { get; set; } = null!;
}