namespace PinkRoosterAi.Framework.PersistentChatClient.EntityFramework.Entities;

public class UsageContentEntity : BaseContentEntity
{
    public long? InputTokenCount { get; set; }
    public long? OutputTokenCount { get; set; }
    public long? TotalTokenCount { get; set; }
}