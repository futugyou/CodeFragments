// See https://aka.ms/new-console-template for more information
using Confluent.Kafka;
using KafkaServiceBus;
using Microsoft.Extensions.DependencyInjection;

Console.WriteLine("Hello, World!");

var services = new ServiceCollection();
services.AddSingleton<IServiceBus, ServiceBus>();
services.AddSingleton<IProducer<Null, string>>(x =>
{
    var producerConfig = new ProducerConfig
    {
        BootstrapServers = "192.168.0.6:9092"
    };
    var producer = new ProducerBuilder<Null, string>(producerConfig).Build();
    return producer;
}
);
services.AddSingleton<IConsumer<Null, string>>(x =>
{
    var consumerConfig = new ConsumerConfig
    {
        BootstrapServers = "192.168.0.6:9092",
        GroupId = "test-consumer-group",
        AutoOffsetReset = AutoOffsetReset.Earliest,
        AllowAutoCreateTopics = true,
    };
    var consumer = new ConsumerBuilder<Null, string>(consumerConfig).Build();
    return consumer;
}
);
var sp = services.BuildServiceProvider();
var bus = sp.GetRequiredService<IServiceBus>();
await bus.SendMessage("thisismessage", "thisistopic");

Console.ReadLine();