using Microsoft.Extensions.AI;
using PinkRoosterAi.Framework.PersistentChatClient.EntityFramework.Entities;

namespace PinkRoosterAi.Framework.PersistentChatClient.EntityFramework.Abstractions;




public interface IEntityChatRepository
{
    Task<StorageResult<Guid>> SaveConversationAsync(IEnumerable<ChatMessage> messages, string? conversationId = null);
    Task<StorageResult<IReadOnlyList<ChatMessage>>> GetMessagesAsync(string conversationId);
    Task<StorageResult<bool>> SaveResponseAsync(ChatResponse response, Guid conversationId);
    Task<StorageResult<ChatResponse>> GetResponseAsync(string responseId);
    Task<StorageResult<IReadOnlyList<ChatConversationInfo>>> GetConversationsAsync(int skip = 0, int take = 50);
}