
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Extensions.Configuration;

public static class ConfigurationBuilderExtensions
{
    /// <summary>
    /// A simplified configuration implementation; for a standard implementation, please refer to Microsoft.Extensions.Configuration.AmazonSSM.
    /// </summary>
    /// <param name="builder">IConfigurationBuilder</param>
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

        var source = new SimpleJsonConfigurationSource
        {
            FileProvider = null,
            Path = path,
            Optional = optional,
            ReloadOnChange = reloadOnChange
        };

        source.ResolveFileProvider();
        builder.Configuration.Add(source);
        return builder;
    }
}
