
using Microsoft.Extensions.VectorData;

namespace AgentStack.ChatHistory;

public enum ChatReducerTriggerEvent
{
    BeforeMessageAdded, AfterMessagesRetrieval
}

public sealed class State
{
    public State(string sessionDbKey)
    {
        this.SessionDbKey = sessionDbKey ?? throw new ArgumentNullException(nameof(sessionDbKey));
    }

    public string SessionDbKey { get; }
}

public sealed class VectorChatHistoryProvider : ChatHistoryProvider
{
    private readonly ProviderSessionState<State> _sessionState;
    private IReadOnlyList<string>? _stateKeys;
    private readonly VectorStore _vectorStore;
    private readonly IChatReducer? _chatReducer;
    private readonly ChatReducerTriggerEvent _reducerTriggerEvent;

    public VectorChatHistoryProvider(
            VectorStore vectorStore,
            Func<AgentSession?, State>? stateInitializer = null,
            string? stateKey = null)
            : this(vectorStore, stateInitializer, stateKey, null, ChatReducerTriggerEvent.AfterMessagesRetrieval)
    {

    }

    public VectorChatHistoryProvider(
        VectorStore vectorStore,
        Func<AgentSession?, State>? stateInitializer = null,
        string? stateKey = null,
        IChatReducer? chatReducer = null,
        ChatReducerTriggerEvent reducerTriggerEvent = ChatReducerTriggerEvent.AfterMessagesRetrieval)
    {
        Console.WriteLine($"VectorStore: {vectorStore.GetType()}");
        _vectorStore = vectorStore ?? throw new ArgumentNullException(nameof(vectorStore));
        _chatReducer = chatReducer;
        _reducerTriggerEvent = reducerTriggerEvent;
        this._sessionState = new ProviderSessionState<State>(
                stateInitializer ?? (_ => new State(Guid.NewGuid().ToString("N"))),
                stateKey ?? this.GetType().Name);
    }

    public string? SessionDbKey => _sessionState.StateKey;


    protected override async ValueTask StoreChatHistoryAsync(InvokedContext context, CancellationToken cancellationToken = default)
    {
        if (context.InvokeException is not null)
        {
            return;
        }

        var messages = context.RequestMessages.Concat(context.RequestMessages ?? []).Concat(context.ResponseMessages ?? []);
        if (_reducerTriggerEvent is ChatReducerTriggerEvent.BeforeMessageAdded && _chatReducer is not null)
        {
            messages = await _chatReducer.ReduceAsync(messages, cancellationToken).ConfigureAwait(false);
        }

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

    protected override async ValueTask<IEnumerable<ChatMessage>> ProvideChatHistoryAsync(InvokingContext context, CancellationToken cancellationToken = default)
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