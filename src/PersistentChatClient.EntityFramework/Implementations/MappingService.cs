using Microsoft.Extensions.AI;
using PinkRoosterAi.Framework.PersistentChatClient.EntityFramework.Abstractions;
using PinkRoosterAi.Framework.PersistentChatClient.EntityFramework.Entities;
using Microsoft.Extensions.Logging;

namespace PinkRoosterAi.Framework.PersistentChatClient.EntityFramework.Implementations;

public sealed class MappingService : IMappingService
{
    private readonly IJsonService _jsonService;
    private readonly ILogger<MappingService> _logger;

    public MappingService(IJsonService jsonService, ILogger<MappingService> logger)
    {
        _jsonService = jsonService;
        _logger = logger;
    }

    public StorageResult<ChatMessageEntity> ToEntity(ChatMessage message, Guid conversationId, int orderIndex)
    {
        try
        {
            ChatMessageEntity entity = new ChatMessageEntity
            {
                ConversationId = conversationId,
                MessageId = message.MessageId,
                AuthorName = message.AuthorName,
                Role = message.Role.Value,
                OrderIndex = orderIndex

            };

            // Map contents
            for (int i = 0; i < message.Contents.Count; i++)
            {
                StorageResult<BaseContentEntity> contentResult = ToContentEntity(message.Contents[i],
                    entity.Id,
                    i);

                if (!contentResult.Success)
                {
                    return new StorageResult<ChatMessageEntity>(false,
                        null,
                        $"Failed to map content at index {i}: {contentResult.ErrorMessage}");
                }

                entity.Contents.Add(contentResult.Data!);
            }

            return new StorageResult<ChatMessageEntity>(true,
                entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error mapping ChatMessage to entity");

            return new StorageResult<ChatMessageEntity>(false,
                null,
                ex.Message);
        }
    }

    public StorageResult<ChatMessage> ToModel(ChatMessageEntity entity)
    {
        try
        {
            ChatMessage message = new ChatMessage
            {
                MessageId = entity.MessageId,
                AuthorName = entity.AuthorName,
            Role = new ChatRole(entity.Role)

            };


            // Map contents in order
            IOrderedEnumerable<BaseContentEntity> orderedContents = entity.Contents.OrderBy(c => c.OrderIndex);

            foreach (BaseContentEntity contentEntity in orderedContents)
            {
                StorageResult<AIContent> contentResult = ToContentModel(contentEntity);

                if (!contentResult.Success)
                {
                    _logger.LogWarning("Failed to map content entity {ContentId}: {Error}",
                        contentEntity.Id,
                        contentResult.ErrorMessage);

                    continue; // Skip corrupted content but continue processing
                }

                message.Contents.Add(contentResult.Data!);
            }

            return new StorageResult<ChatMessage>(true,
                message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error mapping entity {EntityId} to ChatMessage",
                entity.Id);

            return new StorageResult<ChatMessage>(false,
                null,
                ex.Message);
        }
    }

    public StorageResult<BaseContentEntity> ToContentEntity(AIContent content, Guid messageId, int orderIndex)
    {
        try
        {

            BaseContentEntity entity = content switch
            {
                TextContent text => new TextContentEntity
                {
                    Text = text.Text,
                    MessageId = messageId,
                    OrderIndex = orderIndex
                },
                TextReasoningContent reasoning => new ReasoningContentEntity
                {
                    Text = reasoning.Text,
                    MessageId = messageId,
                    OrderIndex = orderIndex
                },
                DataContent data => throw new NotImplementedException(nameof(DataContent)),
                UriContent uri => new UriContentEntity
                {
                    Uri = uri.Uri.ToString(),
                    MediaType = uri.MediaType,
                    MessageId = messageId,
                    OrderIndex = orderIndex
                },
                ErrorContent error => new ErrorContentEntity
                {
                    Message = error.Message,
                    ErrorCode = error.ErrorCode,
                    Details = error.Details,
                    MessageId = messageId,
                    OrderIndex = orderIndex
                },
                FunctionCallContent call => new FunctionCallContentEntity
                {
                    CallId = call.CallId,
                    Name = call.Name,
                    ArgumentsJson = _jsonService.SerializeObject(call.Arguments),
                    MessageId = messageId,
                    OrderIndex = orderIndex
                },
                FunctionResultContent result => new FunctionResultContentEntity
                {
                    CallId = result.CallId,
                    ResultJson = _jsonService.SerializeObject(result.Result),
                    MessageId = messageId,
                    OrderIndex = orderIndex
                },
                UsageContent usage => new UsageContentEntity
                {
                    InputTokenCount = usage.Details.InputTokenCount,
                    OutputTokenCount = usage.Details.OutputTokenCount,
                    TotalTokenCount = usage.Details.TotalTokenCount,
                    MessageId = messageId,
                    OrderIndex = orderIndex

                },
                _ => throw new NotSupportedException($"Content type {content.GetType().Name} is not supported")
            };

            return new StorageResult<BaseContentEntity>(true,
                entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error mapping AIContent to entity");

            return new StorageResult<BaseContentEntity>(false,
                null,
                ex.Message);
        }
    }

    public StorageResult<AIContent> ToContentModel(BaseContentEntity entity)
    {
        try
        {
            AIContent content = entity switch
            {
                TextContentEntity text => new TextContent(text.Text),
                ReasoningContentEntity reasoning => new TextReasoningContent(reasoning.Text),
                DataContentEntity data => throw new NotImplementedException(nameof(DataContentEntity)),
                UriContentEntity uri => new UriContent(uri.Uri,
                    uri.MediaType),
                ErrorContentEntity error => new ErrorContent(error.Message)
                {
                    ErrorCode = error.ErrorCode,
                    Details = error.Details
                },
                FunctionCallContentEntity call => new FunctionCallContent(
                    call.CallId,
                    call.Name,
                    _jsonService.DeserializeObject<Dictionary<string, object?>>(call.ArgumentsJson)),
                FunctionResultContentEntity result => new FunctionResultContent(
                    result.CallId,
                    _jsonService.DeserializeObject<object>(result.ResultJson)),
                UsageContentEntity usage => new UsageContent(new UsageDetails
                {
                    InputTokenCount = usage.InputTokenCount,
                    OutputTokenCount = usage.OutputTokenCount,
                    TotalTokenCount = usage.TotalTokenCount
                }),
                _ => throw new NotSupportedException($"Entity type {entity.GetType().Name} is not supported")
            };

     

            return new StorageResult<AIContent>(true,
                content);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error mapping entity {EntityId} to AIContent",
                entity.Id);

            return new StorageResult<AIContent>(false,
                null,
                ex.Message);
        }
    }
}