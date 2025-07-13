
using Microsoft.Extensions.AI;
using Microsoft.KernelMemory;
using Microsoft.KernelMemory.AI;
using Microsoft.KernelMemory.Diagnostics;
using Microsoft.KernelMemory.SemanticKernel;
using Embedding = Microsoft.KernelMemory.Embedding;

namespace AspnetcoreEx.KernelService.Internal;

[Experimental("SKEXP0010")]
internal sealed class SKTextEmbeddingGenerator : ITextEmbeddingGenerator
{
    private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator;
    private readonly ITextTokenizer _tokenizer;
    private readonly ILogger<SKTextEmbeddingGenerator> _log;

    /// <inheritdoc />
    public int MaxTokens { get; }

    /// <inheritdoc />
    public int CountTokens(string text) => _tokenizer.CountTokens(text);

    /// <inheritdoc />
    public IReadOnlyList<string> GetTokens(string text) => _tokenizer.GetTokens(text);

    public SKTextEmbeddingGenerator(
        IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
        SemanticKernelConfig config,
        ITextTokenizer? textTokenizer = null,
        ILoggerFactory? loggerFactory = null)
    {
        ArgumentNullExceptionEx.ThrowIfNull(embeddingGenerator, nameof(embeddingGenerator), "Embedding generation service is null");

        _embeddingGenerator = embeddingGenerator;
        MaxTokens = config.MaxTokenTotal;

        _log = (loggerFactory ?? DefaultLogger.Factory).CreateLogger<SKTextEmbeddingGenerator>();

        if (textTokenizer == null)
        {
            textTokenizer = new CL100KTokenizer();
            _log.LogWarning(
                "Tokenizer not specified, will use {FullName}. The token count might be incorrect, causing unexpected errors",
                textTokenizer.GetType().FullName);
        }

        _tokenizer = textTokenizer;
    }

    /// <inheritdoc />
    public async Task<Embedding> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default)
    {
        _log.LogTrace("Generating embedding with SK embedding generator service");
        var embeddings = await _embeddingGenerator.GenerateAsync([text], cancellationToken: cancellationToken);
        if (embeddings == null || embeddings.Count == 0)
        {
            return new Embedding();
        }
        return new Embedding(embeddings[0].Vector);
    }
}
