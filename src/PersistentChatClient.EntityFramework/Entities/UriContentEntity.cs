using System.ComponentModel.DataAnnotations;

namespace PinkRoosterAi.Framework.PersistentChatClient.EntityFramework.Entities;

public class UriContentEntity : BaseContentEntity
{
    [MaxLength(2000)]
    public string Uri { get; set; } = string.Empty;

    [MaxLength(200)]
    public string MediaType { get; set; } = string.Empty;
}