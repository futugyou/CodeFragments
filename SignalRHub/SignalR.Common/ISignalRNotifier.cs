namespace SignalR.Common;

public interface ISignalRNotifier
{
    event Action<string, string> ReceivedOnPublishToTopic;

    Task StartAsync();

    Task OnPublish();

    Task OnPublish(string topic);

    Task StopAsync();

}
