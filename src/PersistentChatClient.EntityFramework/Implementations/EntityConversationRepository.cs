using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.AI;
using PinkRoosterAi.Framework.PersistentChatClient.Abstractions;
using PinkRoosterAi.Framework.PersistentChatClient.EntityFramework.Abstractions;
using PinkRoosterAi.Framework.PersistentChatClient.EntityFramework.Context;
using PinkRoosterAi.Framework.PersistentChatClient.EntityFramework.Entities;
using PinkRoosterAi.Framework.PersistentChatClient.Models;
using Microsoft.Extensions.Logging;

namespace PinkRoosterAi.Framework.PersistentChatClient.EntityFramework.Implementations;

public sealed class EntityConversationRepository : IEntityChatRepository, IConversationRepository
{
    private readonly ChatDbContext _context;
    private readonly ILogger<EntityConversationRepository> _logger;
    private readonly IMappingService _mappingService;

    public EntityConversationRepository(ChatDbContext context, ILoggerFactory? loggerFactory = null)
    {
        _context = context;

        _mappingService = new MappingService(new JsonService(),
            loggerFactory.CreateLogger<MappingService>());

        _logger = loggerFactory.CreateLogger<EntityConversationRepository>();
    }

    public async Task SaveMessagesAsync(Conversation conversation, IReadOnlyList<ChatMessage> responseMessages, CancellationToken cancellationToken = default)
    {
        using IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            // Find the conversation entity
            ChatConversationEntity? conversationEntity = await _context.Conversations
                .FirstOrDefaultAsync(c => c.ConversationId == conversation.Id,
                    cancellationToken);

            if (conversationEntity == null)
            {
                throw new InvalidOperationException($"Conversation with ID '{conversation.Id}' not found");
            }

            // Get current message count to set proper order indexes
            int currentMessageCount = await _context.Messages
                .CountAsync(m => m.ConversationId == conversationEntity.Id,
                    cancellationToken);

            // Add response messages
            for (int i = 0; i < responseMessages.Count; i++)
            {
                StorageResult<ChatMessageEntity> messageResult = _mappingService.ToEntity(
                    responseMessages[i],
                    conversationEntity.Id,
                    currentMessageCount + i);

                if (!messageResult.Success)
                {
                    await transaction.RollbackAsync(cancellationToken);

                    throw new InvalidOperationException($"Failed to map message {i}: {messageResult.ErrorMessage}");
                }

                _context.Messages.Add(messageResult.Data!);
            }

            // Update conversation timestamp
            conversationEntity.LastModifiedAt = DateTimeOffset.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);

            _logger.LogError(ex,
                "Error saving messages to conversation {ConversationId}",
                conversation.Id);

