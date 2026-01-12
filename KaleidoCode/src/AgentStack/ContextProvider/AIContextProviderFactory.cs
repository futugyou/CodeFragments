
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

    public AIContextProvider Create(ChatClientAgentOptions.AIContextProviderFactoryContext ctx)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        var request = httpContext?.Request;
        var threadId = request?.Headers != null ? request?.Headers["ThreadId"].FirstOrDefault() : "";
        var userId = request?.Headers != null ? request?.Headers["UserId"].FirstOrDefault() : "";

        Console.WriteLine($"userId: {userId}, threadId: {threadId}");
        return new ChatHistoryMemoryProvider(
            _vectorStore,
            collectionName: "chathistory_memory",
            vectorDimensions: 1536,
            storageScope: new() { UserId = userId, ThreadId = threadId },
            searchScope: new() { UserId = userId });
    }
}