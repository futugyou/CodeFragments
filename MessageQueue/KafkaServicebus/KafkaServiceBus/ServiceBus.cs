using Confluent.Kafka;

namespace KafkaServiceBus;

public class ServiceBus : IServiceBus
{
    private readonly IProducer<Null, string> _producer;
    private readonly IConsumer<Null, string> _consumer;

    public ServiceBus(IProducer<Null, string> producer, IConsumer<Null, string> consumer)
    {
        _producer = producer;
        _consumer = consumer;
    }

    public Task ReceiveMessage(string topic, CancellationToken cancellation, bool forever = true)
    {
        //_consumer.Subscribe(topic);
        _consumer.Assign(new TopicPartitionOffset(topic, 0, Offset.Beginning));

        do
        {
            if (cancellation.IsCancellationRequested)
            {
                break;
            }

            var result = _consumer.Consume(cancellation);

            Console.WriteLine($"Key = {result.Message.Key}");
            Console.WriteLine($"Value = {result.Message.Value}");
            Console.WriteLine($"Offset = {result.Offset.Value}");
            Console.WriteLine($"Partition = {result.Partition.Value}");
        } while (forever);

        return Task.CompletedTask;
    }

    public async Task SendMessage(string message, string topic)
    {
        var result = await _producer.ProduceAsync(topic, new Message<Null, string> { Value = message });

        Console.WriteLine($"Offset = {result.Offset}");
    }
}
