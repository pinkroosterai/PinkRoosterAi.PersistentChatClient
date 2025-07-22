using Microsoft.Extensions.AI;
using PinkRoosterAi.Framework.PersistentChatClient.Models;

namespace PinkRoosterAi.Framework.PersistentChatClient.Abstractions;

/// <summary>

/// </summary>
public interface IConversationRepository 
{
    /// <summary>
    ///     Persists messages to a conversation.
    /// </summary>
    /// <param name="conversation">The conversation to update.</param>
    /// <param name="responseMessages">Response messages to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SaveMessagesAsync(Conversation conversation, IReadOnlyList<ChatMessage> responseMessages, CancellationToken cancellationToken = default);
    
    /// <summary>
    ///     Retrieves a conversation by ID and adds new messages to it.
    /// </summary>
    /// <param name="conversationId">The conversation identifier.</param>
    /// <param name="newMessages">New messages to add to the conversation.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The conversation with new messages added, or a new conversation if not found.</returns>
    Task<Conversation> GetOrCreateConversationAsync(string conversationId, IReadOnlyList<ChatMessage> newMessages, CancellationToken cancellationToken = default);
}