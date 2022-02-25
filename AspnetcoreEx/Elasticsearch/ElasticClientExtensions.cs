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

        // services.AddOptions<ElasticOptions>().Bind(configuration.GetSection("ElasticServer"));
        // services.Configure<ElasticOptions>(configuration.GetSection("ElasticServer"));

        var elasticOptions = configuration.GetSection("ElasticServer").Get<ElasticOptions>() ?? new ElasticOptions();

        if (elasticOptions.Uris == null || !elasticOptions.Uris.Any())
        {
            throw new ArgumentNullException("ElasticServer:Uris");
        }

        var connectionPool = new SniffingConnectionPool(elasticOptions.UriList);
        var settings = new ConnectionSettings(connectionPool);
        if (!string.IsNullOrEmpty(elasticOptions.DefaultIndex))
        {
            settings.DefaultIndex(elasticOptions.DefaultIndex);
        }
        if (elasticOptions.ConnectionLimit > 0)
        {
            settings.ConnectionLimit(elasticOptions.ConnectionLimit);
        }
        if (!string.IsNullOrEmpty(elasticOptions.ApiID) && !string.IsNullOrEmpty(elasticOptions.ApiKey))
        {
            settings.ApiKeyAuthentication(elasticOptions.ApiID, elasticOptions.ApiKey);
        }
        if (!string.IsNullOrEmpty(elasticOptions.Base64EncodedApiKey))
        {
            settings.ApiKeyAuthentication(new ApiKeyAuthenticationCredentials(elasticOptions.Base64EncodedApiKey));
        }
        if (!string.IsNullOrEmpty(elasticOptions.Username) && !string.IsNullOrEmpty(elasticOptions.Password))
        {
            settings.BasicAuthentication(elasticOptions.Username, elasticOptions.Password);
        }
        if (!string.IsNullOrEmpty(elasticOptions.CertificatePath))
        {
            settings.ClientCertificate(elasticOptions.CertificatePath);
        }
#if DEBUG
        settings.EnableDebugMode();
        settings.PrettyJson(true);
#endif
        services.AddSingleton<ElasticClient>(new ElasticClient(settings));
        services.AddSingleton<IndexService>();
        services.AddSingleton<PipelineService>();
        services.AddSingleton<InsertService>();
        services.AddSingleton<ReindexService>();
        services.AddSingleton<AnalyzerService>();
        services.AddSingleton<TestAnalyzerService>();
        services.AddSingleton<SearchService>();
        services.AddSingleton<AggregationSerice>();
        services.AddSingleton<BaseElasticService>();
        return services;
    }
}