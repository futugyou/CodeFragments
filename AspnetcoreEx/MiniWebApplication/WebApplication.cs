using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Hosting.Server;

namespace AspnetcoreEx.MiniWebApplication;

public class WebApplication : IApplicationBuilder, IHost, IEndpointRouteBuilder
{
    private readonly IHost _host;
    private readonly ApplicationBuilder _app;
    private readonly List<EndpointDataSource> _dataSources = new();

    public WebApplication(IHost host)
    {
        _host = host;
        _app = new ApplicationBuilder(host.Services);
    }

    IServiceProvider IHost.Services => _host.Services;
    Task IHost.StartAsync(CancellationToken cancellationToken) => _host.StartAsync(cancellationToken);
    Task IHost.StopAsync(CancellationToken cancellationToken) => _host.StopAsync(cancellationToken);
    ICollection<EndpointDataSource> IEndpointRouteBuilder.DataSources => _dataSources;
    IServiceProvider IEndpointRouteBuilder.ServiceProvider => _app.ApplicationServices;
    IApplicationBuilder IEndpointRouteBuilder.CreateApplicationBuilder() => _app.New();

    IServiceProvider IApplicationBuilder.ApplicationServices
    {
        get => _app.ApplicationServices;
        set => _app.ApplicationServices = value;
    }

    IFeatureCollection IApplicationBuilder.ServerFeatures => _app.ServerFeatures;
    IDictionary<string, object?> IApplicationBuilder.Properties => _app.Properties;
    RequestDelegate IApplicationBuilder.Build() => _app.Build();
    IApplicationBuilder IApplicationBuilder.New() => _app.New();
    IApplicationBuilder IApplicationBuilder.Use(Func<RequestDelegate, RequestDelegate> middleware) => _app.Use(middleware);

    void IDisposable.Dispose() => _host.Dispose();
    public IServiceProvider Services => _host.Services;

    internal RequestDelegate BuildRequestDelegate() => _app.Build();

    public ICollection<string> Urls => _host.Services.GetRequiredService<IServer>()
        .Features.Get<IServerAddressesFeature>()?.Addresses ?? throw new InvalidOperationException("IServerAddressesFeature is not found.");

    public Task RunAsync(string? url = null)
    {
        Listen(url);
        return HostingAbstractionsHostExtensions.RunAsync(this);
    }

    public void Run(string url)
    {
        Listen(url);
        HostingAbstractionsHostExtensions.Run(this);
    }

    private void Listen(string? url)
    {
        if (url is not null)
        {
            var addresses = _host.Services.GetRequiredService<IServer>()
                .Features.Get<IServerAddressesFeature>()?.Addresses ?? throw new InvalidOperationException("IServerAddressesFeature is not found.");
            addresses.Clear();
            addresses.Add(url);
        }
    }

    public static WebApplicationBuilder CreateBuilder()
    {
        return new WebApplicationBuilder(new WebApplicationOptions());
    }

    public static WebApplicationBuilder CreateBuilder(string[] args)
    {
        var options = new WebApplicationOptions()
        {
            Args = args
        };
        return new WebApplicationBuilder(options);
    }

    public static WebApplicationBuilder CreateBuilder(WebApplicationOptions options)
    {
        return new WebApplicationBuilder(options);
    }

    public static WebApplication Create(string[]? args = null)
    {
        var options = new WebApplicationOptions()
        {
            Args = args
        };
        return new WebApplicationBuilder(options).Build();
    }

    public static void StartMiniWebApplication()
    {
        var app = Create();
        app.Run(httpContext => httpContext.Response.WriteAsync("Hello Mini"));
        app.Run();
    }
}