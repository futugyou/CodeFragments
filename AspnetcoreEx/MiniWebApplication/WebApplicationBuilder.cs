namespace AspnetcoreEx.MiniWebApplication;

public class WebApplicationBuilder
{
    private readonly HostBuilder _hostBuilder = new HostBuilder();
    private WebApplication _application;

    public ConfigurationManager Configuration { get; } = new ConfigurationManager();
    public IServiceCollection Services { get; } = new ServiceCollection();
    public IWebHostEnvironment Environment { get; }
    public ConfigureHostBuilder Host { get; }
    public ConfigureWebHostBuilder WebHost { get; }
    public ILoggingBuilder Logging { get; }

    public WebApplicationBuilder(WebApplicationOptions options)
    {
        var args = options.Args;
        var bootstrap = new BootstrapHostBuilder();
        bootstrap
            .ConfigureDefaults(null)
            .ConfigureWebHostDefaults(webHostBuilder =>
                webHostBuilder.Configure(app =>
                {
                    if (_application != null && _application is IEndpointRouteBuilder routeBuilder)
                    {
                        var hasEndpoints = routeBuilder.DataSources.Count != 0;

                        if (hasEndpoints && !app.Properties.ContainsKey("__EndpointRouteBuilder"))
                        {
                            app.UseRouting();
                            app.Properties["__EndpointRouteBuilder"] = _application;
                        }

                        app.Run(_application.BuildRequestDelegate());

                        if (hasEndpoints)
                        {
                            app.UseEndpoints(_ => { });
                        }
                    }
                }))
            .ConfigureHostConfiguration(config =>
            {
                if (args != null && args.Length != 0)
                {
                    config.AddCommandLine(args);
                }

                Dictionary<string, string?>? settings = new();
                if (options.EnvironmentName is not null)
                {
                    settings[HostDefaults.EnvironmentKey] = options.EnvironmentName;
                }
                if (options.ApplicationName is not null)
                {
                    settings[HostDefaults.ApplicationKey] = options.ApplicationName;
                }
                if (options.ContentRootPath is not null)
                {
                    settings[HostDefaults.ContentRootKey] = options.ContentRootPath;
                }
                //if (options.WebRootPath is not null)
                //{
                //    settings[HostDefaults.WebRootKey] = options.WebRootPath;
                //}
                config.AddInMemoryCollection(settings);
            });

        bootstrap.Apply(_hostBuilder, Configuration, Services, out var builderContext);

        if (options.Args != null && options.Args.Length != 0)
        {
            Configuration.AddCommandLine(options.Args);
        }

        var webHostContext = (WebHostBuilderContext)builderContext.Properties[typeof(WebHostBuilderContext)];
        Environment = webHostContext.HostingEnvironment;
        Host = new ConfigureHostBuilder(builderContext, Configuration, Services);
        WebHost = new ConfigureWebHostBuilder(webHostContext, Configuration, Services);
        Logging = new LoggingBuilder(Services);
    }

    public WebApplication Build()
    {
        _hostBuilder.ConfigureAppConfiguration(builder =>
        {
            builder.AddConfiguration(Configuration);
            foreach (var kv in ((IConfigurationBuilder)Configuration).Properties)
            {
                builder.Properties[kv.Key] = kv.Value;
            }
        });

        _hostBuilder.ConfigureServices((_, services) =>
        {
            foreach (var service in Services)
            {
                services.Add(service);
            }
        });

        Host.Apply(_hostBuilder);

        return _application = new WebApplication(_hostBuilder.Build());
    }
}

public class LoggingBuilder(IServiceCollection services) : ILoggingBuilder
{
    public IServiceCollection Services { get; } = services;
}