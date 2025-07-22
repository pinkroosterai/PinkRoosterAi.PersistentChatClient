using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.AI;
using PinkRoosterAi.Framework.PersistentChatClient.Models;

namespace PinkRoosterAi.Framework.PersistentChatClient.Implementations;




internal sealed class ConversationIdGenerator
{
    private readonly AutoConversationCreationMode _mode;

    public ConversationIdGenerator(AutoConversationCreationMode mode)
    {
        _mode = mode;
    }

    public string? GenerateIdIfNeeded(string? conversationId, IReadOnlyList<ChatMessage> messages)
    {
        if (!string.IsNullOrWhiteSpace(conversationId))
        {
            return conversationId;
        }

        return _mode switch
        {
            AutoConversationCreationMode.None => conversationId,
            AutoConversationCreationMode.HashSystemAndUserMessage => GenerateHashBasedId(messages),
            AutoConversationCreationMode.GenerateWhenMissing => Guid.NewGuid().ToString("N"),
            _ => conversationId
        };
    }

    private static string? GenerateHashBasedId(IReadOnlyList<ChatMessage> messages)
    {
        if (messages.Count != 2)
        {
            return null;
        }

        ChatMessage? systemMessage = messages.FirstOrDefault(m => m.Role == ChatRole.System);
        ChatMessage? userMessage = messages.FirstOrDefault(m => m.Role == ChatRole.User);

        if (systemMessage == null || userMessage == null)
        {
            return null;
        }

        string combined = $"{systemMessage.Text}|{userMessage.Text}";
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(combined));

        return Convert.ToHexString(hash)[..16].ToLowerInvariant();
    }
}