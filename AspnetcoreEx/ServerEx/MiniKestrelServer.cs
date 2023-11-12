using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http.Features;

namespace AspnetcoreEx.ServerEx;

public class MiniKestrelServer : IServer
{
    private readonly KestrelServerOptions _options;
    private readonly IConnectionListenerFactory _factory;
    private readonly List<IConnectionListener> _listeners = new();

    public IFeatureCollection Features { get; } = new FeatureCollection();

    public MiniKestrelServer(IOptions<KestrelServerOptions> options, IConnectionListenerFactory factory)
    {
        _options = options.Value;
        _factory = factory;
        Features.Set<IServerAddressesFeature>(new ServerAddressesFeature());
    }

    public void Dispose() => StopAsync(CancellationToken.None).GetAwaiter().GetResult();
    public Task StopAsync(CancellationToken cancellationToken) => Task.WhenAll(_listeners.Select(it => it.DisposeAsync().AsTask()));

    public Task StartAsync<TContext>(IHttpApplication<TContext> application, CancellationToken cancellationToken) where TContext : notnull
    {
        var feature = Features.Get<IServerAddressesFeature>();

        if (feature == null)
        {
            return Task.CompletedTask;
        }

        IEnumerable<ListenOptions> listenOptions;
        if (feature.PreferHostingUrls)
        {
            listenOptions = BuildListenOptions(feature);
        }
        else
        {
            listenOptions = _options.GetListenOptions();
            if (!listenOptions.Any())
            {
                listenOptions = BuildListenOptions(feature);
            }
        }

        foreach (var options in listenOptions)
        {
            _ = StartAsync(options);
        }

        return Task.CompletedTask;

        async Task StartAsync(ListenOptions listenOptions)
        {
            var listener = await _factory.BindAsync(listenOptions.EndPoint, cancellationToken);
            _listeners.Add(listener);
            var hostedApplication = new HostedApplication<TContext>(application);
            var pipeline = listenOptions.Use(next => context => hostedApplication.OnConnectedAsync(context)).Build();
            while (true)
            {
                var connection = await listener.AcceptAsync();
                if (connection != null)
                {
                    _ = pipeline(connection);
                }
            }
        }

        IEnumerable<ListenOptions> BuildListenOptions(IServerAddressesFeature feature)
        {
            var options = new KestrelServerOptions();

            foreach (var address in feature.Addresses)
            {
                var url = new Uri(address);
                if (string.Compare("localhost", url.Host, true) == 0)
                {
                    options.ListenLocalhost(url.Port);
                }
                else
                {
                    options.Listen(IPAddress.Parse(url.Host), url.Port);
                }
            }
            return options.GetListenOptions();
        }
    }
}