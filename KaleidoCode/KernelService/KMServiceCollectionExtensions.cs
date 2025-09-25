
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

        services.AddSingleton<IKernelMemory>(sp =>
        {
            var textGenerationService = sp.GetService<ITextGenerationService>();
            var embeddingGenerator = sp.GetService<IEmbeddingGenerator<string, Embedding<float>>>();
            var semanticKernelConfig = new SemanticKernelConfig();

            var memoryBuilder = new KernelMemoryBuilder(services);
            memoryBuilder.WithSimpleVectorDb();

            if (textGenerationService != null)
            {
                memoryBuilder.WithSemanticKernelTextGenerationService(textGenerationService, semanticKernelConfig);
            }
            if (embeddingGenerator != null)
            {
                memoryBuilder.WithSemanticKernelTextEmbeddingGenerationService(embeddingGenerator, semanticKernelConfig);
            }

            var kernelMemory = memoryBuilder.Build(null);
            return kernelMemory;
        });
        return services;
    }
}