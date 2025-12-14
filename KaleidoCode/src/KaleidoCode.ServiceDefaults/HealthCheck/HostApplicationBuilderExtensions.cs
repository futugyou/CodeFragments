
using Microsoft.Extensions.Hosting;

namespace Microsoft.Extensions.DependencyInjection;

public static class HealthCheckHostApplicationBuilderExtensions
{
    public static TBuilder AddHealthCheckExtensions<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddHealthCheckExtensions(builder.Configuration);

        return builder;
    }
}
