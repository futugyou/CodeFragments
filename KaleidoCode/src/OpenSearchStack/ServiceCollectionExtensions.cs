
namespace OpenSearchStack;

public static class ElasticClientExtensions
{
    public static IServiceCollection AddElasticClientExtension(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<ElasticOptions>(configuration.GetSection("ElasticServer"));

        var elasticOptions = configuration.GetSection("ElasticServer").Get<ElasticOptions>() ?? new ElasticOptions();

        if (elasticOptions.Uris == null || elasticOptions.Uris.Length == 0)
        {
            throw new ArgumentNullException(nameof(configuration), "ElasticServer:Uris can not be null or empty");
        }

        // var connectionPool = new SniffingConnectionPool(elasticOptions.UriList);
        var settings = new ConnectionSettings(elasticOptions.UriList[0]);
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

        if (!string.IsNullOrEmpty(elasticOptions.Username) && !string.IsNullOrEmpty(elasticOptions.Password))
        {
            settings.BasicAuthentication(elasticOptions.Username, elasticOptions.Password);
        }
        if (!string.IsNullOrEmpty(elasticOptions.CertificatePath))
        {
            settings.ClientCertificate(elasticOptions.CertificatePath);
        }
        settings.DefaultMappingFor<Company>(m => m
            .IdProperty(p => p.Name)
        );
        // TODO: can not find WithPropertyMappingProvider
        //settings.WithPropertyMappingProvider(new CustomPropertyMappingProvider());
#if DEBUG
        settings.EnableDebugMode(apiCallDetails =>
        {

        });
        //EnableDebugMode include all
        // settings.PrettyJson(true);
        // settings.IncludeServerStackTraceOnError();
        // settings.DisableDirectStreaming();
        // settings.EnableTcpStats();
        // settings.EnableThreadPoolStats();
#endif
        var list = new List<string>();
        settings.OnRequestCompleted(apiCallDetails =>
        {
            // log out the request and the request body, if one exists for the type of request
            if (apiCallDetails.RequestBodyInBytes != null)
            {
                list.Add(
                    $"{apiCallDetails.HttpMethod} {apiCallDetails.Uri} " +
                    $"{Encoding.UTF8.GetString(apiCallDetails.RequestBodyInBytes)}");
            }
            else
            {
                list.Add($"{apiCallDetails.HttpMethod} {apiCallDetails.Uri}");
            }

            // log out the response and the response body, if one exists for the type of response
            if (apiCallDetails.ResponseBodyInBytes != null)
            {
                list.Add($"Status: {apiCallDetails.HttpStatusCode}" +
                         $"{Encoding.UTF8.GetString(apiCallDetails.ResponseBodyInBytes)}");
            }
            else
            {
                list.Add($"Status: {apiCallDetails.HttpStatusCode}");
            }
        });
        settings.OnRequestDataCreated(requestData =>
        {
            //requestData.RequestMetaData
        });

        settings.EnableHttpCompression();
        var listenerObserver = new ElasticListenerObserver();
        DiagnosticListener.AllListeners.Subscribe(listenerObserver);
        services.AddSingleton(new OpenSearchClient(settings));
        services.AddSingleton<IndexService>();
        services.AddSingleton<PipelineService>();
        services.AddSingleton<InsertService>();
        services.AddSingleton<ReindexService>();
        services.AddSingleton<AnalyzerService>();
        services.AddSingleton<TestAnalyzerService>();
        services.AddSingleton<SearchService>();
        services.AddSingleton<AggregationSerice>();
        return services;
    }
}