
using Microsoft.Extensions.VectorData;

namespace AgentStack.ChatHistory;

public enum ChatReducerTriggerEvent
{
    BeforeMessageAdded, AfterMessagesRetrieval
}

public sealed class VectorChatHistoryProvider : ChatHistoryProvider
{
    private readonly VectorStore _vectorStore;
    private readonly IChatReducer? _chatReducer;
    private readonly ChatReducerTriggerEvent _reducerTriggerEvent;

    public VectorChatHistoryProvider(
            VectorStore vectorStore,
            JsonElement serializedStoreState,
            JsonSerializerOptions? jsonSerializerOptions = null)
            : this(vectorStore, serializedStoreState, jsonSerializerOptions, null, ChatReducerTriggerEvent.AfterMessagesRetrieval)
    {

    }

    public VectorChatHistoryProvider(
        VectorStore vectorStore,
        JsonElement serializedStoreState,
        JsonSerializerOptions? jsonSerializerOptions = null,
        IChatReducer? chatReducer = null,
        ChatReducerTriggerEvent reducerTriggerEvent = ChatReducerTriggerEvent.AfterMessagesRetrieval)
    {
        Console.WriteLine($"VectorStore: {vectorStore.GetType()}");
        _vectorStore = vectorStore ?? throw new ArgumentNullException(nameof(vectorStore));
        _chatReducer = chatReducer;
        _reducerTriggerEvent = reducerTriggerEvent;
        if (serializedStoreState.ValueKind is JsonValueKind.String)
        {
            SessionDbKey = serializedStoreState.Deserialize<string>();
            Console.WriteLine($"SessionDbKey: {SessionDbKey}");
        }
    }

    public string? SessionDbKey { get; private set; }

    public override async ValueTask InvokedAsync(InvokedContext context, CancellationToken cancellationToken = default)
    {
        if (context.InvokeException is not null)
        {
            return;
        }

        var messages = context.RequestMessages.Concat(context.AIContextProviderMessages ?? []).Concat(context.ResponseMessages ?? []);
        if (_reducerTriggerEvent is ChatReducerTriggerEvent.BeforeMessageAdded && _chatReducer is not null)
        {
            messages = await _chatReducer.ReduceAsync(messages, cancellationToken).ConfigureAwait(false);
        }

        SessionDbKey ??= Guid.NewGuid().ToString("N");
        var collection = _vectorStore.GetCollection<string, ChatHistoryItem>("agent_chat_history");
        await collection.EnsureCollectionExistsAsync(cancellationToken);
        await collection.UpsertAsync(messages.Select(x => new ChatHistoryItem()
        {
            Key = SessionDbKey + (x.MessageId ?? Guid.NewGuid().ToString("N")),
            Timestamp = DateTimeOffset.UtcNow,
            SessionId = SessionDbKey,
            SerializedMessage = JsonSerializer.Serialize(x),
            MessageText = x.Text
        }), cancellationToken);
    }

    public override async ValueTask<IEnumerable<ChatMessage>> InvokingAsync(InvokingContext context, CancellationToken cancellationToken = default)
    {
        var collection = _vectorStore.GetCollection<string, ChatHistoryItem>("agent_chat_history");
        await collection.EnsureCollectionExistsAsync(cancellationToken);
        var records = collection
            .GetAsync(
                x => x.SessionId == SessionDbKey, 10,
                new() { OrderBy = x => x.Descending(y => y.Timestamp) },
                cancellationToken);

        List<ChatMessage> messages = [];
        await foreach (var record in records)
        {
            messages.Add(JsonSerializer.Deserialize<ChatMessage>(record.SerializedMessage!)!);
        }

        messages.Reverse();
        if (_reducerTriggerEvent is ChatReducerTriggerEvent.AfterMessagesRetrieval && _chatReducer is not null)
        {
            messages = [.. await _chatReducer.ReduceAsync(messages, cancellationToken).ConfigureAwait(false)];
        }

        return messages;
    }

    public override JsonElement Serialize(JsonSerializerOptions? jsonSerializerOptions = null) =>
        // We have to serialize the session id, so that on deserialization you can retrieve the messages using the same session id.
        JsonSerializer.SerializeToElement(SessionDbKey);

    private sealed class ChatHistoryItem
    {
        [VectorStoreKey]
        public string? Key { get; set; }
        [VectorStoreData]
        public string? SessionId { get; set; }
        [VectorStoreData]
        public DateTimeOffset? Timestamp { get; set; }
        [VectorStoreData]
        public string? SerializedMessage { get; set; }
        [VectorStoreData]
        public string? MessageText { get; set; }
    }
}