using Microsoft.Extensions.Options;

namespace SignalR.Common;

public class HubConnectionInstance : IHubConnectionInstance
{
    private readonly IOptionsMonitor<SignalROption> _options;

    public HubConnection Connection { get; private set; }

    public HubConnectionInstance(IOptionsMonitor<SignalROption> options)
    {
        _options = options;
    }

    public async Task InitAsync()
    {
        var url = _options.CurrentValue.Url;
        Connection = new HubConnectionBuilder()
            .WithUrl(url)
            .Build();
        await Task.CompletedTask;
    }

    public async Task StartAsync()
    {
        await Connection.StartAsync();
    }
}
