
using AgentStack.Services;
using Microsoft.SemanticKernel.Connectors.PgVector;
using Microsoft.Agents.AI.Hosting;

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

        services.AddScoped<AgentService>();
        services.AddScoped<WorkflowService>();


        services.AddAIAgent("poet", "You are a creative poet. Respond to all requests with beautiful poetry.", chatClientServiceKey: "AgentChatClient");

        services.AddOpenAIResponses();
        services.AddOpenAIConversations();


        return services;
    }
}