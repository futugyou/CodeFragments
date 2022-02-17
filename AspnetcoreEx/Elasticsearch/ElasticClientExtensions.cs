using Elasticsearch.Net;
using Nest;

namespace AspnetcoreEx.Elasticsearch;

public static class ElasticClientExtensions
{
    public static IServiceCollection AddElasticClientExtension(this IServiceCollection services)
    {
        if (services is null)
        {
            throw new ArgumentNullException(nameof(services));
        }
        var configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
        return services.AddElasticClientExtension(configuration);
    }

    public static IServiceCollection AddElasticClientExtension(this IServiceCollection services, IConfiguration configuration)
    {
        if (services is null)
        {
            throw new ArgumentNullException(nameof(services));
        }
        if (configuration is null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        var urilist = configuration.GetValue<string[]>("ElasticServer:Uris");
        if (urilist is null)
        {
            throw new ArgumentNullException("ElasticServer:Uris");
        }

        var uris = urilist.Select(p => new Uri(p)).ToList();
        var connectionPool = new SniffingConnectionPool(uris);

        var settings = new ConnectionSettings(connectionPool);
        var defaultIndex = configuration["ElasticServer:DefaultIndex"];
        if (!string.IsNullOrEmpty(defaultIndex))
        {
            settings.DefaultIndex(defaultIndex);
        }
        var client = new ElasticClient(settings);
        services.AddSingleton<ElasticClient>(_ => client);
        services.AddSingleton<EsService>();
        return services;
    }
}