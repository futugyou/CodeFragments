
using Microsoft.Extensions.Options;
using Microsoft.KernelMemory;
using Microsoft.KernelMemory.SemanticKernel;
using Microsoft.SemanticKernel.TextGeneration;

namespace KernelMemoryStack;

[Experimental("SKEXP0010")]
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddKernelMemoryServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<KernelMemoryOptions>(configuration.GetSection("KernelMemory"));

        services.AddSingleton<IKernelMemory>(sp =>
        {
            var textGenerationService = sp.GetService<ITextGenerationService>();
            var config = sp.GetService<IOptionsMonitor<KernelMemoryOptions>>()!.CurrentValue!;
            var embeddingGenerator = sp.GetService<IEmbeddingGenerator<string, Embedding<float>>>();
            var semanticKernelConfig = new SemanticKernelConfig();

            var memoryBuilder = new KernelMemoryBuilder(services);
            memoryBuilder.WithSimpleVectorDb();

            if (textGenerationService != null)
            {
                memoryBuilder.WithSemanticKernelTextGenerationService(textGenerationService, semanticKernelConfig);
            }
            else
            {
                // TODO: 
            }

            if (embeddingGenerator != null)
            {
                memoryBuilder.WithSemanticKernelTextEmbeddingGenerationService(embeddingGenerator, semanticKernelConfig);
            }
            else
            {
                // TODO:
            }

            var kernelMemory = memoryBuilder.Build(null);
            return kernelMemory;
        });
        return services;
    }
}