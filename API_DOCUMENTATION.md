# PinkRoosterAi.PersistentChatClient API Documentation

## Overview

The PinkRoosterAi.PersistentChatClient is a .NET library that provides conversation persistence capabilities for Microsoft.Extensions.AI chat clients. It enables automatic storage and retrieval of chat conversations with support for both in-memory and Entity Framework-based persistence strategies.

## Core Components

### Namespaces

- `PinkRoosterAi.Framework.PersistentChatClient` - Core abstractions and implementations
- `PinkRoosterAi.Framework.PersistentChatClient.EntityFramework` - Entity Framework persistence implementation

## Core Abstractions

### IConversationRepository

The primary interface for conversation persistence operations.

```csharp
namespace PinkRoosterAi.Framework.PersistentChatClient.Abstractions

public interface IConversationRepository 
{
    /// <summary>
    /// Persists messages to a conversation.
    /// </summary>
    /// <param name="conversation">The conversation to update.</param>
    /// <param name="responseMessages">Response messages to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SaveMessagesAsync(Conversation conversation, IReadOnlyList<ChatMessage> responseMessages, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Retrieves a conversation by ID and adds new messages to it.
    /// </summary>
    /// <param name="conversationId">The conversation identifier.</param>
    /// <param name="newMessages">New messages to add to the conversation.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The conversation with new messages added, or a new conversation if not found.</returns>
    Task<Conversation> GetOrCreateConversationAsync(string conversationId, IReadOnlyList<ChatMessage> newMessages, CancellationToken cancellationToken = default);
}
```

## Core Models

### Conversation

Represents a chat conversation with its metadata.

```csharp
namespace PinkRoosterAi.Framework.PersistentChatClient.Models

public sealed class Conversation
{
    /// <summary>Gets or sets the unique conversation identifier.</summary>
    required public string Id { get; set; }

    /// <summary>Gets or sets the list of messages in the conversation.</summary>
    required public List<ChatMessage> Messages { get; set; }

    /// <summary>Gets or sets when the conversation was created.</summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>Gets or sets when the conversation was last updated.</summary>
    public DateTimeOffset LastUpdatedAt { get; set; }
}
```

### ConversationPersistenceOptions

Configuration options for conversation persistence behavior.

```csharp
namespace PinkRoosterAi.Framework.PersistentChatClient.Models

public sealed class ConversationPersistenceOptions
{
    /// <summary>Gets or sets the auto-creation mode for conversation IDs.</summary>
    public AutoConversationCreationMode AutoCreationMode { get; set; } = AutoConversationCreationMode.GenerateWhenMissing;

    /// <summary>Gets or sets whether to continue streaming on persistence failures.</summary>
    public bool ContinueStreamingOnPersistenceFailure { get; set; } = true;
}
```

### AutoConversationCreationMode

Enumeration defining how conversation IDs are automatically created.

```csharp
namespace PinkRoosterAi.Framework.PersistentChatClient.Models

public enum AutoConversationCreationMode
{
    /// <summary>Conversations must have explicit IDs - no automatic generation.</summary>
    None,

    /// <summary>Generates ID from hash of system and user messages when exactly 2 messages exist.</summary>
    HashSystemAndUserMessage,

    /// <summary>Generates a new ID when ConversationId is null or empty.</summary>
    GenerateWhenMissing
}
```

## Main Implementation Classes

### ConversationPersistenceChatClient

The primary chat client implementation that adds conversation persistence capabilities.

