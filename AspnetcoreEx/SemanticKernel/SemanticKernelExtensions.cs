using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI.TextEmbedding;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.Services;
using Microsoft.SemanticKernel.SkillDefinition;
using Microsoft.SemanticKernel.TemplateEngine;
using System.Net.Http;

namespace AspnetcoreEx.SemanticKernel;
public static class SemanticKernelExtensions
{
    internal static IServiceCollection AddSemanticKernelServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SemanticKernelOptions>(configuration.GetSection("SemanticKernel"));
        services.AddScoped<KernelConfig>(sp => new KernelConfig());
        //services.AddScoped<ISemanticTextMemory>(sp => NullMemory.Instance);
        services.AddSingleton<IPromptTemplateEngine, PromptTemplateEngine>();
        services.AddSingleton<IMemoryStore, VolatileMemoryStore>();
        services.AddScoped<ISemanticTextMemory>(sp =>
        {
            var store = sp.GetRequiredService<IMemoryStore>();
            var logger = sp.GetRequiredService<ILogger<Program>>();
            var op = sp.GetRequiredService<IOptionsMonitor<SemanticKernelOptions>>()!.CurrentValue;

            return new SemanticTextMemory(store,
                new OpenAITextEmbeddingGeneration(
                    "text-davinci-003",
                    op.Key,
                    httpClient: null,
                    logger: logger
                    )
                );
        });

        services.AddScoped<ISkillCollection, SkillCollection>();
        services.AddScoped<IKernel, Kernel>();

        return services;
    }
}

