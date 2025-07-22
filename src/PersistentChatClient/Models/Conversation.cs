using Microsoft.Extensions.AI;

namespace PinkRoosterAi.Framework.PersistentChatClient.Models;

/// <summary>

/// </summary>
public sealed class Conversation
{
    /// <summary>Gets or sets the unique conversation identifier.</summary>
    required public string Id { get; set; }

    /// <summary>Gets or sets the list of messages in the conversation.</summary>
    required public List<ChatMessage> Messages { get; set; }

    /// <summary>Gets or sets when the conversation was created.</summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>Gets or sets when the conversation was last updated.</summary>
    public DateTimeOffset LastUpdatedAt { get; set; }
}