```csharp
namespace PinkRoosterAi.Framework.PersistentChatClient.Implementations

public sealed class ConversationPersistenceChatClient : DelegatingChatClient
{
    /// <summary>
    /// Initializes a new instance of the ConversationPersistenceChatClient class.
    /// </summary>
    /// <param name="innerClient">The underlying chat client.</param>
    /// <param name="repository">Repository for conversation persistence.</param>
    /// <param name="options">Configuration options for persistence behavior.</param>
    /// <param name="loggerFactory">Factory for creating loggers.</param>
    public ConversationPersistenceChatClient(
        IChatClient innerClient,
        IConversationRepository repository,
        ConversationPersistenceOptions? options = null,
        ILoggerFactory? loggerFactory = null)

    /// <summary>
    /// Gets a chat response with automatic conversation persistence.
    /// </summary>
    /// <param name="messages">The input messages.</param>
    /// <param name="options">Chat options including ConversationId.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Chat response with ConversationId set.</returns>
    public override async Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)

    /// <summary>
    /// Gets a streaming chat response with automatic conversation persistence.
    /// </summary>
    /// <param name="messages">The input messages.</param>
    /// <param name="options">Chat options including ConversationId.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Streaming chat response updates with ConversationId set.</returns>
    public override async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
}
```

### InMemoryConversationRepository

Default in-memory implementation of IConversationRepository.

```csharp
namespace PinkRoosterAi.Framework.PersistentChatClient.Implementations

public sealed class InMemoryConversationRepository : IConversationRepository
{
    /// <inheritdoc />
    public Task<Conversation> GetOrCreateConversationAsync(string conversationId, IReadOnlyList<ChatMessage> newMessages, CancellationToken cancellationToken = default)

    /// <inheritdoc />
    public Task SaveMessagesAsync(Conversation conversation, IReadOnlyList<ChatMessage> responseMessages, CancellationToken cancellationToken = default)

    /// <summary>
    /// Gets the total number of conversations stored in memory.
    /// </summary>
    public int ConversationCount { get; }

    /// <summary>
    /// Clears all conversations from memory.
    /// </summary>
    public void Clear()

    /// <summary>
    /// Checks if a conversation with the specified ID exists.
    /// </summary>
    /// <param name="conversationId">The conversation identifier.</param>
    /// <returns>True if the conversation exists; otherwise, false.</returns>
    public bool ContainsConversation(string conversationId)
}
```

## Extension Methods

### ChatClientBuilder Extensions

Extensions for configuring conversation persistence in the Microsoft.Extensions.AI pipeline.

```csharp
namespace PinkRoosterAi.Framework.PersistentChatClient.Extensions

public static class ConversationPersistenceChatClientBuilderExtensions
{
    /// <summary>
    /// Adds conversation persistence to the chat client pipeline.
    /// </summary>
    /// <param name="builder">The ChatClientBuilder instance.</param>
    /// <param name="loggerFactory">Optional logger factory.</param>
    /// <param name="repository">Optional conversation repository (defaults to InMemoryConversationRepository).</param>
    /// <param name="configureOptions">Optional configuration action for persistence options.</param>
    /// <returns>The configured ChatClientBuilder.</returns>
    public static ChatClientBuilder UseConversationPersistence(
        this ChatClientBuilder builder,
        ILoggerFactory? loggerFactory = null,
        IConversationRepository? repository = null,
        Action<ConversationPersistenceOptions>? configureOptions = null)
}
```

## Entity Framework Components

### IEntityChatRepository

Extended repository interface for Entity Framework-based persistence.

