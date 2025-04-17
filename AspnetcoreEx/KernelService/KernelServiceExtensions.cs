
using AspnetcoreEx.KernelService.Ingestion;
using AspnetcoreEx.KernelService.Planners;
using AspnetcoreEx.KernelService.Skills;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.VectorData;
using Microsoft.KernelMemory;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.Plugins.Core;
using ModelContextProtocol;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol.Transport;
using OpenAI;
using Qdrant.Client;
using System.ClientModel;
using System.Collections.Concurrent;

namespace AspnetcoreEx.KernelService;

[Experimental("SKEXP0011")]
public static class KernelServiceExtensions
{
    internal static async Task InitAIData(this WebApplication app)
    {
        // await DataIngestor.IngestDataAsync(app.Services, new PDFDirectorySource("./KernelService/Data"));
    }

    internal static async Task<IServiceCollection> AddKernelServiceServices(this IServiceCollection services, IConfiguration configuration)
    {
        // configuration
        services.Configure<SemanticKernelOptions>(configuration.GetSection("SemanticKernel"));
        var sp = services.BuildServiceProvider();
        var config = sp.GetRequiredService<IOptionsMonitor<SemanticKernelOptions>>()!.CurrentValue;

        // mcp server
        services.AddMcpServer().WithToolsFromAssembly().WithHttpTransport();

        // dotnet new install Microsoft.Extensions.AI.Templates
        var credential = new ApiKeyCredential(config.Key ?? throw new InvalidOperationException("Missing configuration: GitHubModels:Token. See the README for details."));
        var openAIOptions = new OpenAIClientOptions()
        {
            Endpoint = new Uri(config.Endpoint)
        };

        var ghModelsClient = new OpenAIClient(credential, openAIOptions);
        var chatClient = ghModelsClient.AsChatClient(config.ChatModel);
        var embeddingGenerator = ghModelsClient.AsEmbeddingGenerator(config.Embedding);

        services.AddSingleton<IVectorStore>(sp =>
        {
            var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
            return new QdrantVectorStore(new QdrantClient(host: config.QdrantHost, port: config.QdrantPort, apiKey: config.QdrantKey, loggerFactory: loggerFactory));
        });

        services.AddChatClient(chatClient).UseFunctionInvocation().UseLogging();
        services.AddEmbeddingGenerator(embeddingGenerator);
        services.AddSingleton<SemanticSearch>();
        services.AddScoped<DataIngestor>();
        services.AddMongoDB<IngestionCacheDbContext>(configuration.GetConnectionString("MongoDb")!, "ingestioncache");

        // Microsoft.SemanticKernel
        var kernelBuilder = services.AddKernel();

        foreach (var item in config.McpServers)
        {
            await kernelBuilder.Plugins.AddMcpFunctionsFromSseServerAsync(item.Key, item.Value);
        }

        if (!string.IsNullOrWhiteSpace(config.Endpoint))
        {
            kernelBuilder.AddAzureOpenAIChatCompletion(config.ChatModel, config.Endpoint, config.Key);
            kernelBuilder.AddAzureOpenAITextEmbeddingGeneration(config.Embedding, config.Endpoint, config.Key);
            kernelBuilder.AddAzureOpenAITextToImage(config.Image, config.Endpoint, config.Key);
        }
        else
        {
            kernelBuilder.AddOpenAIChatCompletion(config.ChatModel, config.Key);
            kernelBuilder.AddOpenAITextEmbeddingGeneration(config.Embedding, config.Key);
            kernelBuilder.AddOpenAITextToImage(config.Key);
        }

        kernelBuilder.Plugins.AddFromType<LightPlugin>();
        kernelBuilder.Plugins.AddFromType<ConversationSummaryPlugin>();
        kernelBuilder.Plugins.AddFromType<AuthorEmailPlanner>();
        kernelBuilder.Plugins.AddFromType<EmailPlugin>();
        kernelBuilder.Plugins.AddFromType<MathExPlugin>();
        kernelBuilder.Plugins.AddFromType<MathSolver>();

        kernelBuilder.Plugins.AddFromPromptDirectory("./KernelService/Skills");

        IHttpClientBuilder httpClientBuilder = services.AddHttpClient("qdrant", c =>
        {
            UriBuilder builder = new(config.QdrantHost);
            builder.Port = config.QdrantPort;
            c.BaseAddress = builder.Uri;
            if (!string.IsNullOrEmpty(config.QdrantKey))
            {
                c.DefaultRequestHeaders.Add("api-key", config.QdrantKey);
            }
        });

        httpClientBuilder.AddResilienceHandler(
            "AdvancedPipeline",
            static (ResiliencePipelineBuilder<HttpResponseMessage> builder, ResilienceHandlerContext context) =>
            {
                // Enable reloads whenever the named options change
                context.EnableReloads<HttpRetryStrategyOptions>("RetryOptions");

                // Retrieve the named options
                var retryOptions = context.GetOptions<HttpRetryStrategyOptions>("RetryOptions");

                // Add retries using the resolved options
                builder.AddRetry(retryOptions);
            });

        services.AddSingleton<IQdrantVectorDbClient>(sp =>
        {
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient("qdrant");
            var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
            return new QdrantVectorDbClient(httpClient, config.QdrantVectorSize, null, loggerFactory);
        });

        services.AddSingleton<IMemoryStore>(sp =>
        {
            var qdrantVectorDbClient = sp.GetRequiredService<IQdrantVectorDbClient>();
            return new QdrantMemoryStore(qdrantVectorDbClient);
        });

        services.AddScoped<ISemanticTextMemory>(sp =>
        {
            var store = sp.GetRequiredService<IMemoryStore>();
            var memoryBuilder = new MemoryBuilder();
            memoryBuilder.WithMemoryStore(store);
            if (!string.IsNullOrWhiteSpace(config.Endpoint))
            {
                // memoryBuilder.WithAzureOpenAITextEmbeddingGeneration(config.Embedding, config.Endpoint, config.Key);
            }
            else
            {
                memoryBuilder.WithOpenAITextEmbeddingGeneration(config.Embedding, config.Key);
            }

            return memoryBuilder.Build();
        });

        return services;
    }

