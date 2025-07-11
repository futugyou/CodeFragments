
using AspnetcoreEx.KernelService.Ingestion;
using AspnetcoreEx.KernelService.Planners;
using AspnetcoreEx.KernelService.Skills;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.VectorData;
using Microsoft.KernelMemory;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Plugins.Core;
using ModelContextProtocol.Client;
using OpenAI;
using System.ClientModel;
using System.ClientModel.Primitives;
using System.Collections.Concurrent;

namespace AspnetcoreEx.KernelService;

[Experimental("SKEXP0011")]
public static class KernelServiceExtensions
{
    internal static async Task InitAIData(this WebApplication app)
    {
        // await DataIngestor.IngestDataAsync(app.Services, new PDFDirectorySource("./KernelService/Data"));
        await Task.CompletedTask;
    }

    // The configuration of SemanticKernel in this extension is the focus, and the others are secondary and can be deleted.
    internal static async Task<IServiceCollection> AddKernelServiceServices(this IServiceCollection services, IConfiguration configuration)
    {
        // configuration
        services.Configure<SemanticKernelOptions>(configuration.GetSection("SemanticKernel"));
        var sp = services.BuildServiceProvider();
        var config = sp.GetRequiredService<IOptionsMonitor<SemanticKernelOptions>>()!.CurrentValue;

        // mongo db, it will register `VectorStore` as singleton
        services.AddMongoVectorStore(configuration.GetConnectionString("MongoDb")!, config.VectorStoreName);

        // mcp server
        // Here we use `mcpserver` alone. `SemanticKernel` has more comprehensive examples to explain how to use `SemanticKernel` with `mcpserver`
        services.AddMcpServer().WithToolsFromAssembly().WithHttpTransport();

        // MEAI
        // dotnet new install Microsoft.Extensions.AI.Templates
        // This is just a demonstration. Under the premise of `SemanticKernel`, there is no active use of the interface provided by `MEAI`
        var credential = new ApiKeyCredential(config.Key ?? throw new InvalidOperationException("Missing configuration: GitHubModels:Token. See the README for details."));
        var openAIOptions = new OpenAIClientOptions()
        {
            Endpoint = new Uri(config.Endpoint)
        };

        var ghModelsClient = new OpenAIClient(credential, openAIOptions);
        var chatClient = ghModelsClient.GetChatClient(config.TextCompletion).AsIChatClient();
        if (!string.IsNullOrEmpty(config.Embedding))
        {
            var embeddingGenerator = ghModelsClient.GetEmbeddingClient(config.Embedding).AsIEmbeddingGenerator();
            services.AddEmbeddingGenerator(embeddingGenerator);
            services.AddSingleton<SemanticSearch>();
            services.AddScoped<DataIngestor>();
        }

        services.AddChatClient(chatClient).UseFunctionInvocation().UseLogging();
        services.AddMongoDB<IngestionCacheDbContext>(configuration.GetConnectionString("MongoDb")!, "ingestioncache");

        // Microsoft.SemanticKernel
        var kernelBuilder = services.AddKernel();

        foreach (var item in config.McpServers)
        {
            await kernelBuilder.Plugins.AddMcpFunctionsFromSseServerAsync(item.Key, item.Value);
        }

        //TODO: wait for an anwaer https://github.com/microsoft/semantic-kernel/issues/10842
        var httpClient = new HttpClient(new ResponseInterceptorHandler())
        {
            BaseAddress = new Uri(config.Endpoint),
        };
        kernelBuilder.AddOpenAIChatCompletion(config.TextCompletion, new Uri(config.Endpoint), config.Key, httpClient: httpClient);

        if (!string.IsNullOrEmpty(config.Image))
        {
            kernelBuilder.AddOpenAITextToImage(config.Image, config.Key, httpClient: httpClient);
        }
        if (!string.IsNullOrEmpty(config.Embedding))
        {
            kernelBuilder.AddOpenAIEmbeddingGenerator(config.Embedding, config.Key, endpoint: new Uri(config.Endpoint), httpClient: httpClient);
        }

        kernelBuilder.Plugins.AddFromType<LightPlugin>("Lights");
        kernelBuilder.Plugins.AddFromType<DataGenerationPlugin>("Generator");
        kernelBuilder.Plugins.AddFromType<ConversationSummaryPlugin>();
        kernelBuilder.Plugins.AddFromType<AuthorEmailPlanner>();
        kernelBuilder.Plugins.AddFromType<EmailPlugin>();
        kernelBuilder.Plugins.AddFromType<MathExPlugin>();

        kernelBuilder.Plugins.AddFromPromptDirectory("./KernelService/Skills");

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

        memoryBuilder.WithOpenAITextEmbeddingGeneration(new OpenAIConfig
        {
            EmbeddingModel = config.Embedding,
            Endpoint = config.Endpoint,
            APIKey = config.Key
        });
        memoryBuilder.WithOpenAITextGeneration(new OpenAIConfig
        {
            TextModel = config.TextCompletion,
            Endpoint = config.Endpoint,
            APIKey = config.Key
        });

        services.AddSingleton(sp =>
        {
            return memoryBuilder.Build();
        });

        return services;
    }

