
using AgentStack.Services;
using AgentStack.ContextProvider;
using AgentStack.Skills;
using AgentStack.ThreadStore;
using AgentStack.Middleware;
using Microsoft.SemanticKernel.Connectors.PgVector;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting;
using AgentStack.MessageStore;
using Microsoft.Extensions.VectorData;
using Npgsql;

namespace AgentStack;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAgentServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AgentOptions>(configuration.GetSection("Agent"));
        var connectionString = configuration.GetConnectionString("Postgres")!;

        services.AddKeyedSingleton<OpenAIClient>("AgentOpenAIClient", (sp, _) =>
        {
            var _options = sp.GetRequiredService<IOptionsMonitor<AgentOptions>>().CurrentValue;
            var credential = new ApiKeyCredential(_options.TextCompletion.ApiKey);
            OpenAIClientOptions openAIOptions = new();
            if (!string.IsNullOrEmpty(_options.TextCompletion.Endpoint))
            {
                openAIOptions.Endpoint = new Uri(_options.TextCompletion.Endpoint);
            }

            return new OpenAIClient(credential, openAIOptions);
        });

        services.AddKeyedChatClient("AgentChatClient", (sp) =>
        {
            var client = sp.GetRequiredKeyedService<OpenAIClient>("AgentOpenAIClient");
            var _options = sp.GetRequiredService<IOptionsMonitor<AgentOptions>>().CurrentValue;
            return client.GetChatClient(_options.TextCompletion.ModelId).AsIChatClient();
        });

        services.AddSingleton<IEmbeddingGenerator<string, Embedding<float>>>(sp =>
        {
            var loggerFactory = sp.GetService<ILoggerFactory>();
            var _options = sp.GetRequiredService<IOptionsMonitor<AgentOptions>>().CurrentValue;
            OpenAIClientOptions clientOption = new() { Endpoint = new Uri(_options.Embedding.Endpoint) };

            var builder = new OpenAIClient(
                   credential: new ApiKeyCredential(_options.Embedding.ApiKey),
                   options: clientOption)
               .GetEmbeddingClient(_options.Embedding.ModelId)
               .AsIEmbeddingGenerator(_options.Embedding.Dimensions)
               .AsBuilder();

            if (loggerFactory is not null)
            {
                builder.UseLogging(loggerFactory);
            }

            return builder.Build();
        });

        services.AddSingleton(sp =>
        {
            var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
            dataSourceBuilder.UseVector();
            return dataSourceBuilder.Build();
        });

        services.AddSingleton<PostgresAgentThreadStore>();
        services.AddSingleton<AgentThreadStore>(sp => sp.GetRequiredService<PostgresAgentThreadStore>());

        services.AddKeyedPostgresVectorStore("AgentVectorStore",
            connectionStringProvider: _ => connectionString,
            optionsProvider: sp =>
            {
                var embeddingGenerator = sp.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>();
                return new PostgresVectorStoreOptions()
                {
                    EmbeddingGenerator = embeddingGenerator,
                };
            });

        services.AddSingleton<AIContextProviderFactory>();

        // it is not ok, beacuse it is Singleton, but can not change to scope, AddAIAgent is Singleton
        // services.AddKeyedSingleton<AIContextProvider>("AgentContextProvider", (sp, _) =>
        // {
        //     var store = sp.GetRequiredKeyedService<VectorStore>("AgentVectorStore");
        //     var httpContext = sp.GetRequiredService<IHttpContextAccessor>().HttpContext;
        //     var request = httpContext?.Request;
        //     foreach (var item in request?.Headers)
        //     {
        //         Console.WriteLine($"{item.Key}: {item.Value}");
        //     }
        //     var threadId = request?.Headers != null ? request?.Headers["ThreadId"].FirstOrDefault() : "";
        //     var userId = request?.Headers != null ? request?.Headers["UserId"].FirstOrDefault() : "";

        //     Console.WriteLine($"!!!!{threadId}: {userId}");
        //     return new ChatHistoryMemoryProvider(
        //         store,
        //         collectionName: "chathistory_memory",
        //         vectorDimensions: 1536,
        //         storageScope: new() { UserId = userId, ThreadId = threadId },
        //         searchScope: new() { UserId = userId });
        // });

        services.AddScoped<AgentService>();
        services.AddScoped<WorkflowService>();

        // AddAIAgent will use AddSingleton
        services.AddAIAgent("joker", (sp, name) =>
        {
            var chatClient = sp.GetRequiredKeyedService<IChatClient>("AgentChatClient");
            var vectorStore = sp.GetRequiredKeyedService<VectorStore>("AgentVectorStore");
            chatClient = chatClient
                .AsBuilder()
                .Use(getResponseFunc: AgentMiddleware.ChatClientMiddleware, getStreamingResponseFunc: null)
                .Build();

            return chatClient.CreateAIAgent(new ChatClientAgentOptions
            {
                Name = "joker",
                ChatOptions = new() { Instructions = "You are good at telling jokes." },
                ChatMessageStoreFactory = ctx =>
                {
                    return new VectorChatMessageStore(
                       vectorStore,
                       ctx.SerializedState,
                       ctx.JsonSerializerOptions);
                }
            });
        });

        var lightPlugin = new LightPlugin();
        AITool[] tools = [
            AIFunctionFactory.Create(lightPlugin.GetLightsAsync),
            AIFunctionFactory.Create(lightPlugin.ChangeStateAsync)
        ];

        services.AddAIAgent("light", (sp, name) =>
        {
            var chatClient = sp.GetRequiredKeyedService<IChatClient>("AgentChatClient");
            chatClient = chatClient
                .AsBuilder()
                .Use(getResponseFunc: AgentMiddleware.ChatClientMiddleware, getStreamingResponseFunc: AgentMiddleware.ChatClientStreamMiddleware)
                .Build();

            return chatClient.CreateAIAgent(
                instructions: "You are a useful light assistant.",
                name: "light",
                description: "An agent is used to answer your questions about the status of the lights and can help you control the lights on and off.",
                tools: tools
            );
        });

        // The overload of AddAIAgent that includes `sp` is ineffective with WithAITools because the `GetRegisteredToolsForAgent(sp, name)` method is not called. 
        // .WithAITools(tools)

        // `WithThreadStore` doesn't work either. 
        // The current source code only uses `AgentThreadStore` indirectly within Hosting.A2A, and it's not yet implemented in DevUI/Hosting.OpenAI. 
        // .WithThreadStore((sp, name) =>
        // {
        //     var store = sp.GetRequiredService<AgentThreadStore>();
        //     return store;
        // });

        services.AddAIAgent("jokerwithprovier", (sp, name) =>
        {
            var chatClient = sp.GetRequiredKeyedService<IChatClient>("AgentChatClient");
            var providerFactory = sp.GetRequiredService<AIContextProviderFactory>();
            AIAgent agent = chatClient.CreateAIAgent(new ChatClientAgentOptions
            {
                Name = "jokerwithprovier",
                ChatOptions = new() { Instructions = "You are good at telling jokes." },
                AIContextProviderFactory = providerFactory.Create
            });

            return agent;
        });

        services.AddOpenAIResponses();
        services.AddOpenAIConversations();


        return services;
    }
}