            throw;
        }
    }

    public async Task<Conversation> GetOrCreateConversationAsync(string conversationId, IReadOnlyList<ChatMessage> newMessages, CancellationToken cancellationToken = default)
    {
        try
        {
            // Try to find existing conversation
            ChatConversationEntity? conversationEntity = await _context.Conversations
                .Include(c => c.Messages)
                .ThenInclude(m => m.Contents)
                .FirstOrDefaultAsync(c => c.ConversationId == conversationId,
                    cancellationToken);

            List<ChatMessage> allMessages = new List<ChatMessage>();
            DateTimeOffset createdAt;
            DateTimeOffset lastUpdatedAt;

            if (conversationEntity != null)
            {
                // Load existing messages
                List<ChatMessageEntity> existingMessages = conversationEntity.Messages
                    .OrderBy(m => m.OrderIndex)
                    .ToList();

                foreach (ChatMessageEntity messageEntity in existingMessages)
                {
                    StorageResult<ChatMessage> messageResult = _mappingService.ToModel(messageEntity);

                    if (messageResult.Success)
                    {
                        allMessages.Add(messageResult.Data!);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to map message {MessageId}: {Error}",
                            messageEntity.MessageId,
                            messageResult.ErrorMessage);
                    }
                }

                createdAt = conversationEntity.CreatedAt;
                lastUpdatedAt = conversationEntity.LastModifiedAt;
            }
            else
            {
                // Create new conversation entity
                conversationEntity = new ChatConversationEntity
                {
                    ConversationId = conversationId
                };

                _context.Conversations.Add(conversationEntity);
                await _context.SaveChangesAsync(cancellationToken);

                createdAt = conversationEntity.CreatedAt;
                lastUpdatedAt = conversationEntity.LastModifiedAt;
            }

            // Add new messages to the list
            allMessages.AddRange(newMessages);

            // Create the conversation model
            Conversation conversation = new Conversation
            {
                Id = conversationId,
                Messages = allMessages,
                CreatedAt = createdAt,
                LastUpdatedAt = lastUpdatedAt
            };

            // If we have new messages, save them to the database
            if (newMessages.Count > 0)
            {
                await SaveMessagesAsync(conversation,
                    newMessages,
                    cancellationToken);

                conversation.LastUpdatedAt = DateTimeOffset.UtcNow;
            }

            return conversation;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error getting or creating conversation {ConversationId}",
                conversationId);

            throw;
        }
    }

    public async Task<StorageResult<Guid>> SaveConversationAsync(IEnumerable<ChatMessage> messages, string? conversationId = null)
    {
        using IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            ChatConversationEntity conversationEntity = new ChatConversationEntity
            {
                ConversationId = conversationId
            };

            _context.Conversations.Add(conversationEntity);
            await _context.SaveChangesAsync();

            List<ChatMessage> messagesList = messages.ToList();

            for (int i = 0; i < messagesList.Count; i++)
            {
                StorageResult<ChatMessageEntity> messageResult = _mappingService.ToEntity(messagesList[i],
                    conversationEntity.Id,
                    i);

                if (!messageResult.Success)
                {
                    await transaction.RollbackAsync();

                    return new StorageResult<Guid>(false,
                        Guid.Empty,
                        $"Failed to save message {i}: {messageResult.ErrorMessage}");
                }

                _context.Messages.Add(messageResult.Data!);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return new StorageResult<Guid>(true,
                conversationEntity.Id);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();

            _logger.LogError(ex,
                "Error saving conversation");

            return new StorageResult<Guid>(false,
                Guid.Empty,
                ex.Message);
        }
    }

    public async Task<StorageResult<IReadOnlyList<ChatMessage>>> GetMessagesAsync(string conversationId)
    {
        try
        {
            List<ChatMessageEntity> messages = await _context.Messages
                .Include(m => m.Contents)
                .Where(m => m.Conversation.ConversationId == conversationId)
                .OrderBy(m => m.OrderIndex)
                .AsNoTracking()
                .ToListAsync();

            List<ChatMessage> results = new List<ChatMessage>();

            foreach (ChatMessageEntity messageEntity in messages)
            {
                StorageResult<ChatMessage> messageResult = _mappingService.ToModel(messageEntity);

                if (messageResult.Success)
                {
                    results.Add(messageResult.Data!);
                }
                else
                {
                    _logger.LogWarning("Failed to map message {MessageId}: {Error}",
                        messageEntity.MessageId,
                        messageResult.ErrorMessage);
                }
            }

            return new StorageResult<IReadOnlyList<ChatMessage>>(true,
                results.AsReadOnly());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error retrieving messages for conversation {ConversationId}",
                conversationId);

            return new StorageResult<IReadOnlyList<ChatMessage>>(false,
                Array.Empty<ChatMessage>(),
                ex.Message);
        }
    }

    public async Task<StorageResult<bool>> SaveResponseAsync(ChatResponse response, Guid conversationId)
    {
        try
        {
            ChatResponseEntity responseEntity = new ChatResponseEntity
            {
                ConversationId = conversationId,
                ResponseId = response.ResponseId,
                ModelId = response.ModelId,
                CreatedAt = response.CreatedAt ?? DateTimeOffset.UtcNow,
                FinishReason = response.FinishReason.HasValue?response.FinishReason.Value.Value: string.Empty,

            };

            // Map usage details
            if (response.Usage is not null)
            {
                responseEntity.InputTokenCount = response.Usage.InputTokenCount;
                responseEntity.OutputTokenCount = response.Usage.OutputTokenCount;
                responseEntity.TotalTokenCount = response.Usage.TotalTokenCount;
            }

            _context.Responses.Add(responseEntity);
            await _context.SaveChangesAsync();

            return new StorageResult<bool>(true,
                true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error saving response");

            return new StorageResult<bool>(false,
                false,
                ex.Message);
        }
    }

    public async Task<StorageResult<ChatResponse>> GetResponseAsync(string responseId)
    {
        try
        {
            ChatResponseEntity? responseEntity = await _context.Responses
                .Include(r => r.Conversation)
                .Include(r => r.Messages)
                .ThenInclude(m => m.Contents)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.ResponseId == responseId);

            if (responseEntity is null)
            {
                return new StorageResult<ChatResponse>(false,
                    null,
                    "Response not found");
            }

            ChatResponse response = new ChatResponse
            {
                ResponseId = responseEntity.ResponseId,
                ConversationId = responseEntity.Conversation.ConversationId,
                ModelId = responseEntity.ModelId,
                CreatedAt = responseEntity.CreatedAt,
                FinishReason = new ChatFinishReason(responseEntity.FinishReason)

            };

            // Map usage details
            if (responseEntity.InputTokenCount.HasValue || responseEntity.OutputTokenCount.HasValue || responseEntity.TotalTokenCount.HasValue)
            {
                response.Usage = new UsageDetails
                {
                    InputTokenCount = responseEntity.InputTokenCount,
                    OutputTokenCount = responseEntity.OutputTokenCount,
                    TotalTokenCount = responseEntity.TotalTokenCount
                };
            }

            // Map messages
            IOrderedEnumerable<ChatMessageEntity> orderedMessages = responseEntity.Messages.OrderBy(m => m.OrderIndex);

            foreach (ChatMessageEntity messageEntity in orderedMessages)
            {
                StorageResult<ChatMessage> messageResult = _mappingService.ToModel(messageEntity);

                if (messageResult.Success)
                {
                    response.Messages.Add(messageResult.Data!);
                }
            }

            return new StorageResult<ChatResponse>(true,
                response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error retrieving response {ResponseId}",
                responseId);

            return new StorageResult<ChatResponse>(false,
                null,
                ex.Message);
        }
    }

    public async Task<StorageResult<IReadOnlyList<ChatConversationInfo>>> GetConversationsAsync(int skip = 0, int take = 50)
    {
        try
        {
            List<ChatConversationInfo> conversations = await _context.Conversations
                .OrderByDescending(c => c.LastModifiedAt)
                .Skip(skip)
                .Take(Math.Min(take,
                    100)) // Limit max results
                .Select(c => new ChatConversationInfo(c.Id,
                    c.ConversationId,
                    c.CreatedAt,
                    c.LastModifiedAt))
                .AsNoTracking()
                .ToListAsync();

            return new StorageResult<IReadOnlyList<ChatConversationInfo>>(true,
                conversations.AsReadOnly());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error retrieving conversations");

            return new StorageResult<IReadOnlyList<ChatConversationInfo>>(false,
                [],
                ex.Message);
        }
    }
}