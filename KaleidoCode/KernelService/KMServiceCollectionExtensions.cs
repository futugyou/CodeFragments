
using Microsoft.Extensions.AI;
using Microsoft.KernelMemory;
using Microsoft.KernelMemory.SemanticKernel;
using Microsoft.SemanticKernel.TextGeneration;

namespace KaleidoCode.KernelService;

[Experimental("SKEXP0010")]
public static class ServiceCollectionExtensions
{
    internal static IServiceCollection AddKernelMemoryServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SemanticKernelOptions>(configuration.GetSection("SemanticKernel"));
        var sp = services.BuildServiceProvider();
        var config = sp.GetRequiredService<IOptionsMonitor<SemanticKernelOptions>>()!.CurrentValue;
        var textGenerationService = sp.GetService<ITextGenerationService>();
        var embeddingGenerator = sp.GetService<IEmbeddingGenerator<string, Embedding<float>>>();

        SemanticKernelConfig semanticKernelConfig = new();
        services.AddKernelMemory(memoryBuilder =>
        {
            memoryBuilder.WithSimpleVectorDb();
            if (textGenerationService != null)
            {
                memoryBuilder.WithSemanticKernelTextGenerationService(textGenerationService, semanticKernelConfig);
            }
            if (embeddingGenerator != null)
            {
                memoryBuilder.WithSemanticKernelTextEmbeddingGenerationService(embeddingGenerator, semanticKernelConfig);
            }
        });
        return services;
    }
}