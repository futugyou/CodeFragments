using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore;
using Microsoft.Extensions.FileProviders;

namespace AspnetcoreEx.MiniWebApplication;

public class BootstrapHostBuilder : IHostBuilder
{
    private readonly List<Action<IConfigurationBuilder>> _configureHostConfigurations = new();
    private readonly List<Action<HostBuilderContext, IConfigurationBuilder>> _configureAppConfigurations = new();
    private readonly List<Action<HostBuilderContext, IServiceCollection>> _configureServices = new();
    private readonly List<Action<IHostBuilder>> _others = new();
 
    public IDictionary<object, object> Properties { get; } = new Dictionary<object, object>();
    public HostBuilderContext builderContext;
    
    public IHost Build()
    {
        // ConfigureWebHostDefaults should never call this.
        throw new InvalidOperationException();
    }
 
    public IHostBuilder ConfigureHostConfiguration(Action<IConfigurationBuilder> configureDelegate)
    {
        _configureHostConfigurations.Add(configureDelegate ?? throw new ArgumentNullException(nameof(configureDelegate)));
        return this;
    }
 
    public IHostBuilder ConfigureAppConfiguration(Action<HostBuilderContext, IConfigurationBuilder> configureDelegate)
    {
        _configureAppConfigurations.Add(configureDelegate ?? throw new ArgumentNullException(nameof(configureDelegate)));
        return this;
    }
 
    public IHostBuilder ConfigureServices(Action<HostBuilderContext, IServiceCollection> configureDelegate)
    {
        _configureServices.Add(configureDelegate ?? throw new ArgumentNullException(nameof(configureDelegate)));
        return this;
    }    
 
    public IHostBuilder ConfigureContainer<TContainerBuilder>(Action<HostBuilderContext, TContainerBuilder> configureDelegate)
    {
         _others.Add(builder => builder.ConfigureContainer(configureDelegate));
        return this;
    }
 
    public IHostBuilder UseServiceProviderFactory<TContainerBuilder>(IServiceProviderFactory<TContainerBuilder> factory)
    {
        _others.Add(builder => builder.UseServiceProviderFactory(factory));
        return this;
    }
 
    public IHostBuilder UseServiceProviderFactory<TContainerBuilder>(Func<HostBuilderContext, IServiceProviderFactory<TContainerBuilder>> factory)
    {
        _others.Add(builder => builder.UseServiceProviderFactory(factory));
        return this;
    }
 
    internal void Apply(IHostBuilder hostBuilder, ConfigurationManager configuration, IServiceCollection services, out HostBuilderContext builderContext)
    {
        var hostConfiguration = new ConfigurationManager();
        _configureHostConfigurations.ForEach(it => it(hostConfiguration));

        var environment = new HostingEnviroment()
        {
            ApplicationName = hostConfiguration[HostDefaults.ApplicationKey],
            Environment = hostConfiguration[HostDefaults.EnvironmentKey] ?? Environments.Production,
            ContentRootPath = HostingPathResolver.ResolvePath(hostConfiguration[HostDefaults.ContentRootKey])
        };
        environment.ContentRootFileProvider = new PhysicalFileProvider(environment.ContentRootPath);

        var hostContext = new HostBuilderContext(Properties)
        {
            Configuration = hostConfiguration,
            HostingEnvironment = environment,
        };

        configuration.AddConfiguration(hostConfiguration, true);
        _configureAppConfigurations.ForEach(it => it(hostcon, configuration));
        _configureServices.ForEach(it => it(hostContext, services));

        _others.ForEach(it => it(hostBuilder));

        foreach (var kv in Properties)
        {
            hostBuilder.Properties[kv.Key] = kv.Value;
        }

        builderContext = hostContext;
    }
}