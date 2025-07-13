

using System.ClientModel;
using System.ClientModel.Primitives;
using AspnetcoreEx.KernelService.Internal;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Google;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.TextGeneration;
using OpenAI;

namespace AspnetcoreEx.KernelService;

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

    public static IServiceCollection AddOpenAIEmbeddingGenerator(
        this IServiceCollection services,
        string modelId,
        string apiKey,
        string? orgId = null,
        int? dimensions = null,
        string? serviceId = null,
        Uri? endpoint = null,
        HttpClient? httpClient = null,
        string? openTelemetrySourceName = null,
        Action<OpenTelemetryEmbeddingGenerator<string, Embedding<float>>>? openTelemetryConfig = null)
    {
        return services.AddKeyedSingleton(serviceId, (serviceProvider, _) =>
        {
            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();

            var builder = new OpenAIClient(
                   credential: new ApiKeyCredential(apiKey),
                   options: GetOpenAIClientOptions(httpClient: HttpClientProvider.GetHttpClient(httpClient, serviceProvider), endpoint: endpoint, orgId: orgId))
               .GetEmbeddingClient(modelId)
               .AsIEmbeddingGenerator(dimensions)
               .AsBuilder()
               .UseOpenTelemetry(loggerFactory, openTelemetrySourceName, openTelemetryConfig);

            if (loggerFactory is not null)
            {
                builder.UseLogging(loggerFactory);
            }

            return builder.Build();
        });
    }

    internal static OpenAIClientOptions GetOpenAIClientOptions(HttpClient? httpClient, Uri? endpoint = null, string? orgId = null)
    {
        OpenAIClientOptions options = new()
        {
            UserAgentApplicationId = "Semantic-Kernel",
        };

        if (endpoint is not null)
        {
            options.Endpoint = endpoint;
        }

        options.AddPolicy(GenericActionPipelinePolicy.CreateRequestHeaderPolicy("Semantic-Kernel-Version", GetAssemblyVersion(typeof(OpenAIFunctionParameter))), PipelinePosition.PerCall);

        if (orgId is not null)
        {
            options.OrganizationId = orgId;
        }

        if (httpClient is not null)
        {
            options.Transport = new HttpClientPipelineTransport(httpClient);
            options.RetryPolicy = new ClientRetryPolicy(maxRetries: 0); // Disable retry policy if and only if a custom HttpClient is provided.
            options.NetworkTimeout = Timeout.InfiniteTimeSpan; // Disable default timeout
        }

        return options;
    }

    public static string GetAssemblyVersion(Type type)
    {
        return type.Assembly.GetName().Version!.ToString();
    }
}