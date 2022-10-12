using SignalR.Common;

namespace SignalR.Client;

public class ClientService : IClientService
{
    private readonly ISignalRNotifier _notifier;
    private readonly ISignalRPublisher _publisher;

    public ClientService(ISignalRNotifier notifier, ISignalRPublisher publisher)
    {
        _notifier = notifier;
        _publisher = publisher;

        _notifier.ReceivedOnPublishToTopic += (topic, @event) =>
        {
            Console.WriteLine("topic: " + topic);
            Console.WriteLine("@event: " + @event);
        };
    }

    public async Task StartAsync()
    {
        await _notifier.StartAsync();
    }

    public async Task StopAsync()
    {
        await _notifier.StopAsync();
    }

    public async Task DoWorkAsync()
    {
        await _publisher.SubscribeAsync("thisistopic");

        await _notifier.OnPublish("thisistopic");
    }
}