```csharp
namespace PinkRoosterAi.Framework.PersistentChatClient.EntityFramework.Abstractions

public interface IEntityChatRepository
{
    /// <summary>
    /// Saves a conversation with messages to the database.
    /// </summary>
    /// <param name="messages">Messages to save.</param>
    /// <param name="conversationId">Optional conversation ID.</param>
    /// <returns>Storage result with conversation GUID.</returns>
    Task<StorageResult<Guid>> SaveConversationAsync(IEnumerable<ChatMessage> messages, string? conversationId = null);

    /// <summary>
    /// Retrieves messages for a conversation.
    /// </summary>
    /// <param name="conversationId">The conversation identifier.</param>
    /// <returns>Storage result with messages.</returns>
    Task<StorageResult<IReadOnlyList<ChatMessage>>> GetMessagesAsync(string conversationId);

    /// <summary>
    /// Saves a chat response to the database.
    /// </summary>
    /// <param name="response">The chat response to save.</param>
    /// <param name="conversationId">The conversation GUID.</param>
    /// <returns>Storage result indicating success.</returns>
    Task<StorageResult<bool>> SaveResponseAsync(ChatResponse response, Guid conversationId);

    /// <summary>
    /// Retrieves a specific chat response.
    /// </summary>
    /// <param name="responseId">The response identifier.</param>
    /// <returns>Storage result with the chat response.</returns>
    Task<StorageResult<ChatResponse>> GetResponseAsync(string responseId);

    /// <summary>
    /// Gets a paginated list of conversation summaries.
    /// </summary>
    /// <param name="skip">Number of conversations to skip.</param>
    /// <param name="take">Number of conversations to retrieve.</param>
    /// <returns>Storage result with conversation information.</returns>
    Task<StorageResult<IReadOnlyList<ChatConversationInfo>>> GetConversationsAsync(int skip = 0, int take = 50);
}
```

### StorageResult<T>

Result wrapper for Entity Framework operations.

```csharp
namespace PinkRoosterAi.Framework.PersistentChatClient.EntityFramework.Entities

public record StorageResult<T>(bool Success, T? Data, string? ErrorMessage = null);
```

### ChatDbContext

Entity Framework database context for chat persistence.

```csharp
namespace PinkRoosterAi.Framework.PersistentChatClient.EntityFramework.Context

public class ChatDbContext : DbContext
{
    public ChatDbContext(DbContextOptions<ChatDbContext> options) : base(options)

    // Main entities
    public DbSet<ChatConversationEntity> Conversations { get; set; }
    public DbSet<ChatMessageEntity> Messages { get; set; }
    public DbSet<ChatResponseEntity> Responses { get; set; }

    // Content entities (supports different message content types)
    public DbSet<TextContentEntity> TextContents { get; set; }
    public DbSet<ReasoningContentEntity> ReasoningContents { get; set; }
    public DbSet<DataContentEntity> DataContents { get; set; }
    public DbSet<UriContentEntity> UriContents { get; set; }
    public DbSet<ErrorContentEntity> ErrorContents { get; set; }
    public DbSet<FunctionCallContentEntity> FunctionCallContents { get; set; }
    public DbSet<FunctionResultContentEntity> FunctionResultContents { get; set; }
    public DbSet<UsageContentEntity> UsageContents { get; set; }
}
```

### Entity Framework Entities

#### BaseEntity

Base class for all entities with common properties.

```csharp
namespace PinkRoosterAi.Framework.PersistentChatClient.EntityFramework.Entities

public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
```

#### ChatConversationEntity

Entity representing a chat conversation.

```csharp
namespace PinkRoosterAi.Framework.PersistentChatClient.EntityFramework.Entities

public class ChatConversationEntity : BaseEntity
{
    [MaxLength(500)]
    public string? ConversationId { get; set; }

    public DateTimeOffset LastModifiedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation properties
    public virtual ICollection<ChatMessageEntity> Messages { get; set; } = [];
    public virtual ICollection<ChatResponseEntity> Responses { get; set; } = [];
}
```

### Service Registration Extensions

Extensions for registering Entity Framework services.

```csharp
namespace PinkRoosterAi.Framework.PersistentChatClient.EntityFramework.Extensions

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers chat storage services with Entity Framework persistence.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureDb">Database context configuration action.</param>
    /// <returns>The configured service collection.</returns>
    public static IServiceCollection AddChatStorage(this IServiceCollection services, Action<DbContextOptionsBuilder> configureDb)
}
```

## Usage Examples

### Basic Usage with In-Memory Storage

