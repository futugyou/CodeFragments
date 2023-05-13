using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.SkillDefinition;
using Microsoft.SemanticKernel.TemplateEngine;

namespace AspnetcoreEx.SemanticKernel;
public static class SemanticKernelExtensions
{
    internal static IServiceCollection AddSemanticKernelServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SemanticKernelOptions>(configuration.GetSection("SemanticKernel"));
        services.AddScoped<KernelConfig>(sp => new KernelConfig());
        services.AddScoped<ISemanticTextMemory>(sp => NullMemory.Instance);
        services.AddSingleton<IPromptTemplateEngine, PromptTemplateEngine>();
        services.AddScoped<ISkillCollection, SkillCollection>();
        services.AddScoped<IKernel, Kernel>();

        return services;
    }
}

