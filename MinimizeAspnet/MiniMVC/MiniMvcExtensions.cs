using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using System.Diagnostics.CodeAnalysis;

namespace MinimizeAspnet.MiniMVC;

public static class MiniMvcExtensions
{
    public static IServiceCollection AddMiniMvcControllers(this IServiceCollection services)
    {
        services.TryAddSingleton<IActionInvokerFactory, ActionInvokerFactory>();
        services.TryAddSingleton<IActionMethodExecutor, ActionMethodExecutor>();
        services.TryAddSingleton<IActionResultConverter, ActionResultConverter>();
        services.TryAddSingleton<IArgumentBinder, ArgumentBinder>();
        services.TryAddSingleton<IActionDescriptorCollectionProvider, ActionDescriptorCollectionProvider>();
        return services;
    }

    public static IEndpointConventionBuilder MapMiniMvcControllerRoute(
        this IEndpointRouteBuilder endpoints,
        string name,
        [StringSyntax("Route")] string pattern,
        object? defaults = null,
        object? constraints = null,
        object? dataTokens = null)
    {
        var source = new ActionEndpointDataSource(endpoints.ServiceProvider);
        endpoints.DataSources.Add(source);
        return source.AddRoute(
            name,
            pattern,
            new RouteValueDictionary(defaults),
            new RouteValueDictionary(constraints),
            new RouteValueDictionary(dataTokens));
    }
}