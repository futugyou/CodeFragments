
using KaleidoCode.KernelService.Internal;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Google;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.TextGeneration;

namespace KaleidoCode.KernelService;

[Experimental("SKEXP0010")]
public static class KernelBuilderExtensions
{
    public static IKernelBuilder AddGoogleAIGeminiChatCompletion(
        this IKernelBuilder builder,
        string modelId,
        string apiKey,
        Uri endpoint,
        GoogleAIVersion apiVersion = GoogleAIVersion.V1_Beta, // todo: change beta to stable when stable version will be available
        string? serviceId = null,
        HttpClient? httpClient = null)
    {

        OpenAIChatCompletionService Factory(IServiceProvider serviceProvider, object? _) =>
                    new(modelId: modelId,
                        apiKey: apiKey,
                        endpoint: endpoint,
                        httpClient: HttpClientProvider.GetHttpClient(httpClient, serviceProvider),
                        loggerFactory: serviceProvider.GetService<ILoggerFactory>());

        builder.Services.AddKeyedSingleton<IChatCompletionService>(serviceId, (serviceProvider, _) =>
            new GoogleAIGeminiChatCompletionService(
                modelId: modelId,
                apiKey: apiKey,
                apiVersion: apiVersion,
                httpClient: HttpClientProvider.GetHttpClient(httpClient, serviceProvider),
                loggerFactory: serviceProvider.GetService<ILoggerFactory>()));

        builder.Services.AddKeyedSingleton<ITextGenerationService>(serviceId, (Func<IServiceProvider, object?, OpenAIChatCompletionService>)Factory);
        return builder;
    }

    public static IKernelBuilder AddOpenAIEmbeddingGenerator(
             this IKernelBuilder builder,
             string modelId,
             string apiKey,
             string? orgId = null,
             int? dimensions = null,
             string? serviceId = null,
             Uri? endpoint = null,
             HttpClient? httpClient = null)
    {

        builder.Services.AddOpenAIEmbeddingGenerator(
            modelId,
            apiKey,
            orgId,
            dimensions,
            serviceId,
            endpoint,
            httpClient);

        return builder;
    }


}