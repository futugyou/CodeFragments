namespace KafkaServiceBus;

public interface IServiceBus
{
    Task SendMessage(string message, string topic);
    Task ReceiveMessage(string topic, CancellationToken cancellation, bool forever = true);
}
