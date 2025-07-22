using System.Text.Json;
using System.Text.Json.Serialization;
using PinkRoosterAi.Framework.PersistentChatClient.EntityFramework.Abstractions;

namespace PinkRoosterAi.Framework.PersistentChatClient.EntityFramework.Implementations;

public sealed class JsonService : IJsonService
{
    private static readonly JsonSerializerOptions SafeOptions = new JsonSerializerOptions
    {
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        MaxDepth = 10 // Prevent deep recursion attacks
    };

    public string? SerializeObject<T>(T obj) where T : class
    {
        return obj is not null
            ? JsonSerializer.Serialize(obj,
                SafeOptions)
            : null;
    }

    public T? DeserializeObject<T>(string? json) where T : class, new()
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<T>(json,
                SafeOptions);
        }
        catch (JsonException)
        {
            return new T(); // Return empty object for corrupted data
        }
    }
}