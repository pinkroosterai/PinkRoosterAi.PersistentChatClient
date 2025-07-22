using System.ComponentModel.DataAnnotations;

namespace PinkRoosterAi.Framework.PersistentChatClient.EntityFramework.Entities;

public class FunctionCallContentEntity : BaseContentEntity
{
    [MaxLength(500)]
    public string CallId { get; set; } = string.Empty;

    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public string? ArgumentsJson { get; set; }
}