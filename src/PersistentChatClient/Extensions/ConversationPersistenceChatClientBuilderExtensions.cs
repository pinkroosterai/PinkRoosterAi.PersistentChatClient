using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using PinkRoosterAi.Framework.PersistentChatClient.Abstractions;
using PinkRoosterAi.Framework.PersistentChatClient.Implementations;
using PinkRoosterAi.Framework.PersistentChatClient.Models;

namespace PinkRoosterAi.Framework.PersistentChatClient.Extensions;




public static class ConversationPersistenceChatClientBuilderExtensions
{








    public static ChatClientBuilder UseConversationPersistence(
        this ChatClientBuilder builder,
        ILoggerFactory? loggerFactory = null,
        IConversationRepository? repository = null,
        Action<ConversationPersistenceOptions>? configureOptions = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.Use((innerClient, services) =>
        {
            ConversationPersistenceOptions options = new ConversationPersistenceOptions();
            configureOptions?.Invoke(options);

            repository ??= services.GetService<IConversationRepository>() ??
                           new InMemoryConversationRepository();

            loggerFactory ??= services.GetService<ILoggerFactory>() ?? NullLoggerFactory.Instance;

            return new ConversationPersistenceChatClient(innerClient,
                repository,
                options,
                loggerFactory);
        });
    }
}