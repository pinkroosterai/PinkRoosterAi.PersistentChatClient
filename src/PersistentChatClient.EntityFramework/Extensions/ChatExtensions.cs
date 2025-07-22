using Microsoft.Extensions.AI;

namespace PinkRoosterAi.Framework.PersistentChatClient.EntityFramework.Extensions;

public static class ChatExtensions
{
    /// <summary>
    
    /// </summary>
    public static bool IsValid(this ChatMessage message)
    {
        return !string.IsNullOrWhiteSpace(message.Text) || message.Contents.Count > 0;
    }

    /// <summary>
    ///     Gets the primary text content from a message
    /// </summary>
    public static string GetPrimaryText(this ChatMessage message)
    {
        return message.Contents.OfType<TextContent>().FirstOrDefault()?.Text ?? string.Empty;
    }
}