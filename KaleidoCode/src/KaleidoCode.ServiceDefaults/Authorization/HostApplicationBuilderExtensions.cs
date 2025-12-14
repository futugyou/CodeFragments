
using Microsoft.Extensions.Hosting;

namespace Microsoft.Extensions.DependencyInjection;

public static class AuthorizationHostApplicationBuilderExtensions
{
    public static TBuilder AddAuthorizationExtension<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddAuthorizationExtension(builder.Configuration);

        return builder;
    }
}
