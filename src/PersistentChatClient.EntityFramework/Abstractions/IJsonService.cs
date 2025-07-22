namespace PinkRoosterAi.Framework.PersistentChatClient.EntityFramework.Abstractions;

/// <summary>

/// </summary>
public interface IJsonService
{
    string? SerializeObject<T>(T obj) where T : class;
    T? DeserializeObject<T>(string? json) where T : class, new();
}