namespace PinkRoosterAi.Framework.PersistentChatClient.Models;

/// <summary>

/// </summary>
public sealed class ConversationPersistenceOptions
{
    /// <summary>Gets or sets the auto-creation mode for conversation IDs.</summary>
    public AutoConversationCreationMode AutoCreationMode { get; set; } = AutoConversationCreationMode.GenerateWhenMissing;

    /// <summary>Gets or sets whether to continue streaming on persistence failures.</summary>
    public bool ContinueStreamingOnPersistenceFailure { get; set; } = true;
}