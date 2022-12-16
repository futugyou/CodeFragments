using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Builder;

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
                    app.Run(_application.BuildRequestDelegate())))
            .ConfigureHostConfiguration(config =>
            {
                if (args?.Any())
                {
                    config.AddCommandLine(args);
                }
                Dictionary<string,string>? settings = new ();
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
                if (options.WebRootPath is not null)
                {
                    settings[HostDefaults.WebRootKey] = options.WebRootPath;
                }
                config.AddInMemoryCollection(settings);
            });

        bootstrap.Apply(_hostBuilder, Configuration, Services, out var builderContext);

        if (options.Args?.Any())
        {
            Configuration.AddCommandLine(options.Args);
        }

        var webHostContext = (WebHostBuilderContext)builderContext.Properties[typeof(WebHostBuilderContext)];
        Environment = webHostContext.HostingEnvironment;
        Host = new ConfigureHostBuilder(webHostContext, Configuration, Services);
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