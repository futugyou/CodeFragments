
using AspnetcoreEx.KernelService.Internal;
using Microsoft.Extensions.AI;
using Microsoft.KernelMemory;
using Microsoft.KernelMemory.AI;
using Microsoft.KernelMemory.SemanticKernel;

namespace AspnetcoreEx.KernelService;

[Experimental("SKEXP0010")]
public static class KernelMemoryBuilderExtensions
{
    public static IKernelMemoryBuilder WithSemanticKernelTextEmbeddingGenerationService(
        this IKernelMemoryBuilder builder,
        IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
        SemanticKernelConfig config,
        ITextTokenizer? textTokenizer = null,
        ILoggerFactory? loggerFactory = null,
        bool onlyForRetrieval = false)
    {
        if (embeddingGenerator == null) { throw new ConfigurationException("Memory Builder: the semantic kernel text embedding generator instance is NULL"); }

        var generator = new SKTextEmbeddingGenerator(embeddingGenerator, config, textTokenizer, loggerFactory);
        builder.AddSingleton<ITextEmbeddingGenerator>(generator);

        if (!onlyForRetrieval)
        {
            builder.AddIngestionEmbeddingGenerator(generator);
        }

        return builder;
    }
}