```csharp
using Microsoft.Extensions.AI;
using PinkRoosterAi.Framework.PersistentChatClient.Extensions;

// Configure chat client with conversation persistence
var chatClient = new ChatClientBuilder()
    .UseChatClient(new OpenAIChatClient(model: "gpt-4", apiKey: "your-api-key"))
    .UseConversationPersistence()  // Uses InMemoryConversationRepository by default
    .Build();

// Use the client - conversations will be automatically persisted
var options = new ChatOptions { ConversationId = "my-conversation-1" };
var response = await chatClient.GetResponseAsync([
    new ChatMessage(ChatRole.User, "Hello, how are you?")
], options);

// Continue the conversation
var followUpResponse = await chatClient.GetResponseAsync([
    new ChatMessage(ChatRole.User, "Tell me a joke")
], options);
```

### Custom Configuration

```csharp
var chatClient = new ChatClientBuilder()
    .UseChatClient(new OpenAIChatClient(model: "gpt-4", apiKey: "your-api-key"))
    .UseConversationPersistence(
        configureOptions: options =>
        {
            options.AutoCreationMode = AutoConversationCreationMode.HashSystemAndUserMessage;
            options.ContinueStreamingOnPersistenceFailure = false;
        })
    .Build();
```

### Entity Framework Integration

```csharp
// Register services
services.AddChatStorage(options =>
{
    options.UseSqlServer(connectionString);
});

// Use with dependency injection
public class ChatService
{
    private readonly IChatClient _chatClient;
    
    public ChatService(IChatClient chatClient)
    {
        _chatClient = chatClient;
    }
    
    public async Task<string> GetResponseAsync(string message, string conversationId)
    {
        var options = new ChatOptions { ConversationId = conversationId };
        var response = await _chatClient.GetResponseAsync([
            new ChatMessage(ChatRole.User, message)
        ], options);
        
        return response.Message.Text ?? string.Empty;
    }
}
```

### Streaming with Persistence

```csharp
var options = new ChatOptions { ConversationId = "streaming-conversation" };
var messages = new[] { new ChatMessage(ChatRole.User, "Write a long story") };

await foreach (var update in chatClient.GetStreamingResponseAsync(messages, options))
{
    Console.Write(update.Text);
    // Conversation is automatically persisted when streaming completes
}
```

### Custom Repository Implementation

```csharp
public class CustomConversationRepository : IConversationRepository
{
    public async Task<Conversation> GetOrCreateConversationAsync(
        string conversationId, 
        IReadOnlyList<ChatMessage> newMessages, 
        CancellationToken cancellationToken = default)
    {
        // Custom implementation
        // Return existing conversation or create new one
    }

    public async Task SaveMessagesAsync(
        Conversation conversation, 
        IReadOnlyList<ChatMessage> responseMessages, 
        CancellationToken cancellationToken = default)
    {
        // Custom implementation
        // Persist response messages to your storage
    }
}

// Use custom repository
var chatClient = new ChatClientBuilder()
    .UseChatClient(new OpenAIChatClient(model: "gpt-4", apiKey: "your-api-key"))
    .UseConversationPersistence(repository: new CustomConversationRepository())
    .Build();
```

## Key Features

1. **Automatic Persistence** - Conversations are automatically saved without manual intervention
2. **Flexible ID Generation** - Support for automatic, hash-based, or manual conversation ID creation
3. **Streaming Support** - Full support for streaming responses with persistence
4. **Entity Framework Integration** - Rich database persistence with complex content type support
5. **Extensible Repository Pattern** - Easy to implement custom storage backends
6. **Error Handling** - Configurable behavior for persistence failures during streaming
7. **Logging Integration** - Built-in logging support for monitoring and debugging
8. **Thread-Safe Operations** - Concurrent conversation access supported

## Thread Safety

- `InMemoryConversationRepository` uses `ConcurrentDictionary` for thread-safe operations
- `ConversationPersistenceChatClient` is thread-safe for concurrent requests
- Entity Framework implementations should follow EF Core threading guidelines

## Performance Considerations

- In-memory storage has O(1) lookup time but limited to application lifetime
- Entity Framework implementation supports database indexing for optimal query performance
- Large conversations may impact memory usage - consider implementing conversation trimming for production scenarios
- Streaming persistence happens asynchronously to minimize impact on response time