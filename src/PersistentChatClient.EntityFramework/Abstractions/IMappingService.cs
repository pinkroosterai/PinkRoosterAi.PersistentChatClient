using Microsoft.Extensions.AI;
using PinkRoosterAi.Framework.PersistentChatClient.EntityFramework.Entities;

namespace PinkRoosterAi.Framework.PersistentChatClient.EntityFramework.Abstractions;




public interface IMappingService
{
    StorageResult<ChatMessageEntity> ToEntity(ChatMessage message, Guid conversationId, int orderIndex);
    StorageResult<ChatMessage> ToModel(ChatMessageEntity entity);
    StorageResult<BaseContentEntity> ToContentEntity(AIContent content, Guid messageId, int orderIndex);
    StorageResult<AIContent> ToContentModel(BaseContentEntity entity);
}