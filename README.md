<div align="center">
  <img src="img/logo_transparent.png" alt="PinkRoosterAi.PersistentChatClient Logo" width="300">
  
  # PinkRoosterAi.PersistentChatClient
  
  **A robust, flexible conversation persistence library for Microsoft.Extensions.AI chat clients**
  
  [![.NET](https://img.shields.io/badge/.NET-9.0-blue)](https://dotnet.microsoft.com/)
  [![NuGet](https://img.shields.io/badge/NuGet-1.0.0-orange)](https://www.nuget.org/packages/PinkRoosterAi.PersistentChatClient)
  [![License](https://img.shields.io/badge/License-MIT-green)](LICENSE)
  
</div>

---

## 🚀 Overview

PersistentChatClient is a production-ready .NET library that seamlessly adds conversation persistence to any `Microsoft.Extensions.AI.IChatClient` implementation. Built with the **decorator pattern**, it provides transparent conversation history management, flexible storage backends, and intelligent conversation ID generation without changing your existing chat client code.

### Key Features

- 🎭 **Decorator Pattern Integration** - Seamlessly wraps any existing `IChatClient` implementation
- 🔌 **Flexible Storage Backends** - In-memory, Entity Framework Core with SQLite, PostgreSQL support
- 🎯 **Smart Conversation ID Management** - Multiple automatic generation strategies
- 📊 **Complete Content Type Support** - Handles all Microsoft.Extensions.AI content types
- ⚡ **Streaming Support** - Works with both regular and streaming AI responses
- 🛡️ **Transaction Safety** - Database transactions ensure data consistency
- 🔗 **Dependency Injection Ready** - Full .NET DI container integration

---

## 📦 Installation

### Package Manager
```powershell
Install-Package PinkRoosterAi.PersistentChatClient
Install-Package PinkRoosterAi.PersistentChatClient.EntityFramework
```

### .NET CLI
```bash
dotnet add package PinkRoosterAi.PersistentChatClient
dotnet add package PinkRoosterAi.PersistentChatClient.EntityFramework
```

### PackageReference
```xml
<PackageReference Include="PinkRoosterAi.PersistentChatClient" Version="1.0.0" />
<PackageReference Include="PinkRoosterAi.PersistentChatClient.EntityFramework" Version="1.0.0" />
```

---

## 🏗️ Architecture

PersistentChatClient implements a **decorator pattern** around `Microsoft.Extensions.AI.IChatClient`:

```
┌─────────────────────────────────────┐
│    ConversationPersistenceChatClient │ ← Decorator wrapping IChatClient
│  ┌─────────────────────────────────┐ │
│  │      Original IChatClient       │ │ ← Your existing chat client
│  └─────────────────────────────────┘ │
└─────────────────────────────────────┘
                   │
                   ▼
┌─────────────────────────────────────┐
│      IConversationRepository        │ ← Persistence abstraction
├─────────────────────────────────────┤
│  InMemoryConversationRepository     │ ← Thread-safe in-memory storage
│  EntityConversationRepository       │ ← Full database persistence
│  CustomConversationRepository       │ ← Your custom implementation
└─────────────────────────────────────┘
```

### Core Components

- **`ConversationPersistenceChatClient`** - Main decorator providing transparent persistence
- **`IConversationRepository`** - Storage abstraction for conversation management
- **Repository Implementations**:
  - `InMemoryConversationRepository` - Thread-safe in-memory storage
  - `EntityConversationRepository` - Entity Framework Core database persistence
- **`AutoConversationCreationMode`** - Intelligent conversation ID generation strategies

---

## ⚡ Quick Start

### Basic Usage with Any Chat Client
```csharp
using Microsoft.Extensions.AI;
using PinkRoosterAi.PersistentChatClient;

// Wrap any existing chat client with persistence
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

// Use normally - conversation persistence happens automatically
var response = await client.GetResponseAsync("Hello, how are you?");
var followUp = await client.GetResponseAsync("What's the weather like?");
// Both messages and responses are automatically persisted
```

### Entity Framework Setup
```csharp
using Microsoft.Extensions.DependencyInjection;
using PinkRoosterAi.PersistentChatClient.EntityFramework;

var builder = Host.CreateApplicationBuilder();

// Configure chat storage with automatic migrations
builder.Services.AddChatStorage(options => 
    options.UseSqlite("Data Source=conversations.db"));

// Register your chat client with persistence
builder.Services.AddChatClient(services =>
    new YourChatClient().AsIChatClient())
    .UseConversationPersistence();

var app = builder.Build();

// Repository is automatically registered and available for injection
var chatClient = app.Services.GetRequiredService<IChatClient>();
var repository = app.Services.GetRequiredService<IConversationRepository>();
```

### Database Provider Configuration
```csharp
// SQLite (recommended for single-user applications)
services.AddChatStorage(options => 
    options.UseSqlite("Data Source=conversations.db"));

// PostgreSQL (recommended for multi-user applications)
services.AddChatStorage(options => 
    options.UseNpgsql("Host=localhost;Database=chats;Username=user;Password=pass"));

// In-Memory (perfect for testing)
services.AddChatStorage(options => 
    options.UseInMemoryDatabase("TestDatabase"));
```

---

## 🎛️ Configuration

### Conversation ID Generation Modes
```csharp
// Generate new GUID when conversation ID is missing (default)
options.AutoCreationMode = AutoConversationCreationMode.GenerateWhenMissing;

// Create deterministic hash-based IDs from system + user message content
options.AutoCreationMode = AutoConversationCreationMode.HashSystemAndUserMessage;

// Require explicit conversation IDs (throws if missing)
options.AutoCreationMode = AutoConversationCreationMode.None;
```

### Persistence Behavior Options
```csharp
builder.Services.AddChatClient(services => yourChatClient)
    .UseConversationPersistence(options =>
    {
        // Continue streaming even if persistence fails (default: true)
        options.ContinueStreamingOnPersistenceFailure = true;
        
        // Set conversation ID generation strategy
        options.AutoCreationMode = AutoConversationCreationMode.GenerateWhenMissing;
    });
```

---

## 🔧 Advanced Usage

### Custom Repository Implementation
```csharp
public class CustomConversationRepository : IConversationRepository
{
    private readonly IYourStorageSystem _storage;

    public CustomConversationRepository(IYourStorageSystem storage)
    {
        _storage = storage;
    }

    public async Task<ChatConversation> GetOrCreateConversationAsync(
        string conversationId,
        CancellationToken cancellationToken = default)
    {
        var existing = await _storage.FindConversationAsync(conversationId);
        if (existing != null)
            return existing;

        var newConversation = new ChatConversation
        {
            ConversationId = conversationId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Messages = new List<ChatMessage>()
        };

        await _storage.SaveConversationAsync(newConversation);
        return newConversation;
    }

    public async Task SaveMessagesAsync(
        string conversationId,
        IEnumerable<ChatMessage> messages,
        ChatCompletion completion,
        CancellationToken cancellationToken = default)
    {
        var conversation = await GetOrCreateConversationAsync(conversationId, cancellationToken);
        
        // Add new messages to conversation
        foreach (var message in messages)
        {
            conversation.Messages.Add(message);
        }
        
        conversation.UpdatedAt = DateTime.UtcNow;
        await _storage.SaveConversationAsync(conversation);
    }
}
```

### Explicit Conversation Management
```csharp
// Use specific conversation ID
var options = new ChatOptions
{
    AdditionalProperties = new Dictionary<string, object>
    {
        ["ConversationId"] = "user-session-12345"
    }
};

var response = await client.GetResponseAsync("Continue our previous discussion", options);

// Generate deterministic conversation IDs based on content
var client = originalClient
    .AsBuilder()
    .UseConversationPersistence(options =>
    {
        options.AutoCreationMode = AutoConversationCreationMode.HashSystemAndUserMessage;
    })
    .Build();

// Same system + user message combination will always use same conversation
var response1 = await client.GetResponseAsync("What is machine learning?");
var response2 = await client.GetResponseAsync("What is machine learning?"); 
// Both responses will be in the same conversation
```

### Streaming Support
```csharp
// Streaming responses are automatically persisted
await foreach (var update in client.GetStreamingResponseAsync("Tell me a long story"))
{
    Console.Write(update.Content);
    // Each streaming chunk is automatically captured
    // Final complete response is persisted to conversation
}
```

---

## 📊 Content Type Support

The library handles all Microsoft.Extensions.AI content types seamlessly:

### Supported Content Types
| Content Type | Description | Persistence Behavior |
|--------------|-------------|---------------------|
| **Text Content** | Standard text messages | Full content preserved |
| **Reasoning Content** | AI reasoning/thinking processes | Complete reasoning chain stored |
| **Data Content** | Structured data and binary content | Serialized and stored |
| **Uri Content** | Reference links and resources | URI and metadata preserved |
| **Error Content** | Error information and diagnostics | Error details and context saved |
| **Function Call Content** | Tool/function invocations | Call parameters and metadata stored |
| **Function Result Content** | Tool execution results | Results and execution context preserved |
| **Usage Content** | Token usage and performance metrics | Complete usage statistics tracked |

### Advanced Content Example
```csharp
// All content types are automatically handled
var messages = new List<ChatMessage>
{
    new ChatMessage(ChatRole.System, "You are a helpful assistant."),
    new ChatMessage(ChatRole.User, "Analyze this data and call the weather API"),
    new ChatMessage(ChatRole.Assistant, new FunctionCallContent("get_weather", new { city = "Paris" })),
    new ChatMessage(ChatRole.Tool, new FunctionResultContent("get_weather", "Sunny, 22°C")),
    new ChatMessage(ChatRole.Assistant, "The weather in Paris is sunny and 22°C.")
};

// All message types are preserved in conversation history
var response = await client.GetResponseAsync(messages);
```

---

## 🛠️ Database Schema

The Entity Framework integration uses an optimized **Table-Per-Hierarchy (TPH)** pattern:

### Database Tables
```sql
-- Conversations table
CREATE TABLE Conversations (
    ConversationId NVARCHAR(450) PRIMARY KEY,
    CreatedAt DATETIME2 NOT NULL,
    UpdatedAt DATETIME2 NOT NULL
);

-- Messages table with TPH for content types
CREATE TABLE Messages (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    ConversationId NVARCHAR(450) NOT NULL,
    Role NVARCHAR(MAX) NOT NULL,
    ContentType NVARCHAR(MAX) NOT NULL,
    -- Content-specific columns
    TextContent NVARCHAR(MAX),
    DataContent NVARCHAR(MAX),
    UriValue NVARCHAR(MAX),
    -- ... additional content type columns
    CreatedAt DATETIME2 NOT NULL,
    FOREIGN KEY (ConversationId) REFERENCES Conversations(ConversationId)
);

-- Performance indexes
CREATE INDEX IX_Messages_ConversationId ON Messages(ConversationId);
CREATE INDEX IX_Messages_CreatedAt ON Messages(CreatedAt);
```

### Performance Characteristics
- **Optimized indexes** for conversation and message retrieval
- **Virtual navigation properties** for lazy loading
- **Bulk insert optimization** for batch message saving
- **Transaction isolation** ensuring data consistency

---

## 🧪 Testing

### Unit Testing with Mock Repository
```csharp
[Test]
public async Task Should_Persist_Messages_Automatically()
{
    // Arrange
    var mockRepository = new Mock<IConversationRepository>();
    var mockChatClient = new Mock<IChatClient>();
    
    mockChatClient
        .Setup(c => c.GetResponseAsync(It.IsAny<IEnumerable<ChatMessage>>(), It.IsAny<ChatOptions>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(new ChatCompletion(new ChatMessage(ChatRole.Assistant, "Test response")));

    var persistentClient = new ConversationPersistenceChatClient(
        mockChatClient.Object, 
        mockRepository.Object,
        new ConversationPersistenceOptions(),
        NullLogger<ConversationPersistenceChatClient>.Instance);

    // Act
    await persistentClient.GetResponseAsync("Hello");

    // Assert
    mockRepository.Verify(r => r.SaveMessagesAsync(
        It.IsAny<string>(), 
        It.IsAny<IEnumerable<ChatMessage>>(), 
        It.IsAny<ChatCompletion>(), 
        It.IsAny<CancellationToken>()), Times.Once);
}
```

### Integration Testing
```csharp
[Test]
public async Task Should_Load_Conversation_History()
{
    // Arrange
    var options = new DbContextOptionsBuilder<ConversationDbContext>()
        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
        .Options;

    await using var context = new ConversationDbContext(options);
    var repository = new EntityConversationRepository(context);
    
    // Create initial conversation
    var conversation = await repository.GetOrCreateConversationAsync("test-conv");
    await repository.SaveMessagesAsync("test-conv", 
        new[] { new ChatMessage(ChatRole.User, "Hello") },
        new ChatCompletion(new ChatMessage(ChatRole.Assistant, "Hi there!")));

    // Act - Load conversation in new context
    await using var newContext = new ConversationDbContext(options);
    var newRepository = new EntityConversationRepository(newContext);
    var loadedConversation = await newRepository.GetOrCreateConversationAsync("test-conv");

    // Assert
    Assert.AreEqual(2, loadedConversation.Messages.Count);
    Assert.AreEqual("Hello", loadedConversation.Messages[0].Content.First().ToString());
}
```

---

## 📋 API Reference

### ConversationPersistenceChatClient

| Method | Description | Returns |
|--------|-------------|---------|
| `GetResponseAsync(string, ChatOptions?, CancellationToken)` | Get response with automatic persistence | `Task<ChatCompletion>` |
| `GetResponseAsync(IEnumerable<ChatMessage>, ChatOptions?, CancellationToken)` | Get response for message list with persistence | `Task<ChatCompletion>` |
| `GetStreamingResponseAsync(string, ChatOptions?, CancellationToken)` | Get streaming response with persistence | `IAsyncEnumerable<StreamingChatCompletionUpdate>` |
| `GetStreamingResponseAsync(IEnumerable<ChatMessage>, ChatOptions?, CancellationToken)` | Get streaming response for messages with persistence | `IAsyncEnumerable<StreamingChatCompletionUpdate>` |

### IConversationRepository

| Method | Description | Returns |
|--------|-------------|---------|
| `GetOrCreateConversationAsync(string, CancellationToken)` | Retrieve or create conversation by ID | `Task<ChatConversation>` |
| `SaveMessagesAsync(string, IEnumerable<ChatMessage>, ChatCompletion, CancellationToken)` | Persist messages and completion | `Task` |

### Configuration Options

| Property | Description | Default |
|----------|-------------|---------|
| `AutoCreationMode` | Conversation ID generation strategy | `GenerateWhenMissing` |
| `ContinueStreamingOnPersistenceFailure` | Continue streaming if persistence fails | `true` |

---

## 🔧 Requirements

- **.NET 9.0** or later
- **Dependencies**:
  - Microsoft.Extensions.AI (≥9.7.1)
  - Microsoft.Extensions.DependencyInjection.Abstractions (≥9.0.7)
  
### Entity Framework Package
- **Additional Dependencies**:
  - Microsoft.EntityFrameworkCore (≥9.0.7)
  - Microsoft.EntityFrameworkCore.Sqlite.Core (≥9.0.7)
  - Npgsql.EntityFrameworkCore.PostgreSQL (≥9.0.4)

---

## 📚 Examples

### Real-World Usage Scenarios

#### Customer Support Chatbot
```csharp
// Configure persistent chat for customer support
var supportClient = openAiClient
    .AsBuilder()
    .UseConversationPersistence(options =>
    {
        options.AutoCreationMode = AutoConversationCreationMode.GenerateWhenMissing;
    })
    .Build();

// Each customer interaction maintains conversation history
var response = await supportClient.GetResponseAsync(
    "I'm having trouble with my order #12345",
    new ChatOptions 
    { 
        AdditionalProperties = new() { ["ConversationId"] = $"customer-{customerId}" }
    });
```

#### Multi-Turn AI Assistant
```csharp
// Configure AI assistant with conversation persistence
services.AddChatStorage(options => 
    options.UseNpgsql(connectionString));

services.AddChatClient(services => 
    new OpenAIClient(apiKey).AsChatClient("gpt-4"))
    .UseConversationPersistence(options =>
    {
        options.AutoCreationMode = AutoConversationCreationMode.HashSystemAndUserMessage;
    });

// Assistant maintains context across sessions
var assistant = serviceProvider.GetRequiredService<IChatClient>();
var response1 = await assistant.GetResponseAsync("My name is John. Help me plan a trip to Paris.");
var response2 = await assistant.GetResponseAsync("What restaurants should I visit there?");
// Second response will reference John's name and Paris trip context
```

#### Educational Tutoring System
```csharp
// Configure tutoring system with session persistence
var tutorClient = anthropicClient
    .AsBuilder()
    .UseConversationPersistence(options =>
    {
        options.AutoCreationMode = AutoConversationCreationMode.GenerateWhenMissing;
        options.ContinueStreamingOnPersistenceFailure = true;
    })
    .Build();

// Each student gets persistent learning conversations
await foreach (var update in tutorClient.GetStreamingResponseAsync(
    "Explain quantum physics in simple terms",
    new ChatOptions
    {
        AdditionalProperties = new() { ["ConversationId"] = $"student-{studentId}-physics" }
    }))
{
    await SendToStudent(update.Content);
    // Learning progress is automatically tracked
}
```

---

## 🤝 Contributing

We welcome contributions! Please see our [Contributing Guidelines](CONTRIBUTING.md) for details.

### Development Setup
```bash
git clone https://github.com/pinkroosterai/PersistentChatClient.git
cd PersistentChatClient
dotnet restore
dotnet build
dotnet test
```

### Project Structure
```
PersistentChatClient/
├── src/
│   ├── PersistentChatClient/                    # Core library
│   └── PersistentChatClient.EntityFramework/    # EF Core integration
└── tests/
    └── PersistentChatClient.Tests/              # Unit tests
```

---

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 🆘 Support

- **Issues**: [GitHub Issues](https://github.com/pinkroosterai/PersistentChatClient/issues)
- **Documentation**: [GitHub Wiki](https://github.com/pinkroosterai/PersistentChatClient/wiki)
- **NuGet Package**: [PinkRoosterAi.PersistentChatClient](https://www.nuget.org/packages/PinkRoosterAi.PersistentChatClient)

---

<div align="center">
  <sub>Built with ❤️ by <a href="https://github.com/pinkroosterai">PinkRoosterAI</a></sub>
</div>
