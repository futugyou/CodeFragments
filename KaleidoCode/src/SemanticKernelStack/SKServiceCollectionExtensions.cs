
using SemanticKernelStack.Ingestion;
using SemanticKernelStack.Internal;
using SemanticKernelStack.Skills;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Plugins.Core;
using OpenAI;
using System.ClientModel;
using System.ClientModel.Primitives;
using Microsoft.AspNetCore.Builder;
using SemanticKernelStack.Services;

namespace SemanticKernelStack;

[Experimental("SKEXP0011")]
public static class SKServiceCollectionExtensions
{
    public static async Task InitAIData(this WebApplication app)
    {
        // await DataIngestor.IngestDataAsync(app.Services, new PDFDirectorySource("./KernelService/Data"));
        await Task.CompletedTask;
    }

    // The configuration of SemanticKernel in this extension is the focus, and the others are secondary and can be deleted.
    public static async Task<IServiceCollection> AddKernelServiceServices(this IServiceCollection services, IConfiguration configuration)
    {
        // services.AddDuckduckgoTextSearch();

        // configuration
        services.Configure<SemanticKernelOptions>(configuration.GetSection("SemanticKernel"));
        var config = configuration.GetSection("SemanticKernel").Get<SemanticKernelOptions>() ?? new();

        // mongo db, it will register `VectorStore` as singleton
        // TODO: Currently, Microsoft.SemanticKernel.Connectors.MongoDB can only use MongoDB.Driver 2.30.0, 3.X.X will report an error, 
        // but all my other libraries that depend on MongoDB are 3.3.0 and above.
        switch (config.VectorStoreType)
        {
            case "memory":
                services.AddInMemoryVectorStore();
                services.AddInMemoryVectorStoreRecordCollection<string, SemanticSearchRecord>(SemanticSearchRecord.GetCollectionName());
                break;
            case "mongo":
                services.AddMongoVectorStore(configuration.GetConnectionString("MongoDb")!, config.VectorStoreName);
                services.AddMongoCollection<SemanticSearchRecord>(SemanticSearchRecord.GetCollectionName());
                break;
            case "postgres":
                services.AddPostgresVectorStore(configuration.GetConnectionString("Postgres")!);
                services.AddPostgresCollection<string, SemanticSearchRecord>(SemanticSearchRecord.GetCollectionName());
                break;
            default:
                throw new NotImplementedException();
        }

        services.AddMongoDB<IngestionCacheDbContext>(configuration.GetConnectionString("MongoDb")!, config.VectorStoreName);

        // mcp server
        // Here we use `mcpserver` alone. `SemanticKernel` has more comprehensive examples to explain how to use `SemanticKernel` with `mcpserver`
        services.AddMcpServer().WithToolsFromAssembly().WithHttpTransport();

        // MEAI
        // dotnet new install Microsoft.Extensions.AI.Templates
        // This is just a demonstration. Under the premise of `SemanticKernel`, there is no active use of the interface provided by `MEAI`
        // Use `TextCompletion` config, because `TextCompletion` is the most commonly used model
        if (!string.IsNullOrEmpty(config.TextCompletion.ModelId))
        {
            var credential = new ApiKeyCredential(config.TextCompletion.ApiKey);
            OpenAIClientOptions openAIOptions = new();
            if (!string.IsNullOrEmpty(config.TextCompletion.Endpoint))
            {
                openAIOptions.Endpoint = new Uri(config.TextCompletion.Endpoint);
            }

            var ghModelsClient = new OpenAIClient(credential, openAIOptions);
            var chatClient = ghModelsClient.GetChatClient(config.TextCompletion.ModelId).AsIChatClient();
            services.AddChatClient(chatClient).UseFunctionInvocation().UseLogging();
        }

        // Microsoft.SemanticKernel
        var kernelBuilder = services.AddKernel();

        foreach (var item in config.McpServers)
        {
            await kernelBuilder.Plugins.AddMcpFunctionsFromSseServerAsync(item.Key, item.Value);
        }

        services.AddSingleton<IAutoFunctionInvocationFilter, ToolCallIdFilter>();
        services.AddSingleton<IFunctionInvocationFilter, ToolCallIdFilter>();

        if (!string.IsNullOrEmpty(config.TextCompletion.ModelId))
        {
            // https://github.com/microsoft/semantic-kernel/issues/10842
            var httpClient = new HttpClient(new ResponseInterceptorHandler())
            {
                BaseAddress = new Uri(config.TextCompletion.Endpoint),
            };
            if (config.TextCompletion.Provider == "google")
            {
                kernelBuilder.AddGoogleAIGeminiChatCompletion(config.TextCompletion.ModelId, config.TextCompletion.ApiKey, new Uri(config.TextCompletion.Endpoint), httpClient: httpClient);
            }
            if (config.TextCompletion.Provider == "openai")
            {
                kernelBuilder.AddOpenAIChatCompletion(config.TextCompletion.ModelId, new Uri(config.TextCompletion.Endpoint), config.TextCompletion.ApiKey, httpClient: httpClient);
            }
        }

        if (!string.IsNullOrEmpty(config.Image.ModelId))
        {
            kernelBuilder.AddOpenAITextToImage(config.Image.ModelId, config.Image.ApiKey);
        }

        if (!string.IsNullOrEmpty(config.Embedding.ModelId))
        {
            if (config.Embedding.Provider == "google")
            {
                kernelBuilder.AddGoogleAIEmbeddingGenerator(config.Embedding.ModelId, config.Embedding.ApiKey, dimensions: config.Embedding.Dimensions);
            }
            else
            {
                kernelBuilder.AddOpenAIEmbeddingGenerator(config.Embedding.ModelId, config.Embedding.ApiKey, config.Embedding.Endpoint, dimensions: config.Embedding.Dimensions);
            }

            services.AddSingleton<SemanticSearch>();
            services.AddScoped<DataIngestor>();
        }

        services.AddSingleton<IEmailService, EmailService>();

        #region Plugins
        // It is best to register the `plugin` in each usage scenario, which can greatly reduce the judgment options of `LLM`.
        // This is just a demo, for convenience
        kernelBuilder.Plugins.AddFromType<LightPlugin>("Lights");
        kernelBuilder.Plugins.AddFromType<DataGenerationPlugin>("Generator");
        kernelBuilder.Plugins.AddFromType<ConversationSummaryPlugin>();
        // `AuthorEmailPlanner` will cause llm to call it instead of `EmailPlugin` in `EmailSender`
        // kernelBuilder.Plugins.AddFromType<AuthorEmailPlanner>();
        kernelBuilder.Plugins.AddFromType<EmailPlugin>();
        kernelBuilder.Plugins.AddFromType<MathExPlugin>();
        kernelBuilder.Plugins.AddFromType<WebSearch>("WebSearch");

        // TODO
        // Unhandled exception. System.TypeLoadException: Could not load type 'Microsoft.OpenApi.Interfaces.IOpenApiReferenceable' from assembly 'Microsoft.OpenApi, Version=2.3.0.0, Culture=neutral, PublicKeyToken=3f5743946376f042'.
        // KernelPlugin infrProplugin = await OpenApiKernelPluginFactory.CreateFromOpenApiAsync(
        //   pluginName: "InfrastructureProject",
        //   filePath: "Resources/infr-project.yaml",
        //   executionParameters: new OpenApiFunctionExecutionParameters()
        //   {
        //       EnablePayloadNamespacing = true
        //   }
        // );
        // kernelBuilder.Plugins.Add(infrProplugin);

        kernelBuilder.Plugins.AddFromPromptDirectory("../SemanticKernelStack/Skills");
        #endregion

        #region Agent
        services.AddSingleton<ArtDirectorAgentFactory>();
        services.AddSingleton<CopyWriterAgentFactory>();
        services.AddTransient<ArtReviewerAgentChat>();
        services.AddKeyedSingleton<ChatCompletionAgent>(
            "CopyWriter",
            (sp, key) =>
            {
                var factory = sp.GetRequiredService<CopyWriterAgentFactory>();
                return factory.Create();
            });
        services.AddKeyedSingleton<ChatCompletionAgent>(
            "ArtDirector",
            (sp, key) =>
            {
                var factory = sp.GetRequiredService<ArtDirectorAgentFactory>();
                return factory.Create();
            });
        #endregion

        services.AddScoped<A2AService>();
        services.AddScoped<AgentService>();
        services.AddScoped<DeclarativeService>();
        services.AddScoped<EmbeddingService>();
        services.AddScoped<PluginsService>();
        services.AddScoped<ProcessService>();
        services.AddScoped<PromptService>();
        services.AddScoped<TokenCounterService>();

        return services;
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
        return services.AddKeyedSingleton(serviceId, (serviceProvider, _) =>
        {
            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();

            var builder = new OpenAIClient(
                   credential: new ApiKeyCredential(apiKey),
                   options: GetOpenAIClientOptions(httpClient: HttpClientProvider.GetHttpClient(httpClient, serviceProvider), endpoint: endpoint, orgId: orgId))
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

    private static OpenAIClientOptions GetOpenAIClientOptions(HttpClient? httpClient, Uri? endpoint = null, string? orgId = null)
    {
        OpenAIClientOptions options = new()
        {
            UserAgentApplicationId = "Semantic-Kernel",
        };

        if (endpoint is not null)
        {
            options.Endpoint = endpoint;
        }

        options.AddPolicy(GenericActionPipelinePolicy.CreateRequestHeaderPolicy("Semantic-Kernel-Version", GetAssemblyVersion(typeof(OpenAIFunctionParameter))), PipelinePosition.PerCall);

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

    private static string GetAssemblyVersion(Type type)
    {
        return type.Assembly.GetName().Version!.ToString();
    }
}
