﻿namespace PinkRoosterAi.Framework.PersistentChatClient.EntityFramework.Entities;

public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}