using System.ComponentModel.DataAnnotations;

namespace PinkRoosterAi.Framework.PersistentChatClient.EntityFramework.Entities;

public class ErrorContentEntity : BaseContentEntity
{
    public string Message { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? ErrorCode { get; set; }

    public string? Details { get; set; }
}