    private static readonly ConcurrentDictionary<string, IKernelBuilderPlugins> SseMap = new();

    /// <summary>
    /// Creates a Model Content Protocol plugin from an SSE server that contains the specified MCP functions and adds it into the plugin collection.
    /// </summary>
    /// <param name="endpoint"></param>
    /// <param name="serverName"></param>
    /// <param name="cancellationToken">The optional <see cref="CancellationToken"/>.</param>
    /// <param name="plugins"></param>
    /// <returns>A <see cref="KernelPlugin"/> containing the functions.</returns>
    public static async Task<IKernelBuilderPlugins> AddMcpFunctionsFromSseServerAsync(this IKernelBuilderPlugins plugins,
        string name, McpServer server, CancellationToken cancellationToken = default)
    {
        var key = PluginNameSanitizer.ToSafePluginName(name);

        if (SseMap.TryGetValue(key, out var sseKernelPlugin))
        {
            return sseKernelPlugin;
        }

        var mcpClient = await GetClientAsync(name, server, cancellationToken).ConfigureAwait(false);
        var functions = await mcpClient.MapToFunctionsAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

        cancellationToken.Register(() => mcpClient.DisposeAsync().ConfigureAwait(false).GetAwaiter().GetResult());

        sseKernelPlugin = plugins.AddFromFunctions(key, functions);
        return SseMap[key] = sseKernelPlugin;
    }

    private static async Task<IMcpClient> GetClientAsync(string name, McpServer mcpServer, CancellationToken cancellationToken)
    {
        IClientTransport clientTransport;
        if (!string.IsNullOrEmpty(mcpServer.Url))
        {
            clientTransport = new SseClientTransport(new()
            {
                Name = name,
                Endpoint = new Uri(mcpServer.Url),
                ConnectionTimeout = TimeSpan.FromSeconds(30),
            });
        }
        else
        {
            clientTransport = new StdioClientTransport(new()
            {
                Name = name,
                Command = mcpServer.Command,
                Arguments = mcpServer.Args,
                EnvironmentVariables = mcpServer.Env,
            });
        }

        var transportType = clientTransport.GetType().Name;
        McpClientOptions options = new()
        {
            ClientInfo = new()
            {
                Name = $"{name} {transportType}Client",
                Version = "1.0.0"
            }
        };

        return await McpClientFactory.CreateAsync(clientTransport, options, NullLoggerFactory.Instance, cancellationToken);
    }

    internal static IServiceCollection AddKernelMemoryServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SemanticKernelOptions>(configuration.GetSection("SemanticKernel"));
        var sp = services.BuildServiceProvider();
        var config = sp.GetRequiredService<IOptionsMonitor<SemanticKernelOptions>>()!.CurrentValue;
        var memoryBuilder = new KernelMemoryBuilder().WithSimpleVectorDb();

        if (!string.IsNullOrWhiteSpace(config.Endpoint))
        {
            memoryBuilder.WithAzureOpenAITextEmbeddingGeneration(new AzureOpenAIConfig
            {
                Deployment = config.Embedding,
                Endpoint = config.Endpoint,
                Auth = AzureOpenAIConfig.AuthTypes.APIKey,
                APIType = AzureOpenAIConfig.APITypes.EmbeddingGeneration,
                APIKey = config.Key
            });
            memoryBuilder.WithAzureOpenAITextGeneration(new AzureOpenAIConfig
            {
                Deployment = config.TextCompletion,
                Endpoint = config.Endpoint,
                Auth = AzureOpenAIConfig.AuthTypes.APIKey,
                APIType = AzureOpenAIConfig.APITypes.EmbeddingGeneration,
                APIKey = config.Key
            });
        }
        else
        {
            // memoryBuilder.WithOpenAITextEmbeddingGeneration(new OpenAIConfig
            // {
            //     EmbeddingModel = config.Embedding,
            //     APIKey = config.Key,
            // });
            // Remove the call to WithOpenAITextGeneration as it does not exist
        }

        services.AddSingleton(sp =>
        {
            return memoryBuilder.Build();
        });


        return services;
    }
}

