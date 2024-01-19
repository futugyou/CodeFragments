
using AspnetcoreEx.KernelService.Planners;
using AspnetcoreEx.KernelService.Skills;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.Plugins.Core;

namespace AspnetcoreEx.KernelService;


[Experimental("SKEXP0011")]
public static class KernelServiceExtensions
{
    internal static IServiceCollection AddKernelServiceServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SemanticKernelOptions>(configuration.GetSection("SemanticKernel"));
        var sp = services.BuildServiceProvider();
        var config = sp.GetRequiredService<IOptionsMonitor<SemanticKernelOptions>>()!.CurrentValue;
        var kernelBuilder = services.AddKernel();
        if (!string.IsNullOrWhiteSpace(config.Endpoint))
        {
            kernelBuilder.AddAzureOpenAIChatCompletion(config.ChatModel, config.Endpoint, config.Key);
            kernelBuilder.AddAzureOpenAITextEmbeddingGeneration(config.Embedding, config.Endpoint, config.Key);
            kernelBuilder.AddAzureOpenAITextGeneration(config.TextCompletion, config.Endpoint, config.Key);
            kernelBuilder.AddAzureOpenAITextToImage(config.Image, config.Endpoint, config.Key);
        }
        else
        {
            kernelBuilder.AddOpenAIChatCompletion(config.ChatModel, config.Key);
            kernelBuilder.AddOpenAITextEmbeddingGeneration(config.Embedding, config.Key);
            kernelBuilder.AddOpenAITextGeneration(config.TextCompletion, config.Key);
            kernelBuilder.AddOpenAITextToImage(config.Key);
        }

        kernelBuilder.Plugins.AddFromType<LightPlugin>();
        kernelBuilder.Plugins.AddFromType<ConversationSummaryPlugin>();
        kernelBuilder.Plugins.AddFromType<AuthorEmailPlanner>();
        kernelBuilder.Plugins.AddFromType<EmailPlugin>();
        kernelBuilder.Plugins.AddFromType<MathExPlugin>();
        kernelBuilder.Plugins.AddFromType<MathSolver>();

        services.AddHttpClient("qdrant", c =>
        {
            UriBuilder builder = new(config.QdrantHost);
            if (config.QdrantPort.HasValue) { builder.Port = config.QdrantPort.Value; }
            c.BaseAddress = builder.Uri;
            if (!string.IsNullOrEmpty(config.QdrantKey))
            {
                c.DefaultRequestHeaders.Add("api-key", config.QdrantKey);
            }
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
                memoryBuilder.WithAzureOpenAITextEmbeddingGeneration(config.Embedding, config.Endpoint, config.Key);
            }
            else
            {
                memoryBuilder.WithOpenAITextEmbeddingGeneration(config.Embedding, config.Key);
            }

            return memoryBuilder.Build();
        });

        return services;
    }
}

