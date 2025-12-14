
using Microsoft.Extensions.Hosting;

namespace Microsoft.Extensions.Configuration;

public static class OpenTelemetryHostApplicationBuilderExtensions
{
    /// <summary>
    /// A simplified configuration implementation; for a standard implementation, please refer to Microsoft.Extensions.Configuration.AmazonSSM.
    /// </summary>
    /// <param name="builder">IHostApplicationBuilder</param>
    /// <param name="path">json file path</param>
    /// <param name="optional"></param>
    /// <param name="reloadOnChange"></param>
    /// <returns></returns>
    public static TBuilder AddJsonConfigurationExtensions<TBuilder>(this TBuilder builder, string path, bool optional, bool reloadOnChange) where TBuilder : IHostApplicationBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);
        if (string.IsNullOrEmpty(path))
        {
            throw new ArgumentException("File path must be a non-empty string.");
        }

        builder.Configuration.AddJsonConfigurationExtensions(path, optional, reloadOnChange);
        return builder;
    }
}
