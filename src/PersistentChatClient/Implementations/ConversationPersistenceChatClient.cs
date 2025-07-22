// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.CompilerServices;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using PinkRoosterAi.Framework.PersistentChatClient.Abstractions;
using PinkRoosterAi.Framework.PersistentChatClient.Models;

namespace PinkRoosterAi.Framework.PersistentChatClient.Implementations;

/// <summary>

/// </summary>
public sealed class ConversationPersistenceChatClient : DelegatingChatClient
{
    private readonly ConversationIdGenerator _idGenerator;
    private readonly ILogger<ConversationPersistenceChatClient> _logger;
    private readonly ConversationPersistenceOptions _options;
    private readonly IConversationRepository _repository;

    /// <summary>
    ///     Initializes a new instance of the ConversationPersistenceChatClient class.
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
        : base(innerClient)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _options = options ?? new ConversationPersistenceOptions();
        _idGenerator = new ConversationIdGenerator(_options.AutoCreationMode);

        loggerFactory ??= NullLoggerFactory.Instance;
        _logger = loggerFactory.CreateLogger<ConversationPersistenceChatClient>();
    }

    /// <inheritdoc />
    public override async Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<ChatMessage> messageList = ValidateAndConvertMessages(messages);
        ChatOptions chatOptions = options ?? new ChatOptions();

        string? conversationId = GetOrGenerateConversationId(chatOptions.ConversationId,
            messageList);

        if (conversationId == null && _options.AutoCreationMode == AutoConversationCreationMode.None)
        {
            throw new InvalidOperationException("ConversationId is required when AutoConversationCreationMode is None.");
        }

        conversationId ??= Guid.NewGuid().ToString("N"); // Final fallback
        chatOptions.ConversationId = conversationId;

        try
        {
            Conversation conversation = await _repository.GetOrCreateConversationAsync(conversationId,
                messageList,
                cancellationToken);

            ChatResponse response = await base.GetResponseAsync(conversation.Messages,
                chatOptions,
                cancellationToken);

            response.ConversationId = conversationId;

            await _repository.SaveMessagesAsync(conversation,
                response.Messages.ToList(),
                cancellationToken);

            return response;
        }
        catch (Exception ex) when (ShouldLogAndRethrow(ex,
                                       conversationId))
        {
            throw;
        }
    }

    /// <inheritdoc />
    public override async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        IReadOnlyList<ChatMessage> messageList = ValidateAndConvertMessages(messages);
        ChatOptions chatOptions = options ?? new ChatOptions();

        string? conversationId = GetOrGenerateConversationId(chatOptions.ConversationId,
            messageList);

        if (conversationId == null && _options.AutoCreationMode == AutoConversationCreationMode.None)
        {
            throw new InvalidOperationException("ConversationId is required when AutoConversationCreationMode is None.");
        }

        conversationId ??= Guid.NewGuid().ToString("N"); // Final fallback
        chatOptions.ConversationId = conversationId;

        Conversation conversation;

        try
        {
            conversation = await _repository.GetOrCreateConversationAsync(conversationId,
                messageList,
                cancellationToken);
        }
        catch (Exception ex) when (ShouldLogAndRethrow(ex,
                                       conversationId))
        {
            throw;
        }

        List<ChatResponseUpdate> updates = new List<ChatResponseUpdate>();

        await foreach (ChatResponseUpdate update in base.GetStreamingResponseAsync(conversation.Messages,
                           chatOptions,
                           cancellationToken))
        {
            updates.Add(update);

            yield return update;
        }

        await PersistStreamingResponse(conversation,
            updates,
            conversationId);
    }

    private static IReadOnlyList<ChatMessage> ValidateAndConvertMessages(IEnumerable<ChatMessage> messages)
    {
        ArgumentNullException.ThrowIfNull(messages);

        IReadOnlyList<ChatMessage> messageList = messages as IReadOnlyList<ChatMessage> ?? messages.ToList();

        if (messageList.Count == 0)
        {
            throw new ArgumentException("Messages collection cannot be empty.",
                nameof(messages));
        }

        return messageList;
    }

    private string? GetOrGenerateConversationId(string? conversationId, IReadOnlyList<ChatMessage> messages)
    {
        return _idGenerator.GenerateIdIfNeeded(conversationId,
            messages);
    }

    private async Task PersistStreamingResponse(Conversation conversation, List<ChatResponseUpdate> updates, string conversationId)
    {
        if (updates.Count == 0)
        {
            _logger.LogWarning("No streaming updates received for conversation {ConversationId}",
                conversationId);

            return;
        }

        try
        {
            ChatResponse response = updates.ToChatResponse();

            await _repository.SaveMessagesAsync(conversation,
                response.Messages.ToList(),
                CancellationToken.None);
        }
        catch (Exception ex) when (_options.ContinueStreamingOnPersistenceFailure)
        {
            _logger.LogError(ex,
                "Failed to persist streaming response for conversation {ConversationId}",
                conversationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to persist streaming response for conversation {ConversationId}",
                conversationId);

            throw;
        }
    }

    private bool ShouldLogAndRethrow(Exception ex, string? conversationId)
    {
        _logger.LogError(ex,
            "Error processing chat request for conversation {ConversationId}",
            conversationId);

        return false; // Always rethrow
    }
}