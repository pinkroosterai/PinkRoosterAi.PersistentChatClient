using System.Collections.Concurrent;
using Microsoft.Extensions.AI;
using PinkRoosterAi.Framework.PersistentChatClient.Abstractions;
using PinkRoosterAi.Framework.PersistentChatClient.Models;

namespace PinkRoosterAi.Framework.PersistentChatClient.Implementations;

/// <summary>

/// </summary>
public sealed class InMemoryConversationRepository : IConversationRepository
{
    private readonly ConcurrentDictionary<string, Conversation> _conversations = new();

    /// <inheritdoc />
    public Task<Conversation> GetOrCreateConversationAsync(string conversationId, IReadOnlyList<ChatMessage> newMessages, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(conversationId);
        ArgumentNullException.ThrowIfNull(newMessages);

        DateTimeOffset now = DateTimeOffset.UtcNow;
        
        Conversation conversation = _conversations.AddOrUpdate(
            conversationId,
            // Add new conversation
            _ => new Conversation
            {
                Id = conversationId,
                Messages = new List<ChatMessage>(newMessages),
                CreatedAt = now,
                LastUpdatedAt = now
            },
            // Update existing conversation
            (_, existing) =>
            {
                List<ChatMessage> updatedMessages = new List<ChatMessage>(existing.Messages);
                updatedMessages.AddRange(newMessages);
                
                return new Conversation
                {
                    Id = conversationId,
                    Messages = updatedMessages,
                    CreatedAt = existing.CreatedAt,
                    LastUpdatedAt = now
                };
            });

        return Task.FromResult(conversation);
    }

    /// <inheritdoc />
    public Task SaveMessagesAsync(Conversation conversation, IReadOnlyList<ChatMessage> responseMessages, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(conversation);
        ArgumentNullException.ThrowIfNull(responseMessages);

        if (responseMessages.Count == 0)
        {
            return Task.CompletedTask;
        }

        DateTimeOffset now = DateTimeOffset.UtcNow;

        _conversations.AddOrUpdate(
            conversation.Id,
            // Should not happen, but handle gracefully
            _ => new Conversation
            {
                Id = conversation.Id,
                Messages = new List<ChatMessage>(conversation.Messages.Concat(responseMessages)),
                CreatedAt = conversation.CreatedAt,
                LastUpdatedAt = now
            },
            // Update existing conversation
            (_, existing) =>
            {
                List<ChatMessage> updatedMessages = new List<ChatMessage>(existing.Messages);
                updatedMessages.AddRange(responseMessages);
                
                return new Conversation
                {
                    Id = conversation.Id,
                    Messages = updatedMessages,
                    CreatedAt = existing.CreatedAt,
                    LastUpdatedAt = now
                };
            });

        return Task.CompletedTask;
    }

    /// <summary>
    ///     Gets the total number of conversations stored in memory.
    /// </summary>
    public int ConversationCount => _conversations.Count;

    /// <summary>
    ///     Clears all conversations from memory.
    /// </summary>
    public void Clear()
    {
        _conversations.Clear();
    }

    /// <summary>
    ///     Checks if a conversation with the specified ID exists.
    /// </summary>
    /// <param name="conversationId">The conversation identifier.</param>
    /// <returns>True if the conversation exists; otherwise, false.</returns>
    public bool ContainsConversation(string conversationId)
    {
        ArgumentNullException.ThrowIfNull(conversationId);
        return _conversations.ContainsKey(conversationId);
    }
}