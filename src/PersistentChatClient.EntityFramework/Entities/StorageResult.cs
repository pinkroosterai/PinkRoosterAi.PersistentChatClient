namespace PinkRoosterAi.Framework.PersistentChatClient.EntityFramework.Entities;

public record StorageResult<T>(bool Success, T? Data, string? ErrorMessage = null);