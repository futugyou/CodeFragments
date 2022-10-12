namespace SignalR.Common;

public class SignalRNotifier : ISignalRNotifier
{
    private readonly IHubConnectionInstance _hubConnectionInstance;

    public event Action<CloudEvent> ReceivedOnPublish;
    public event Action<string, string> ReceivedOnPublishToTopic;

    public SignalRNotifier(IHubConnectionInstance hubConnectionInstance)
    {
        _hubConnectionInstance = hubConnectionInstance;
    }

    public async Task OnPublish()
    {
        // hub call client
        _hubConnectionInstance.Connection.On<CloudEvent>(nameof(IHubNotifier<CloudEvent>.OnPublish), u => ReceivedOnPublish?.Invoke(u));
        await Task.CompletedTask;
    }

    public async Task OnPublish(string topic)
    {
        // hub call client
        _hubConnectionInstance.Connection.On<string, string>(nameof(IHubNotifier<string>.OnPublish), (u, v) => ReceivedOnPublishToTopic?.Invoke(u, v));
        await Task.CompletedTask;
    }

    public async Task StartAsync()
    {
        await _hubConnectionInstance.Connection.StartAsync();
    }

    public async Task StopAsync()
    {
        await _hubConnectionInstance.Connection.StopAsync();
    }
}
