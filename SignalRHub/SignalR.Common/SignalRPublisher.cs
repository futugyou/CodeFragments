namespace SignalR.Common;

/// <summary>
/// call hub mothed from client
/// </summary>
public class SignalRPublisher : ISignalRPublisher
{
    private readonly IHubConnectionInstance _hubConnectionInstance;

    public SignalRPublisher(IHubConnectionInstance hubConnectionInstance)
    {
        _hubConnectionInstance = hubConnectionInstance;
    }

    /// <summary>
    /// 
    /// </summary>
    /// this will call <see cref="LogCornerHub<T>.PublishToTopic"/>
    /// <typeparam name="T"></typeparam>
    /// <param name="topic"></param>
    /// <param name="payload"></param>
    /// <returns></returns>
    public async Task PublishAsync<T>(string topic, T payload)
    {
        if (_hubConnectionInstance.Connection.State != HubConnectionState.Connected)
        {
            await _hubConnectionInstance.StartAsync();
        }

        await _hubConnectionInstance.Connection.InvokeAsync(nameof(IHubInvoker<object>.PublishToTopic), topic, payload);
    }

    /// <summary>
    /// 
    /// </summary>
    /// this will call <see cref="LogCornerHub<T>.Subscribe"/>
    /// <param name="topic"></param>
    /// <returns></returns>
    public async Task SubscribeAsync(string topic)
    {
        if (_hubConnectionInstance.Connection.State != HubConnectionState.Connected)
        {
            await _hubConnectionInstance.StartAsync();
        }

        await _hubConnectionInstance.Connection.InvokeAsync(nameof(IHubInvoker<string>.Subscribe), topic);
    }
}
