
using Microsoft.Agents.AI;
using Microsoft.Extensions.VectorData;

namespace AgentStack.ContextProvider;

public class AIContextProviderFactory
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly VectorStore _vectorStore;
    public AIContextProviderFactory(IHttpContextAccessor httpContextAccessor,
        [FromKeyedServices("AgentVectorStore")] VectorStore vectorStore)
    {
        _httpContextAccessor = httpContextAccessor;
        _vectorStore = vectorStore;
    }

    public ValueTask<AIContextProvider> Create(ChatClientAgentOptions.AIContextProviderFactoryContext ctx, CancellationToken ct)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        var request = httpContext?.Request;
        var SessionId = request?.Headers != null ? request?.Headers["SessionId"].FirstOrDefault() : "";
        var userId = request?.Headers != null ? request?.Headers["UserId"].FirstOrDefault() : "";

        Console.WriteLine($"userId: {userId}, sessionId: {SessionId}");
        return ValueTask.FromResult<AIContextProvider>(new ChatHistoryMemoryProvider(
            _vectorStore,
            collectionName: "chathistory_memory",
            vectorDimensions: 1536,
            storageScope: new() { UserId = userId, SessionId = SessionId },
            searchScope: new() { UserId = userId }));
    }
}