    public static IKernelBuilder AddOpenAIEmbeddingGenerator(
            this IKernelBuilder builder,
            string modelId,
            string apiKey,
            string? orgId = null,
            int? dimensions = null,
            string? serviceId = null,
            Uri? endpoint = null,
            HttpClient? httpClient = null)
    {

        builder.Services.AddOpenAIEmbeddingGenerator(
            modelId,
            apiKey,
            orgId,
            dimensions,
            serviceId,
            endpoint,
            httpClient);

        return builder;
    }
    public static IServiceCollection AddOpenAIEmbeddingGenerator(
        this IServiceCollection services,
        string modelId,
        string apiKey,
        string? orgId = null,
        int? dimensions = null,
        string? serviceId = null,
        Uri? endpoint = null,
        HttpClient? httpClient = null,
        string? openTelemetrySourceName = null,
        Action<OpenTelemetryEmbeddingGenerator<string, Embedding<float>>>? openTelemetryConfig = null)
    {
        return services.AddKeyedSingleton<IEmbeddingGenerator<string, Embedding<float>>>(serviceId, (serviceProvider, _) =>
        {
            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();

            var builder = new OpenAIClient(
                   credential: new ApiKeyCredential(apiKey),
                   options: GetOpenAIClientOptions(httpClient: httpClient, endpoint: endpoint, orgId: orgId))
               .GetEmbeddingClient(modelId)
               .AsIEmbeddingGenerator(dimensions)
               .AsBuilder()
               .UseOpenTelemetry(loggerFactory, openTelemetrySourceName, openTelemetryConfig);

            if (loggerFactory is not null)
            {
                builder.UseLogging(loggerFactory);
            }

            return builder.Build();
        });
    }

    internal static OpenAIClientOptions GetOpenAIClientOptions(HttpClient? httpClient, Uri? endpoint = null, string? orgId = null)
    {
        OpenAIClientOptions options = new()
        {
            UserAgentApplicationId = "Semantic-Kernel",
        };

        if (endpoint is not null)
        {
            options.Endpoint = endpoint;
        }

        options.AddPolicy(CreateRequestHeaderPolicy("Semantic-Kernel-Version", GetAssemblyVersion(typeof(OpenAIFunctionParameter))), PipelinePosition.PerCall);

        if (orgId is not null)
        {
            options.OrganizationId = orgId;
        }

        if (httpClient is not null)
        {
            options.Transport = new HttpClientPipelineTransport(httpClient);
            options.RetryPolicy = new ClientRetryPolicy(maxRetries: 0); // Disable retry policy if and only if a custom HttpClient is provided.
            options.NetworkTimeout = Timeout.InfiniteTimeSpan; // Disable default timeout
        }

        return options;
    }

    static GenericActionPipelinePolicy CreateRequestHeaderPolicy(string headerName, string headerValue)
    {
        return new GenericActionPipelinePolicy((message) =>
        {
            if (message?.Request?.Headers?.TryGetValue(headerName, out string? _) == false)
            {
                message.Request.Headers.Set(headerName, headerValue);
            }
        });
    }
    public static string GetAssemblyVersion(Type type)
    {
        return type.Assembly.GetName().Version!.ToString();
    }
}

internal sealed class GenericActionPipelinePolicy : PipelinePolicy
{
    private readonly Action<PipelineMessage> _processMessageAction;

    internal GenericActionPipelinePolicy(Action<PipelineMessage> processMessageAction)
    {
        this._processMessageAction = processMessageAction;
    }

    public override void Process(PipelineMessage message, IReadOnlyList<PipelinePolicy> pipeline, int currentIndex)
    {
        this._processMessageAction(message);
        if (currentIndex < pipeline.Count - 1)
        {
            pipeline[currentIndex + 1].Process(message, pipeline, currentIndex + 1);
        }
    }

    public override async ValueTask ProcessAsync(PipelineMessage message, IReadOnlyList<PipelinePolicy> pipeline, int currentIndex)
    {
        this._processMessageAction(message);
        if (currentIndex < pipeline.Count - 1)
        {
            await pipeline[currentIndex + 1].ProcessAsync(message, pipeline, currentIndex + 1).ConfigureAwait(false);
        }
    }
}