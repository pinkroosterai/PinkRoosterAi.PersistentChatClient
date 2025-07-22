# PinkRoosterAi.PersistentChatClient

A .NET 9.0 library that provides conversation persistence capabilities for Microsoft.Extensions.AI chat clients. This library acts as a decorator around existing chat clients to automatically persist conversation history and manage conversation IDs with flexible storage options.

## Features

- **Decorator Pattern Integration**: Seamlessly wraps any `Microsoft.Extensions.AI.IChatClient`
- **Flexible Conversation ID Management**: Multiple strategies for automatic conversation ID generation
- **Multiple Storage Backends**: In-memory and Entity Framework Core support
- **Database Provider Support**: SQLite, PostgreSQL, and In-Memory providers
- **Comprehensive Content Type Support**: Handles all Microsoft.Extensions.AI content types
- **Streaming Support**: Works with both regular and streaming AI responses
- **Transaction Safety**: Database transactions ensure data consistency
- **Dependency Injection Ready**: Full support for .NET DI container

## Quick Start

### Installation

```console
dotnet add package PersistentChatClient
dotnet add package PersistentChatClient.EntityFramework
```

### Basic Usage

```csharp
using Microsoft.Extensions.AI;
using PersistentChatClient;

// Wrap any existing chat client
IChatClient client = yourExistingChatClient
    .AsBuilder()
    .UseConversationPersistence(
        loggerFactory: loggerFactory,
        repository: customRepository,
        options => {
            options.AutoCreationMode = AutoConversationCreationMode.GenerateWhenMissing;
            options.ContinueStreamingOnPersistenceFailure = true;
        })
    .Build();

// Use normally - conversation persistence is automatic
var response = await client.GetResponseAsync("Hello, how are you?");
```

### Entity Framework Setup

```csharp
using Microsoft.Extensions.DependencyInjection;

// Configure services
services.AddChatStorage(options => 
    options.UseSqlite("Data Source=conversations.db"));

// Repository is automatically registered and can be injected
```

## Architecture

The library implements a **Decorator Pattern** around `Microsoft.Extensions.AI.IChatClient`, providing:

### Core Components

- **`ConversationPersistenceChatClient`**: Main decorator implementation
- **`IConversationRepository`**: Core persistence abstraction
- **Repository Implementations**:
  - `InMemoryConversationRepository`: Thread-safe in-memory storage
  - `EntityConversationRepository`: Full database persistence via EF Core

### Conversation ID Generation Modes

- **`None`**: Requires explicit conversation IDs
- **`GenerateWhenMissing`**: Creates new GUID when ID is missing (default)
- **`HashSystemAndUserMessage`**: Generates deterministic hash-based IDs from system + user message content

## Entity Framework Integration

The library provides comprehensive Entity Framework Core support with:

### Data Model

- **Table-Per-Hierarchy (TPH)** pattern for content types
- **Performance-optimized indexes** for conversation and message queries
- **Comprehensive content type support**: Text, Reasoning, Data, Uri, Error, Function calls/results, Usage metrics

### Supported Database Providers

- **SQLite**: `Microsoft.EntityFrameworkCore.Sqlite.Core`
- **PostgreSQL**: `Npgsql.EntityFrameworkCore.PostgreSQL` 
- **In-Memory**: `Microsoft.EntityFrameworkCore.InMemory`

### Database Configuration

```csharp
// SQLite
services.AddChatStorage(options => 
    options.UseSqlite("Data Source=conversations.db"));

// PostgreSQL
services.AddChatStorage(options => 
    options.UseNpgsql("Host=localhost;Database=chats;Username=user;Password=pass"));

// In-Memory (for testing)
services.AddChatStorage(options => 
    options.UseInMemoryDatabase("TestDatabase"));
```

## Configuration Options

### Auto-Creation Modes

```csharp
options.AutoCreationMode = AutoConversationCreationMode.GenerateWhenMissing; // Default
options.AutoCreationMode = AutoConversationCreationMode.HashSystemAndUserMessage;
options.AutoCreationMode = AutoConversationCreationMode.None;
```

### Persistence Behavior

```csharp
// Continue streaming even if persistence fails (default: true)
options.ContinueStreamingOnPersistenceFailure = true;
```

## Content Type Support

The library handles all Microsoft.Extensions.AI content types:

- **Text Content**: Standard text messages
- **Reasoning Content**: AI reasoning/thinking processes
- **Data Content**: Structured data and binary content
- **Uri Content**: Reference links and resources
- **Error Content**: Error information and diagnostics
- **Function Call Content**: Tool/function invocations
- **Function Result Content**: Tool execution results
- **Usage Content**: Token usage and performance metrics

## Advanced Usage

### Custom Repository Implementation

```csharp
public class CustomRepository : IConversationRepository
{
    public async Task<ChatConversation> GetOrCreateConversationAsync(
        string conversationId,
        CancellationToken cancellationToken = default)
    {
        // Custom implementation
    }

    public async Task SaveMessagesAsync(
        string conversationId,
        IEnumerable<ChatMessage> messages,
        ChatCompletion completion,
        CancellationToken cancellationToken = default)
    {
        // Custom implementation
    }
}
```

### Dependency Injection Configuration

```csharp
var builder = Host.CreateApplicationBuilder();

// Register services
builder.Services.AddChatStorage(options => 
    options.UseSqlite("Data Source=conversations.db"));

builder.Services.AddChatClient(services =>
    new YourChatClient().AsIChatClient())
    .UseConversationPersistence();

var app = builder.Build();

// Use the configured client
var chatClient = app.Services.GetRequiredService<IChatClient>();
```

## Testing

The library includes comprehensive test coverage using:

- **xUnit**: Test framework
- **FluentAssertions**: Clear, readable assertions
- **Moq**: Mock-based testing
- **Entity Framework In-Memory**: Database testing

### Running Tests

```console
dotnet test
```

## Project Structure

```
PersistentChatClient/
├── src/
│   ├── PersistentChatClient/           # Core library
│   └── PersistentChatClient.EntityFramework/  # EF Core integration
└── tests/
    └── PersistentChatClient.Tests/     # Unit tests
```

## Dependencies

### Core Library
- `Microsoft.Extensions.AI` (9.7.1)
- `Microsoft.Extensions.DependencyInjection.Abstractions` (9.0.7)

### Entity Framework Integration
- `Microsoft.EntityFrameworkCore` (9.0.7)
- `Microsoft.EntityFrameworkCore.Sqlite.Core` (9.0.7)
- `Npgsql.EntityFrameworkCore.PostgreSQL` (9.0.4)

## Requirements

- **.NET 9.0** or later
- **Microsoft.Extensions.AI** integration

## Performance Characteristics

- **Database Indexes**: Optimized for conversation and message retrieval
- **Transaction Safety**: Proper database transaction handling
- **Memory Efficiency**: Minimal memory overhead for decoration pattern
- **Lazy Loading**: Virtual navigation properties for performance

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests for new functionality
5. Ensure all tests pass
6. Submit a pull request

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Changelog

### Version 1.0.0
- Initial release
- Core conversation persistence functionality
- Entity Framework integration
- Multiple database provider support
- Comprehensive content type support

---

**Note**: This library is designed to work seamlessly with any `Microsoft.Extensions.AI.IChatClient` implementation, providing transparent conversation persistence without changing your existing chat client code.