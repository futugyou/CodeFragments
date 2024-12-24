
using AspnetcoreEx.KernelService.Planners;
using AspnetcoreEx.KernelService.Skills;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.Plugins.Core;
using Microsoft.KernelMemory;

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
            if (config.QdrantPort.HasValue) { builder.Port = config.QdrantPort.Value; }
            c.BaseAddress = builder.Uri;
            if (!string.IsNullOrEmpty(config.QdrantKey))
            {
                c.DefaultRequestHeaders.Add("api-key", config.QdrantKey);
            }
        });

        // httpClientBuilder.AddResilienceHandler(
        //     "CustomPipeline",
        //     static builder =>
        // {
        //     // See: https://www.pollydocs.org/strategies/retry.html
        //     builder.AddRetry(new HttpRetryStrategyOptions
        //     {
        //         // Customize and configure the retry logic.
        //         BackoffType = DelayBackoffType.Exponential,
        //         MaxRetryAttempts = 5,
        //         UseJitter = true
        //     });

        //     // See: https://www.pollydocs.org/strategies/circuit-breaker.html
        //     builder.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
        //     {
        //         // Customize and configure the circuit breaker logic.
        //         SamplingDuration = TimeSpan.FromSeconds(10),
        //         FailureRatio = 0.2,
        //         MinimumThroughput = 3,
        //         ShouldHandle = static args =>
        //         {
        //             return ValueTask.FromResult(args is
        //             {
        //                 Outcome.Result.StatusCode:
        //                     HttpStatusCode.RequestTimeout or
        //                         HttpStatusCode.TooManyRequests
        //             });
        //         }
        //     });

        //     // See: https://www.pollydocs.org/strategies/timeout.html
        //     builder.AddTimeout(TimeSpan.FromSeconds(5));
        // });
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

