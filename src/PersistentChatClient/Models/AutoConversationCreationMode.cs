namespace PinkRoosterAi.Framework.PersistentChatClient.Models;

/// <summary>

/// </summary>
public enum AutoConversationCreationMode
{
    /// <summary>Conversations must have explicit IDs - no automatic generation.</summary>
    None,

    /// <summary>Generates ID from hash of system and user messages when exactly 2 messages exist.</summary>
    HashSystemAndUserMessage,

    /// <summary>Generates a new ID when ConversationId is null or empty.</summary>
    GenerateWhenMissing
}