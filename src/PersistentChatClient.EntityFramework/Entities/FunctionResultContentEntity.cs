using System.ComponentModel.DataAnnotations;

namespace PinkRoosterAi.Framework.PersistentChatClient.EntityFramework.Entities;

public class FunctionResultContentEntity : BaseContentEntity
{
    [MaxLength(500)]
    public string CallId { get; set; } = string.Empty;

    public string? ResultJson { get; set; }
}