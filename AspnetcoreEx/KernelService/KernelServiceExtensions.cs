
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
        
        return services;
    }
}

