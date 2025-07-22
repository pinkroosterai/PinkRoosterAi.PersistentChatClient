using Microsoft.EntityFrameworkCore;

using PinkRoosterAi.Framework.PersistentChatClient.EntityFramework.Abstractions;
using PinkRoosterAi.Framework.PersistentChatClient.EntityFramework.Context;
using PinkRoosterAi.Framework.PersistentChatClient.EntityFramework.Implementations;
using Microsoft.Extensions.DependencyInjection;

namespace PinkRoosterAi.Framework.PersistentChatClient.EntityFramework.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    
    /// </summary>
    public static IServiceCollection AddChatStorage(this IServiceCollection services, Action<DbContextOptionsBuilder> configureDb)
    {
        services.AddDbContext<ChatDbContext>(configureDb);
        services.AddScoped<IJsonService, JsonService>();
        services.AddScoped<IMappingService, MappingService>();
        services.AddScoped<IEntityChatRepository, EntityConversationRepository>();

        return services;
    }
}