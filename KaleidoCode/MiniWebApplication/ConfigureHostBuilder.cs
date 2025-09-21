namespace KaleidoCode.MiniWebApplication;

public class ConfigureHostBuilder : IHostBuilder
{
    private readonly ConfigurationManager _configuration;
    private readonly IServiceCollection _services;
    private readonly HostBuilderContext _context;
    private readonly List<Action<IHostBuilder>> _configureActions = new();

    internal ConfigureHostBuilder(
        HostBuilderContext context,
        ConfigurationManager configuration,
        IServiceCollection services)
    {
        _configuration = configuration;
        _services = services;
        _context = context;
    }

    public IDictionary<object, object> Properties => _context.Properties;

    IHost IHostBuilder.Build()
    {
        throw new NotSupportedException($"Call {nameof(WebApplicationBuilder)}.{nameof(WebApplicationBuilder.Build)}() instead.");
    }

    private IHostBuilder Configure(Action configure)
    {
        configure();
        return this;
    }

    public IHostBuilder ConfigureAppConfiguration(Action<HostBuilderContext, IConfigurationBuilder> configureDelegate)
    {
        return Configure(() => configureDelegate(_context, _configuration));
    }

    /// <inheritdoc />
    public IHostBuilder ConfigureContainer<TContainerBuilder>(Action<HostBuilderContext, TContainerBuilder> configureDelegate)
    {
        return Configure(() => _configureActions.Add(b => b.ConfigureContainer(configureDelegate)));
    }

    /// <inheritdoc />
    public IHostBuilder ConfigureHostConfiguration(Action<IConfigurationBuilder> configureDelegate)
    {
        var applicationName = _configuration[HostDefaults.ApplicationKey] ?? "";
        var contentRoot = _context.HostingEnvironment.ContentRootPath ?? "";
        var environment = _configuration[HostDefaults.EnvironmentKey] ?? "";

        configureDelegate(_configuration);

        Validate(applicationName, HostDefaults.ApplicationKey, "applicationName can not be change");
        Validate(contentRoot, HostDefaults.ContentRootKey, "contentRoot can not be change");
        Validate(environment, HostDefaults.EnvironmentKey, "environment can not be change");

        return this;

        void Validate(string previousValue, string key, string message)
        {
            if (!string.Equals(previousValue, _configuration[key], StringComparison.OrdinalIgnoreCase))
            {
                throw new NotSupportedException(message);
            }
        }
    }

    /// <inheritdoc />
    public IHostBuilder ConfigureServices(Action<HostBuilderContext, IServiceCollection> configureDelegate)
    {
        return Configure(() => configureDelegate(_context, _services));
    }

    public IHostBuilder UseServiceProviderFactory<TContainerBuilder>(IServiceProviderFactory<TContainerBuilder> factory) where TContainerBuilder : notnull
    {
        return Configure(() => _configureActions.Add(b => b.UseServiceProviderFactory(factory)));
    }

    public IHostBuilder UseServiceProviderFactory<TContainerBuilder>(Func<HostBuilderContext, IServiceProviderFactory<TContainerBuilder>> factory) where TContainerBuilder : notnull
    {
        return Configure(() => _configureActions.Add(b => b.UseServiceProviderFactory(factory)));
    }

    internal void Apply(IHostBuilder hostBuilder)
    {
        _configureActions.ForEach(op => op(hostBuilder));
    }
}