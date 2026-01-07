
using KernelMemoryStack.Services;
using Microsoft.Extensions.Options;
using System.ClientModel;
using Microsoft.KernelMemory;
using Microsoft.KernelMemory.SemanticKernel;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.TextGeneration;
using OpenAI;
using Microsoft.Extensions.AI;

namespace KernelMemoryStack;

[Experimental("SKEXP0010")]
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddKernelMemoryServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<KernelMemoryOptions>(configuration.GetSection("KernelMemory"));
        var config = configuration.GetSection("KernelMemory").Get<KernelMemoryOptions>() ?? new();

        services.AddOpenAIChatCompletion(config.TextCompletion.ModelId, new Uri(config.TextCompletion.Endpoint), config.TextCompletion.ApiKey);

        services.AddSingleton<IKernelMemory>(sp =>
        {
            var loggerFactory = sp.GetService<ILoggerFactory>();
            var config = sp.GetRequiredService<IOptionsMonitor<KernelMemoryOptions>>().CurrentValue;

            var clientOption = new OpenAIClientOptions();
            clientOption.Endpoint = new Uri(config.Embedding.Endpoint);
            var openaiClient = new OpenAIClient(
                   credential: new ApiKeyCredential(config.Embedding.ApiKey),
                   options: clientOption);

            var embeddingBuilder = openaiClient
               .GetEmbeddingClient(config.Embedding.ModelId)
               .AsIEmbeddingGenerator(config.Embedding.Dimensions)
               .AsBuilder();

            if (loggerFactory is not null)
            {
                embeddingBuilder.UseLogging(loggerFactory);
            }

            var embeddingGenerator = embeddingBuilder.Build();

            var textGenerationService = sp.GetRequiredService<ITextGenerationService>();
            
            var semanticKernelConfig = new SemanticKernelConfig();

            var memoryBuilder = new KernelMemoryBuilder(services);
            memoryBuilder.WithSimpleVectorDb();
            memoryBuilder.WithSemanticKernelTextGenerationService(textGenerationService, semanticKernelConfig);
            memoryBuilder.WithSemanticKernelTextEmbeddingGenerationService(embeddingGenerator, semanticKernelConfig);

            var kernelMemory = memoryBuilder.Build(null);

            return kernelMemory;
        });

        services.AddScoped<WebImportService>();

        return services;
    }
}