namespace SignalR.Common;

public interface ISignalRNotifier
{
    event Action<string, CloudEvent> ReceivedOnPublishToTopic;

    Task StartAsync();

    Task OnPublish();

    Task OnPublish(string topic);

    Task StopAsync();

}
