
using Microsoft.SemanticKernel.Data;

namespace SemanticKernelStack.Duckduckgo;

public static class DockServiceCollectionExtensions
{
    public static IServiceCollection AddDuckduckgoTextSearch(
        this IServiceCollection services,
        DuckTextSearchOptions? options = null,
        string? serviceId = default)
    {

        services.AddKeyedSingleton<ITextSearch>(
            serviceId,
            (sp, obj) =>
            {
                var selectedOptions = options ?? sp.GetService<DuckTextSearchOptions>();

                return new DuckduckgoTextSearch(selectedOptions);
            });

        return services;
    }
}