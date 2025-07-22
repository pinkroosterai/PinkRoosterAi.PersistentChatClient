using System.ComponentModel.DataAnnotations;

namespace PinkRoosterAi.Framework.PersistentChatClient.EntityFramework.Entities;

public class DataContentEntity : BaseContentEntity
{
    [MaxLength(200)]
    public string MediaType { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Name { get; set; }

    // Store reference to blob storage instead of inline data
    [MaxLength(500)]
    public string? BlobReference { get; set; }

    // Only store small data inline (< 1KB)
    public byte[]? SmallData { get; set; }

    public bool IsLargeData => BlobReference is not null;
}