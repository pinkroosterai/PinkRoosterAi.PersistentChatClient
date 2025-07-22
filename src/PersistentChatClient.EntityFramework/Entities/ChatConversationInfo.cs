namespace PinkRoosterAi.Framework.PersistentChatClient.EntityFramework.Entities;

public record ChatConversationInfo(
    Guid Id,
    string? ConversationId,
    DateTimeOffset CreatedAt,
    DateTimeOffset LastModifiedAt);